using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameController : MonoBehaviour {

    TileController[,,] heightmap;

    public int width = 20;
    public int height = 20;
    public int depth = 20;

    public GameObject tile;

    public Material baseMaterial;
    public Material darkMaterial;
    public Material startMaterial;
    public Material endMaterial;

    public Material checkingMaterial;

    private Vector3Int rayStart;
    private GameObject tileStart;

    private Vector3Int rayEnd;
    private GameObject tileEnd;

    private bool placingStart = true;

    public GameObject line;

    public bool clearLOS = true;

    public float maxTraversableCover = 0.5f;

    public bool HasLineOfSightDDA(Vector3Int start, Vector3Int end, out byte cover) {
        //Iterate from start position to end position, checking for blocking cover
        //TODO: Allow for stepping out
        //Full cover between blocks completely
        //Half cover between doesn't block but increases cover value
        //No cover between causes flanks

        //TODO: Fix 45 degree being dependant on direction
        line.GetComponent<LineRenderer>().SetPosition(0, start + new Vector3(0, 0.5f, 0));
        line.GetComponent<LineRenderer>().SetPosition(1, end + new Vector3(0, 0.5f, 0));
        //Get Ray direction
        float dirX = end.x - start.x;
        float dirY = end.y - start.y;
        float dirZ = end.z - start.z;

        int stepX = dirX >= 0 ? 1 : -1;
        int stepY = dirY >= 0 ? 1 : -1;
        int stepZ = dirZ >= 0 ? 1 : -1;

        float deltaX = stepX / dirX;
        float deltaY = stepY / dirY;
        float deltaZ = stepZ / dirZ;

        Vector3Int grid = start;

        float maxX = ((grid.x + Mathf.Max(0, stepX)) - (start.x + 0.5f)) / dirX;
        float maxY = ((grid.y + Mathf.Max(0, stepY)) - (start.y + 0.5f)) / dirY;
        float maxZ = ((grid.z + Mathf.Max(0, stepZ)) - (start.z + 0.5f)) / dirZ;

        bool stillSearching = true;

        cover = (byte)CoverType.NONE;

        Vector3Int oldGrid = new Vector3Int();
        /*
         if(dda.maxX < dda.maxY) {
			if(dda.maxX < dda.maxZ) {
				dda.gridX = dda.gridX + dda.stepX;
				if(dda.gridX == numX || dda.gridX == -1)
					return false; 
				dda.maxX = dda.maxX + dda.deltaX;
				dda.distToEdge += dda.deltaX;
			} else  {
				dda.gridZ = dda.gridZ + dda.stepZ;
				if(dda.gridZ == numZ || dda.gridZ == -1)
					return false;
				dda.maxZ = dda.maxZ + dda.deltaZ;
				dda.distToEdge += dda.deltaZ;
			}
		} else  {
			if(dda.maxY < dda.maxZ) {
				dda.gridY = dda.gridY + dda.stepY;
				if(dda.gridY == numY || dda.gridY == -1)
					return false;
				dda.maxY = dda.maxY + dda.deltaY;
				dda.distToEdge += dda.deltaY;
			} else  {
				dda.gridZ = dda.gridZ + dda.stepZ;
				if(dda.gridZ == numZ || dda.gridZ == -1)
					return false;
				dda.maxZ = dda.maxZ + dda.deltaZ;
				dda.distToEdge += dda.deltaZ;
			}
		}
         */
        while (stillSearching) {
            oldGrid = grid;
            //Which face of the tile was passed through
            //0 = NegX, 1 = PosX, 2 = NegY, 3 = PosY, 4 = NegZ, 5 = PosZ
            byte faceCurrent;
            byte faceNext;
            if (maxX < maxY) {
                if (maxX < maxZ) {
                    //Change in X
                    if (stepX < 0) {
                        faceCurrent = 0;
                        faceNext = 1;
                    } else {
                        faceCurrent = 1;
                        faceNext = 0;
                    }
                    grid.x = grid.x + stepX;
                    maxX = maxX + deltaX;
                } else {
                    //Change in Z
                    if (stepZ < 0) {
                        faceCurrent = 4;
                        faceNext = 5;
                    } else {
                        faceCurrent = 5;
                        faceNext = 4;
                    }
                    grid.z = grid.z + stepZ;
                    maxZ = maxZ + deltaZ;
                }
            } else {
                if (maxY < maxZ) {
                    //Change in Y
                    if (stepY < 0) {
                        faceCurrent = 2;
                        faceNext = 3;
                    } else {
                        faceCurrent = 3;
                        faceNext = 2;
                    }
                    grid.y = grid.y + stepY;
                    maxY = maxY + deltaY;
                } else {
                    //Change in Z
                    if (stepZ < 0) {
                        faceCurrent = 4;
                        faceNext = 5;
                    } else {
                        faceCurrent = 5;
                        faceNext = 4;
                    }
                    grid.z = grid.z + stepZ;
                    maxZ = maxZ + deltaZ;
                }
            }
            //Update current tile (DEBUG)
            UpdateEdges(oldGrid, faceCurrent);
            //Update next tile (DEBUG)
            UpdateEdges(grid, faceNext);
            //Break out if we hit the goal
            stillSearching = grid.x != end.x || grid.y != end.y || grid.z != end.z;
            if (stillSearching) {
                //Update cover value
                cover = Math.Max(cover, GetCoverValue(oldGrid, grid, faceCurrent, faceNext));
                //Check for cover
                if (cover == (byte)CoverType.FULL) {
                    return false;
                }
            }
        }
        return true;
    }

    byte GetCoverValue(Vector3Int t1, Vector3Int t2, byte f1, byte f2) {
        //Get the tile controllers for both tiles to check
        TileController tCon1 = GetTileController(t1), tCon2 = GetTileController(t2);
        if(tCon1 == null || tCon2 == null) {
            Debug.LogWarning("Missing tile controller!");
            return 0;
        }
        //Return the heighest cover value from the two tiles
        return Math.Max(tCon1.cover.GetCover(f1), tCon2.cover.GetCover(f2));
    }

    /*
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

    bool CanTraverse(Vector3Int first, Vector3Int last) {
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
    */
    void UpdateEdges(Vector3Int pos, int edge) {
        string edgeName;
        switch (edge) {
            case 0:
                edgeName = "NegX";
                break;
            case 1:
                edgeName = "PosX";
                break;
            case 2:
                edgeName = "NegY";
                break;
            case 3:
                edgeName = "PosY";
                break;
            case 4:
                edgeName = "NegZ";
                break;
            case 5:
                edgeName = "PosZ";
                break;
            default:
                edgeName = "ERR";
                break;
        }
        GameObject tile = GetTile(pos);
        if (!tile) {
            return;
        }
        Transform edgeTransform = tile.transform.Find(edgeName);
        if (!edgeTransform) {
            Debug.LogError("No edge with name " + edgeName);
        }
        edgeTransform.gameObject.GetComponent<Renderer>().material = checkingMaterial;
    }

    // Use this for initialization
    void Start() {
        heightmap = new TileController[width, height, depth];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {
                    //Associate grid positions
                    heightmap[x, y, z] = new TileController(this) {
                        gridPos = new Vector3Int(x, y, z)
                    };
                }
            }
        }
        GenerateTestBoard();
    }

    //Creates a board with tiles on for testing
    void GenerateTestBoard() {
        //Ground floor
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < depth; z++) {
                TileController tc = heightmap[x, 0, z];
                if (tc.Tile == null) {
                    GameObject newTile = Instantiate(tile, tc.gridPos, Quaternion.identity, gameObject.transform);
                    tc.Tile = newTile;
                }
                tc.cover.SetCover((byte)CoverSides.NEGY, (byte)CoverType.FULL);
                tc.Tile.transform.Find("NegX").gameObject.SetActive(false);
                tc.Tile.transform.Find("PosX").gameObject.SetActive(false);
                tc.Tile.transform.Find("NegZ").gameObject.SetActive(false);
                tc.Tile.transform.Find("PosZ").gameObject.SetActive(false);
                tc.Tile.transform.Find("PosY").gameObject.SetActive(false);
            }
        }
        //Small wall section
        for (int z = 10; z < width; z++) {
            int y = 1;
            int x = 10;
            TileController tc = heightmap[x, y, z];
            if (tc.Tile == null) {
                GameObject newTile = Instantiate(tile, tc.gridPos, Quaternion.identity, gameObject.transform);
                tc.Tile = newTile;
            }
            tc.cover.SetCover((byte)CoverSides.NEGX, (byte)CoverType.FULL);
            tc.Tile.transform.Find("PosY").gameObject.SetActive(false);
            tc.Tile.transform.Find("PosX").gameObject.SetActive(false);
            tc.Tile.transform.Find("NegZ").gameObject.SetActive(false);
            tc.Tile.transform.Find("PosZ").gameObject.SetActive(false);
            tc.Tile.transform.Find("NegY").gameObject.SetActive(false);
        }
        //Small raised area
        for (int x = 10; x < width-5; x++) {
            for (int z = 0; z < depth-10; z++) {
                TileController tc = heightmap[x, 2, z];
                if (tc.Tile == null) {
                    GameObject newTile = Instantiate(tile, tc.gridPos, Quaternion.identity, gameObject.transform);
                    tc.Tile = newTile;
                }
                tc.cover.SetCover((byte)CoverSides.NEGY, (byte)CoverType.FULL);
                tc.Tile.transform.Find("NegX").gameObject.SetActive(false);
                tc.Tile.transform.Find("PosX").gameObject.SetActive(false);
                tc.Tile.transform.Find("NegZ").gameObject.SetActive(false);
                tc.Tile.transform.Find("PosZ").gameObject.SetActive(false);
                tc.Tile.transform.Find("PosY").gameObject.SetActive(false);
            }
        }
    }

    private void ClearLOS() {
        //If clearing has been disabled for some reason, stop
        if (!clearLOS) {
            return;
        }
        //Iterate over each tile and reset material properties
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    //Don't reset marker tiles (begininng and end of path)
                    if (pos == rayStart || pos == rayEnd) {
                        continue;
                    }
                    GameObject tileObj = GetTile(pos);
                    //Skip any tiles that don't have a corresponding game object
                    if (tileObj == null) {
                        continue;
                    }
                    ResetMaterials(tileObj);
                }
            }
        }
    }

    private void Update() {
        //Toggle begin/end node placement
        if (Input.GetKeyDown(KeyCode.Space)) {
            placingStart = !placingStart;
        }
        //Manually clear the board
        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            bool prevClear = clearLOS;
            clearLOS = true;
            ClearLOS();
            clearLOS = prevClear;
        }
    }

    void SetMaterials(GameObject parent, Material mat) {
        //Loop through and mess with materials
        foreach (Renderer child in parent.GetComponentsInChildren<Renderer>()) {
            child.material = mat;
        }
    }

    void ResetMaterials(GameObject parent) {
        //Loop through and mess with materials
        foreach (Renderer child in parent.GetComponentsInChildren<Renderer>()) {
            //Floor/Ceiling tiles are lighter to add contrast
            if (child.gameObject.name.Contains("Y")) {
                child.material = baseMaterial;
            } else {
                child.material = darkMaterial;
            }
        }
    }

    public void TileClicked(Vector3Int tile) {
        //Wipe the board
        ClearLOS();
        //Place correct node
        if (placingStart) {
            if (tileStart) {
                ResetMaterials(tileStart);
            }
            rayStart = tile;
            tileStart = GetTile(tile);
            SetMaterials(tileStart, startMaterial);
        } else {
            if (tileEnd) {
                ResetMaterials(tileEnd);
            }
            rayEnd = tile;
            tileEnd = GetTile(tile);
            SetMaterials(tileEnd, endMaterial);
        }
        byte cover;
        bool los = HasLineOfSightDDA(rayStart, rayEnd, out cover);
        Debug.Log(los ? "LOS " + cover : "No LOS");
        //Disabled pathfinding until it has been updated to function in 3D
        /*
        Vector3Int[] route = FindPath(rayStart, rayEnd);
        if (route != null) {
            foreach (Vector3Int pos in route) {
                if (pos != rayStart && pos != rayEnd) {
                    GameObject t = GetTile(pos);
                    if (t) {
                        t.GetComponent<Renderer>().material = checkingMaterial;
                    }
                }
            }
        }
        */
    }

    //Gets the TileController for a specified tile (returns null if out of bounds)
    private TileController GetTileController(Vector3Int pos) {
        //Skip out of bounds
        if (pos.x < 0 || pos.y < 0 || pos.z < 0) {
            return null;
        }
        if (pos.x >= width || pos.y >= height || pos.z >= depth) {
            return null;
        }
        return heightmap[pos.x, pos.y, pos.z];
    }

    //Gets the Tile GameObject for a specified tile (if it exists)
    private GameObject GetTile(Vector3Int pos) {
        TileController tc = GetTileController(pos);
        if (tc == null) {
            return null;
        }
        return tc.Tile;
    }
}
