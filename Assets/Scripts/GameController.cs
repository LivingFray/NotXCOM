using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

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

    private bool placingStart = true;

    public GameObject line;

    public bool clearLOS = true;

    public int maxClimbHeight = 2;
    public int maxFallHeight = 4;

    //TODO: Make private?
    public List<GameObject> entities;
    public Team[] teams;

    public GameObject entityPrefab;

    public GameObject turnText;

    public GameObject entitySelect;

    private int currentTeam = 0;

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
        if (tCon1 == null || tCon2 == null) {
            Debug.LogWarning("Missing tile controller!");
            return 0;
        }
        //Return the heighest cover value from the two tiles
        return Math.Max(tCon1.cover.GetCover(f1), tCon2.cover.GetCover(f2));
    }

    public Vector3Int[] FindPath(Vector3Int start, Vector3Int end) {
        //Track visited nodes
        List<Vector3Int> closed = new List<Vector3Int>();
        //Track potential nodes
        PriorityQueue<Vector3Int> open = new PriorityQueue<Vector3Int>();
        open.Enqueue(start, 0.0f);
        //Track best previous step
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        //Track g-scores for visiting each node
        Dictionary<Vector3Int, float> gScore = new Dictionary<Vector3Int, float>();
        //Set cost of first node to 0
        gScore[start] = 0.0f;
        while (open.Count > 0) {
            //Get node with lowest cost
            Vector3Int current = open.Dequeue();
            //If goal construct path
            if (current == end) {
                return ConstructPath(cameFrom, current);
            }
            //Remove current node and add to closed
            closed.Add(current);
            //For every neighbouring node
            List<Vector3Int> neighbours = GetNeighbours(current);
            foreach (Vector3Int neighbour in neighbours) {
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
        return null;
    }

    //Extracts the shortest path from the distances calculated by A*
    Vector3Int[] ConstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current) {
        //Add current to path
        List<Vector3Int> path = new List<Vector3Int> {
            current
        };
        //While current exists in camefrom
        while (cameFrom.ContainsKey(current)) {
            //Set current to value in came from map
            current = cameFrom[current];
            //Add current to path
            path.Add(current);
        }
        Vector3Int[] retArray = new Vector3Int[path.Count];
        //Path is from end to start, so reverse it
        for (int i = 0; i < path.Count; i++) {
            retArray[i] = path[path.Count - i - 1];
        }
        return retArray;
    }

    //Returns a list of every cell that can be reached in one step 
    List<Vector3Int> GetNeighbours(Vector3Int cell) {
        /*
         * Valid neighbour cells:
         * Have a floor tile (NegY)
         * Are 1 tile away (no diagonals) horizontally
         * Are at most MaxClimb tiles away up
         * Are at most MaxFall tiles away down
         * Have no full cover between the start cell and neighbour
         * 
         * Vertical Movement is done in current cell when jumping
         * and neighbouring cell when falling
         */

        //Get maximum distance jumpable
        int maxJump = 0;
        {
            Vector3Int upCell = cell;
            Vector3Int lastCell = cell;
            while (maxJump < maxClimbHeight) {
                upCell = upCell + new Vector3Int(0, 1, 0);
                //Check for air space above
                if (CanTraverseVertical(lastCell, upCell)) {
                    maxJump++;
                } else {
                    break;
                }
                lastCell = upCell;
            }
        }
        List<Vector3Int> neighbours = new List<Vector3Int>();

        Vector3Int[] offsets = new Vector3Int[4];
        offsets[0] = new Vector3Int(-1, 0, 0);
        offsets[1] = new Vector3Int(1, 0, 0);
        offsets[2] = new Vector3Int(0, 0, -1);
        offsets[3] = new Vector3Int(0, 0, 1);

        foreach (Vector3Int offset in offsets) {
            Vector3Int offCell = offset + cell;
            //Skip the cell if it is outside the map
            if (OutOfBounds(offCell)) {
                continue;
            }
            //Check all cells that can be jumped to aswell as walked to
            for (int i = 0; i <= maxJump; i++) {
                Vector3Int n = offCell + new Vector3Int(0, i, 0);
                //Skip cells that aren't in the playing field
                if (n.y >= height) {
                    continue;
                }
                //If horizontal movement is fine, add it
                if (IsWalkable(n) && CanTraverse(cell + new Vector3Int(0, i, 0), n) && !IsOccupied(n)) {
                    neighbours.Add(n);
                }
            }

            //Check we can actually reach the air space to fall from
            if(!CanTraverse(cell, offCell)) {
                continue;
            }

            //Iterate down until OOB, at max fall distance, hit a roof, or hit a floor
            Vector3Int currentCell = offCell;
            //Only check a fixed distance down
            for (int i = 0; i <= maxFallHeight; i++) {
                //Add cell if floor of current cell exists
                if (GetTileController(currentCell).cover.GetCover((byte)CoverSides.NEGY) == (byte)CoverType.FULL) {
                    //Don't add the tile if the height is unchanged
                    if (i != 0 && !IsOccupied(currentCell)) {
                        neighbours.Add(currentCell);
                    }
                    break;
                }
                //Drop down a cell
                currentCell += new Vector3Int(0, -1, 0);
                //Check we didn't fall out of the map
                if (currentCell.y < 0) {
                    break;
                }
                //Check if ceiling of new cell blocks the fall
                if (GetTileController(currentCell).cover.GetCover((byte)CoverSides.POSY) == (byte)CoverType.FULL) {
                    break;
                }
            }
        }

        return neighbours;
    }

    bool IsOccupied(Vector3Int tile) {
        foreach (GameObject ent in entities) {
            if(ent.GetComponent<EntityController>().GridPos == tile) {
                return true;
            }
        }
        return false;
    }

    bool IsWalkable(Vector3Int cell) {
        return !OutOfBounds(cell) && GetTileController(cell).cover.GetCover((byte)CoverSides.NEGY) == (byte)CoverType.FULL;
    }

    bool CanTraverseVertical(Vector3Int first, Vector3Int last) {
        //Check we are still in the map
        if (OutOfBounds(first) || OutOfBounds(last)) {
            return false;
        }
        //Ensure first is lower than last
        if (first.y > last.y) {
            Vector3Int temp = first;
            first = last;
            last = temp;
        }
        //Check first tile's ceiling
        if (GetTileController(first).cover.GetCover((byte)CoverSides.POSY) == (byte)CoverType.FULL) {
            return false;
        }
        //Check last tile's floor
        if (GetTileController(last).cover.GetCover((byte)CoverSides.NEGY) == (byte)CoverType.FULL) {
            return false;
        }
        return true;
    }

    //Returns whether a queried cell is not within the grid 
    bool OutOfBounds(Vector3Int cell) {
        return cell.x < 0 || cell.y < 0 || cell.z < 0 || cell.x >= width || cell.y >= height || cell.z >= depth;
    }

    //Tests if a unit can move from one tile to the other
    bool CanTraverse(Vector3Int first, Vector3Int last) {
        TileController firstTile = GetTileController(first);
        TileController lastTile = GetTileController(last);
        //Moving Left
        if (last.x < first.x) {
            if (firstTile.cover.negativeX == (byte)CoverType.FULL) {
                return false;
            }
            if (lastTile.cover.positiveX == (byte)CoverType.FULL) {
                return false;
            }
        }
        //Moving Right
        if (last.x > first.x) {
            if (firstTile.cover.positiveX == (byte)CoverType.FULL) {
                return false;
            }
            if (lastTile.cover.negativeX == (byte)CoverType.FULL) {
                return false;
            }
        }
        //Moving Backwards
        if (last.z < first.z) {
            if (firstTile.cover.negativeZ == (byte)CoverType.FULL) {
                return false;
            }
            if (lastTile.cover.positiveZ == (byte)CoverType.FULL) {
                return false;
            }
        }
        //Moving Forwards
        if (last.z > first.z) {
            if (firstTile.cover.positiveZ == (byte)CoverType.FULL) {
                return false;
            }
            if (lastTile.cover.negativeZ == (byte)CoverType.FULL) {
                return false;
            }
        }
        return true;
    }

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
        //Initialise map
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
        //Create map
        GenerateTestBoard();
        //Create teams
        AddTeams();
        //Add entities
        foreach (Team t in teams) {
            t.PopulateEntities();
        }
        //Start game

        //Turn off entity select as nno entity is selected
        entitySelect.SetActive(false);
    }

    public void NextTurn() {
        teams[currentTeam].OnTurnEnd();
        currentTeam = (currentTeam + 1) % teams.Length;
        teams[currentTeam].OnTurnStart();
        turnText.GetComponent<TextMeshProUGUI>().text = "Player " + (currentTeam + 1) + "'s turn";
    }

    void AddTeams() {
        teams = new Team[2];
        //Human team
        teams[0] = new HumanTeam {
            Controller = this,
            entityPrefab = entityPrefab
        };
        //"""AI""" team (human for now, AI is hard)
        teams[1] = new HumanTeam {
            Controller = this,
            entityPrefab = entityPrefab
        };
        var spawnList = ((HumanTeam)teams[1]).spawnPositions;
        for (int i = 0; i < spawnList.Length; i++) {
            spawnList[i].x += 4;
        }
        //Set turn to player 1's (or player 0 if you use index position I guess)
        currentTeam = 0;
        teams[0].OnTurnStart();
    }

    void SetTileObject(GameObject tile) {
        tile.transform.Find("NegX").gameObject.SetActive(false);
        tile.transform.Find("PosX").gameObject.SetActive(false);
        tile.transform.Find("NegY").gameObject.SetActive(false);
        tile.transform.Find("PosY").gameObject.SetActive(false);
        tile.transform.Find("NegZ").gameObject.SetActive(false);
        tile.transform.Find("PosZ").gameObject.SetActive(false);
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
                    SetTileObject(newTile);
                }
                tc.cover.SetCover((byte)CoverSides.NEGY, (byte)CoverType.FULL);
                tc.Tile.transform.Find("NegY").gameObject.SetActive(true);
            }
        }
        //Small wall section
        for (int z = 10; z < width; z++) {
            int y = 0;
            int x = 10;
            TileController tc = heightmap[x, y, z];
            if (tc.Tile == null) {
                GameObject newTile = Instantiate(tile, tc.gridPos, Quaternion.identity, gameObject.transform);
                tc.Tile = newTile;
                SetTileObject(newTile);
            }
            tc.cover.SetCover((byte)CoverSides.NEGX, (byte)CoverType.FULL);
            tc.Tile.transform.Find("NegX").gameObject.SetActive(true);
        }
        //Small raised area
        for (int x = 10; x < width - 5; x++) {
            for (int z = 0; z < depth - 10; z++) {
                TileController tc = heightmap[x, 2, z];
                if (tc.Tile == null) {
                    GameObject newTile = Instantiate(tile, tc.gridPos, Quaternion.identity, gameObject.transform);
                    tc.Tile = newTile;
                    SetTileObject(newTile);
                }
                tc.cover.SetCover((byte)CoverSides.NEGY, (byte)CoverType.FULL);
                tc.Tile.transform.Find("NegY").gameObject.SetActive(true);
            }
        }
    }

    private void Update() {
        //Toggle begin/end node placement
        if (Input.GetKeyDown(KeyCode.Space)) {
            placingStart = !placingStart;
        }
        teams[currentTeam].Update();
    }

    public void TileClicked(Vector3Int tile) {
        teams[currentTeam].TileClicked(GetTileController(tile));
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

    public void EntityClicked(EntityController entity) {
        if(entity.team == teams[currentTeam]) {
            return;
        }
        teams[currentTeam].EnemyClicked(entity);
    }
}
