using UnityEngine;
using System;
using System.Collections.Generic;

/* 
 * Container object for the game board
 * Handles terrain, LOS, pathing
 */
public class Board : MonoBehaviour {
    Tile[,,] tiles;

    GameController controller;

    int width, height, depth;

    private void Awake() {
        controller = GetComponent<GameController>();
        if(controller == null) {
            Debug.LogError("Cannot find game controller script");
        }
    }

    public void CreateBoard(int width, int height, int depth) {
        this.width = width;
        this.height = height;
        this.depth = depth;
        //Initialise the board
        tiles = new Tile[width, height, depth];
        //Set default values
        tiles = new Tile[width, height, depth];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < depth; z++) {
                    //Associate grid positions
                    tiles[x, y, z] = new Tile(controller) {
                        gridPos = new Vector3Int(x, y, z)
                    };
                }
            }
        }
    }

    public void LoadBoard(GameObject board) {
        Debug.Log("Finding tiles...");
        TileInput[] tiles = board.GetComponentsInChildren<TileInput>();
        Debug.Log("Found " + tiles.Length + " tiles");
        int minX = 0, minY = 0, minZ = 0;
        int maxX = 0, maxY = 0, maxZ = 0;
        for(int i = 0; i < tiles.Length; i++) {
            int x = (int)tiles[i].transform.position.x;
            int y = (int)tiles[i].transform.position.y;
            int z = (int)tiles[i].transform.position.z;
            if(i == 0) {
                //Initialise bounding box
                minX = maxX = x;
                minY = maxY = y;
                minZ = maxZ = z;
            } else {
                //Update min values
                if(minX > x) {
                    minX = x;
                }
                if(minY > y) {
                    minY = y;
                }
                if(minZ > z) {
                    minZ = z;
                }
                //Update max values
                if (maxX < x) {
                    maxX = x;
                }
                if (maxY < y) {
                    maxY = y;
                }
                if (maxZ < z) {
                    maxZ = z;
                }
            }
        }
        Debug.Log("New map bounding box identified: (" + minX + ", " + minY + ", " + minZ + ") - (" + maxX + ", " + maxY + ", " + maxZ + ")");
        //Game board assumes a lower bound of (0, 0, 0) so offset board to fix
        Debug.Log("Transforming board...");
        board.transform.position -= new Vector3(minX, minY, minZ);
        Debug.Log("Creating internal board representation");
        CreateBoard(1 + maxX - minX, 1 + maxY - minY, 1 + maxZ - minZ);
        Debug.Log("Loading tile collision data...");
        foreach(TileInput tile in tiles) {
            AddCover(tile);
        }
    }

    void AddCover(TileInput tileInput) {
        Tile tile = GetTile(Vector3Int.RoundToInt(tileInput.transform.position));
        tile.TileObject = tileInput.gameObject;
        if(tile == null) {
            Debug.Log(tileInput.transform.position);
        }
        Tile.Cover c = tile.cover;
        c.negativeX = Math.Max(c.negativeX, tileInput.negativeX);
        c.positiveX = Math.Max(c.positiveX, tileInput.positiveX);
        c.negativeY = Math.Max(c.negativeY, tileInput.negativeY);
        c.positiveY = Math.Max(c.positiveY, tileInput.positiveY);
        c.negativeZ = Math.Max(c.negativeX, tileInput.negativeZ);
        c.positiveZ = Math.Max(c.positiveZ, tileInput.positiveZ);
    }

    #region LOS
    public bool HasLineOfSight(Vector3Int start, Vector3Int end, out byte cover) {
        //Iterate from start position to end position, checking for blocking cover
        //TODO: Allow for stepping out
        //Full cover between blocks completely
        //Half cover between doesn't block but increases cover value
        //No cover between causes flanks

        //TODO: Fix 45 degree being dependant on direction
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
            //Break out if we hit the goal
            stillSearching = grid.x != end.x || grid.y != end.y || grid.z != end.z;
            //Update cover value
            cover = Math.Max(cover, GetCoverValue(oldGrid, grid, faceCurrent, faceNext));
            //Check for cover
            if (cover == (byte)CoverType.FULL) {
                return false;
            }
        }
        return true;
    }

    byte GetCoverValue(Vector3Int t1, Vector3Int t2, byte f1, byte f2) {
        //Get the tile controllers for both tiles to check
        Tile tile1 = GetTile(t1), tile2 = GetTile(t2);
        if (tile1 == null || tile2 == null) {
            Debug.LogWarning("Can't get cover, no tile scripts");
            return 0;
        }
        //Return the heighest cover value from the two tiles
        return Math.Max(tile1.cover.GetCover(f1), tile2.cover.GetCover(f2));
    }
    #endregion
    #region PATHFINDING
    public Vector3Int[] FindPath(Vector3Int start, Vector3Int end, int maxFallHeight = 10, int maxClimbHeight = 1) {
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
            List<Vector3Int> neighbours = GetNeighbours(current, maxFallHeight, maxClimbHeight);
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
    List<Vector3Int> GetNeighbours(Vector3Int cell, int maxFallHeight, int maxClimbHeight) {
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
            if (!CanTraverse(cell, offCell)) {
                continue;
            }

            //Iterate down until OOB, at max fall distance, hit a roof, or hit a floor
            Vector3Int currentCell = offCell;
            //Only check a fixed distance down
            for (int i = 0; i <= maxFallHeight; i++) {
                //Add cell if floor of current cell exists
                if (GetTile(currentCell).cover.GetCover((byte)CoverSides.NEGY) == (byte)CoverType.FULL) {
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
                if (GetTile(currentCell).cover.GetCover((byte)CoverSides.POSY) == (byte)CoverType.FULL) {
                    break;
                }
            }
        }

        return neighbours;
    }

    bool IsOccupied(Vector3Int tile) {
        //TODO: When moving entity update lookup table
        foreach (Entity ent in controller.entities) {
            if (ent.health > 0 && ent.GridPos == tile) {
                return true;
            }
        }
        return false;
    }

    bool IsWalkable(Vector3Int cell) {
        return !OutOfBounds(cell) && GetTile(cell).cover.GetCover((byte)CoverSides.NEGY) == (byte)CoverType.FULL;
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
        if (GetTile(first).cover.GetCover((byte)CoverSides.POSY) == (byte)CoverType.FULL) {
            return false;
        }
        //Check last tile's floor
        if (GetTile(last).cover.GetCover((byte)CoverSides.NEGY) == (byte)CoverType.FULL) {
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
        Tile firstTile = GetTile(first);
        Tile lastTile = GetTile(last);
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
    #endregion
    #region GETTERS
    //Gets the TileController for a specified tile (returns null if out of bounds)
    public Tile GetTile(Vector3Int pos) {
        //Skip out of bounds
        if (pos.x < 0 || pos.y < 0 || pos.z < 0) {
            return null;
        }
        if (pos.x >= width || pos.y >= height || pos.z >= depth) {
            return null;
        }
        return tiles[pos.x, pos.y, pos.z];
    }

    //Gets the Tile GameObject for a specified tile (if it exists)
    public GameObject GetTileObject(Vector3Int pos) {
        Tile tc = GetTile(pos);
        if (tc == null) {
            return null;
        }
        return tc.TileObject;
    }
    #endregion
    #region TESTING

    //Test tile object (full version will use visual-obly meshes that overlap board positions)
    public GameObject tile;

    //Creates a board with tiles on for testing
    public void GenerateTestBoard() {
        //Ground floor
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < depth; z++) {
                Tile tc = tiles[x, 0, z];
                if (tc.TileObject == null) {
                    GameObject newTile = Instantiate(tile, tc.gridPos, Quaternion.identity, gameObject.transform);
                    tc.TileObject = newTile;
                    SetTileObject(newTile);
                }
                tc.cover.SetCover((byte)CoverSides.NEGY, (byte)CoverType.FULL);
                tc.TileObject.transform.Find("NegY").gameObject.SetActive(true);
            }
        }
        //Small wall section
        for (int z = 10; z < width; z++) {
            int y = 0;
            int x = 10;
            Tile tc = tiles[x, y, z];
            if (tc.TileObject == null) {
                GameObject newTile = Instantiate(tile, tc.gridPos, Quaternion.identity, gameObject.transform);
                tc.TileObject = newTile;
                SetTileObject(newTile);
            }
            tc.cover.SetCover((byte)CoverSides.NEGX, (byte)CoverType.FULL);
            tc.TileObject.transform.Find("NegX").gameObject.SetActive(true);
        }
        //Small raised area
        for (int x = 10; x < width - 5; x++) {
            for (int z = 0; z < depth - 10; z++) {
                Tile tc = tiles[x, 2, z];
                if (tc.TileObject == null) {
                    GameObject newTile = Instantiate(tile, tc.gridPos, Quaternion.identity, gameObject.transform);
                    tc.TileObject = newTile;
                    SetTileObject(newTile);
                }
                tc.cover.SetCover((byte)CoverSides.NEGY, (byte)CoverType.FULL);
                tc.TileObject.transform.Find("NegY").gameObject.SetActive(true);
            }
        }
    }

    //Disables the sides of the test tile object
    void SetTileObject(GameObject tile) {
        tile.transform.Find("NegX").gameObject.SetActive(false);
        tile.transform.Find("PosX").gameObject.SetActive(false);
        tile.transform.Find("NegY").gameObject.SetActive(false);
        tile.transform.Find("PosY").gameObject.SetActive(false);
        tile.transform.Find("NegZ").gameObject.SetActive(false);
        tile.transform.Find("PosZ").gameObject.SetActive(false);
    }
    #endregion
}
