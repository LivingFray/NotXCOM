using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

    TileController[,] heightmap;

    public int width = 20;
    public int height = 20;

    public GameObject tile;

    public float baseTileHeight = 0.1f;

    public Material baseMaterial;
    public Material darkMaterial;
    public Material startMaterial;
    public Material endMaterial;

    public Material checkingMaterial;

    private Vector2Int rayStart;
    private GameObject tileStart;

    private Vector2Int rayEnd;
    private GameObject tileEnd;

    private bool placingStart = true;

    public GameObject line;

    public bool clearLOS = true;

    public float maxTraversableCover = 0.5f;

    public bool HasLineOfSightDDA(Vector2Int start, Vector2Int end, out float cover) {
        //Iterate from start position to end position, checking for blocking cover
        //TODO: Allow for stepping out
        //Full cover between blocks completely
        //Half cover between doesn't block but increases cover value
        //No cover between causes flanks

        //TODO: Fix 45 degree being dependant on direction
        line.GetComponent<LineRenderer>().SetPosition(0, new Vector3(start.x, 1.0f, start.y));
        line.GetComponent<LineRenderer>().SetPosition(1, new Vector3(end.x, 1.0f, end.y));
        //Get Ray direction
        float dirX = end.x - start.x;
        float dirY = end.y - start.y;

        int stepX = dirX >= 0 ? 1 : -1;
        int stepY = dirY >= 0 ? 1 : -1;

        float deltaX = stepX / dirX;
        float deltaY = stepY / dirY;

        int gridX = start.x;
        int gridY = start.y;

        float maxX = ((gridX + Mathf.Max(0, stepX)) - (start.x + 0.5f)) / dirX;
        float maxY = ((gridY + Mathf.Max(0, stepY)) - (start.y + 0.5f)) / dirY;

        bool stillSearching = true;

        cover = 0;

        int oldGridX;
        int oldGridY;

        while (stillSearching) {
            oldGridX = gridX;
            oldGridY = gridY;
            //Which face of the tile was passed through
            //0 = NegX, 1 = PosX, 2 = NegZ, 3 = PosZ
            int faceCurrent;
            int faceNext;
            if (maxX < maxY) {
                //Change in X
                if (stepX < 0) {
                    faceCurrent = 0;
                    faceNext = 1;
                } else {
                    faceCurrent = 1;
                    faceNext = 0;
                }
                maxX += deltaX;
                gridX += stepX;
            } else {
                if (stepY < 0) {
                    faceCurrent = 2;
                    faceNext = 3;
                } else {
                    faceCurrent = 3;
                    faceNext = 2;
                }
                maxY += deltaY;
                gridY += stepY;
            }
            //Update current tile
            UpdateEdges(new Vector2Int(oldGridX, oldGridY), faceCurrent);
            //Update next tile
            UpdateEdges(new Vector2Int(gridX, gridY), faceNext);
            //Break out if we hit the goal
            stillSearching = gridX != end.x || gridY != end.y;
            if (stillSearching) {
                //Current face
                float height = heightmap[oldGridX, oldGridY].cover.GetCover(faceCurrent);
                cover = Mathf.Max(cover, height);
                //Next face
                height = heightmap[gridX, gridY].cover.GetCover(faceNext);
                cover = Mathf.Max(cover, height);
                //Check for cover
                if (cover >= 1.0f) {
                    return false;
                }
            }
        }
        return true;
    }

    public Vector2Int[] FindPath(Vector2Int start, Vector2Int end) {
        //Track visited nodes
        List<Vector2Int> closed = new List<Vector2Int>();
        //Track potential nodes
        PriorityQueue<Vector2Int> open = new PriorityQueue<Vector2Int>();
        open.Enqueue(start, 0.0f);
        //Track best previous step
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        //Track g-scores for visiting each node
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        //Set cost of first node to 0
        gScore[start] = 0.0f;
        while (open.Count > 0) {
            //Get node with lowest cost
            Vector2Int current = open.Dequeue();
            //If goal construct path
            if (current == end) {
                return ConstructPath(cameFrom, current);
            }
            //Remove current node and add to closed
            closed.Add(current);
            //For every neighbouring node
            for (int x = -1; x <= 1; x++) {
                for (int z = -1; z <= 1; z++) {
                    //A node is not a neighbour of itself
                    if (x == 0 && z == 0) {
                        continue;
                    }
                    //No diagonals
                    if (x != 0 && z != 0) {
                        continue;
                    }

                    Vector2Int neighbour = current + new Vector2Int(x, z);
                    //Skip out of bounds cells
                    if (neighbour.x < 0 || neighbour.y < 0 || neighbour.x >= width || neighbour.y >= height) {
                        continue;
                    }

                    //Check for walls blocking path
                    if (!CanTraverse(current, neighbour)) {
                        continue;
                    }

                    //If neighbour is closed, skip
                    if (closed.Contains(neighbour)) {
                        continue;
                    }
                    //Calculate distance from start to neighbour
                    float tentG = gScore[current] + 1.0f;
                    //Add neighbour to open if not in already
                    if (!open.Contains(neighbour)) {
                        open.Enqueue(neighbour, tentG + (neighbour - end).sqrMagnitude);
                    } else if (gScore.ContainsKey(neighbour) && tentG >= gScore[neighbour]) {
                        //Otherwise if score is greater than current, skip
                        continue;
                    }
                    //Update neighbour's camefrom to this node
                    cameFrom[neighbour] = current;
                    //Set neighbours g-score to new calculated
                    gScore[neighbour] = tentG;
                }
            }
        }
        return null;
    }

    Vector2Int[] ConstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current) {
        //Add current to path
        List<Vector2Int> path = new List<Vector2Int> {
            current
        };
        //While current exists in camefrom
        while (cameFrom.ContainsKey(current)) {
            //Set current to value in came from map
            current = cameFrom[current];
            //Add current to path
            path.Add(current);
        }
        return path.ToArray();
    }

    bool CanTraverse(Vector2Int first, Vector2Int last) {
        if (first.x != last.x && first.y != last.y) {
            //These tiles aren't neighbours!
            return false;
        }
        if (first.x == last.x && first.y == last.y) {
            //Why would you want to check if a tile can reach itself?
            return true;
        }
        GameObject firstObj = GetTile(first);
        GameObject lastObj = GetTile(last);
        //Check objects both exist
        if (!firstObj || !lastObj) {
            return false;
        }
        TileController firstTile = firstObj.GetComponent<TileController>();
        TileController lastTile = lastObj.GetComponent<TileController>();
        //Moving Left
        if (last.x < first.x) {
            if (firstTile.cover.negativeX > maxTraversableCover) {
                return false;
            }
            if (lastTile.cover.positiveX > maxTraversableCover) {
                return false;
            }
        }
        //Moving Right
        if (last.x > first.x) {
            if (firstTile.cover.positiveX > maxTraversableCover) {
                return false;
            }
            if (lastTile.cover.negativeX > maxTraversableCover) {
                return false;
            }
        }
        //Moving Down
        if (last.y < first.y) {
            if (firstTile.cover.negativeZ > maxTraversableCover) {
                return false;
            }
            if (lastTile.cover.positiveZ > maxTraversableCover) {
                return false;
            }
        }
        //Moving Up
        if (last.y > first.y) {
            if (firstTile.cover.positiveZ > maxTraversableCover) {
                return false;
            }
            if (lastTile.cover.negativeZ > maxTraversableCover) {
                return false;
            }
        }
        return true;
    }

    void UpdateEdges(Vector2Int pos, int edge) {
        string edgeName;
        switch (edge) {
            case 0:
                edgeName = "NegX";
                break;
            case 1:
                edgeName = "PosX";
                break;
            case 2:
                edgeName = "NegZ";
                break;
            case 3:
                edgeName = "PosZ";
                break;
            default:
                edgeName = "ERR";
                break;
        }
        GameObject tile = GetTile(pos);
        if (!tile) {
            Debug.LogError("No tile");
        }
        Transform edgeTransform = tile.transform.Find(edgeName);
        if (!edgeTransform) {
            Debug.LogError("No edge with name " + edgeName);
        }
        edgeTransform.gameObject.GetComponent<Renderer>().material = checkingMaterial;
    }

    // Use this for initialization
    void Start() {
        heightmap = new TileController[width, height];
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < height; z++) {
                GameObject newTile = Instantiate(tile, new Vector3(x, 0.0f, z), Quaternion.identity, gameObject.transform);
                newTile.name = x + "," + z;
                TileController tCon = newTile.GetComponent<TileController>();
                if (!tCon) {
                    Debug.LogError("Tile lacks controller");
                }
                tCon.gridPos = new Vector2Int(x, z);
                heightmap[x, z] = tCon;
            }
        }
    }

    private void ClearLOS() {
        if (!clearLOS) {
            return;
        }
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < height; z++) {
                Vector2Int pos = new Vector2Int(x, z);
                if(pos == rayStart || pos == rayEnd) {
                    continue;
                }
                GameObject tileObj = GetTile(pos);
                foreach (Renderer child in tileObj.GetComponentsInChildren<Renderer>()) {
                    if (child.gameObject != tileObj) {
                        child.material = darkMaterial;
                    } else {
                        child.material = baseMaterial;
                    }
                }
            }
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            placingStart = !placingStart;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            ClearLOS();
        }
    }

    public void TileClicked(Vector2Int tile) {
        ClearLOS();
        if (placingStart) {
            if (tileStart) {
                tileStart.GetComponent<Renderer>().material = baseMaterial;
            }
            rayStart = tile;
            tileStart = GetTile(tile);
            tileStart.GetComponent<Renderer>().material = startMaterial;
        } else {
            if (tileEnd) {
                tileEnd.GetComponent<Renderer>().material = baseMaterial;
            }
            rayEnd = tile;
            tileEnd = GetTile(tile);
            tileEnd.GetComponent<Renderer>().material = endMaterial;
        }
        /*
        float cover;
        bool los = HasLineOfSightDDA(rayStart, rayEnd, out cover);
        Debug.Log(los ? "LOS " + cover : "No LOS");
        */
        Vector2Int[] route = FindPath(rayStart, rayEnd);
        if (route != null) {
            foreach (Vector2Int pos in route) {
                if (pos != rayStart && pos != rayEnd) {
                    GetTile(pos).GetComponent<Renderer>().material = checkingMaterial;
                }
            }
        }
    }

    private GameObject GetTile(Vector2Int pos) {
        return GameObject.Find(pos.x + "," + pos.y);
    }
}
