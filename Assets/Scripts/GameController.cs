using UnityEngine;
using System;

public class GameController : MonoBehaviour {

    float[,] heightmap;

    public int width = 20;
    public int height = 20;

    public GameObject tile;

    public float baseTileHeight = 0.1f;

    public Material baseMaterial;
    public Material startMaterial;
    public Material endMaterial;

    public Material checkingMaterial;

    private Vector2Int rayStart;
    private GameObject tileStart;

    private Vector2Int rayEnd;
    private GameObject tileEnd;

    private bool placingStart = true;

    public GameObject line;

    public bool clearLOS;

    public bool HasLineOfSightDDA(Vector2Int start, Vector2Int end, out float cover) {
        ClearLOS();
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

        while (stillSearching) {
            if(maxX < maxY) {
                maxX += deltaX;
                gridX += stepX;
            } else {
                maxY += deltaY;
                gridY += stepY;
            }
            stillSearching = gridX != end.x || gridY != end.y;
            if (stillSearching) {
                float height = heightmap[gridX, gridY];
                cover = Mathf.Max(cover, height);
                //Debug
                GameObject tile = GetTile(new Vector2Int(gridX, gridY));
                tile.GetComponent<Renderer>().material = checkingMaterial;

                if (cover >= 1.0f) {
                    return false;
                }
            }
        }

        return true;

    }

    public bool HasLineOfSight(Vector2Int start, Vector2Int end, out float cover) {
        ClearLOS();
        line.GetComponent<LineRenderer>().SetPosition(0, new Vector3(start.x, 1.0f, start.y));
        line.GetComponent<LineRenderer>().SetPosition(1, new Vector3(end.x, 1.0f, end.y));
        //Iterate from start position to end position, checking for blocking cover
        //TODO: Allow for stepping out
        //Full cover between blocks completely
        //Half cover between doesn't block but increases cover value
        //No cover between causes flanks

        //Implementation of DDA algorithm
        float deltaX = end.x - start.x;
        float deltaY = end.y - start.y;
        float step;
        if (Mathf.Abs(deltaX) >= Mathf.Abs(deltaY)) {
            step = Mathf.Abs(deltaX);
        } else {
            step = Mathf.Abs(deltaY);
        }

        deltaX /= step;
        deltaY /= step;

        float x = start.x;
        float y = start.y;
        //Skip initial step (don't care about start tile)
        x += deltaX;
        y += deltaY;

        cover = 0;

        for (int i = 1; i < step; i++) {
            int intX = Mathf.RoundToInt(x);
            int intY = Mathf.RoundToInt(y);
            float height = heightmap[intX, intY];
            cover = Mathf.Max(cover, height);

            //Debug
            GameObject tile = GetTile(new Vector2Int(intX, intY));
            tile.GetComponent<Renderer>().material = checkingMaterial;

            if (cover >= 1.0f) {
            //    return false;
            }
            x += deltaX;
            y += deltaY;
        }

        return true;
    }


    public void SetTileHeight(Vector2Int pos, float height) {
        GameObject selectedTile = GetTile(pos);
        if (!selectedTile) {
            Debug.LogWarning("Tile at pos " + pos + " not found");
            return;
        }
        selectedTile.transform.localScale = new Vector3(1.0f, baseTileHeight + height, 1.0f);
        selectedTile.transform.localPosition = new Vector3(pos.x, (baseTileHeight + height) / 2.0f, pos.y);
        heightmap[pos.x, pos.y] = height;
        TileController tCon = selectedTile.GetComponent<TileController>();
        if (!tCon) {
            Debug.LogError("Tile lacks controller");
            return;
        }
        tCon.cover = height;
    }

    // Use this for initialization
    void Start() {
        heightmap = new float[width, height];
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < height; z++) {
                heightmap[x, z] = 0.0f;
                GameObject newTile = Instantiate(tile, new Vector3(x, baseTileHeight / 2.0f, z), Quaternion.identity, gameObject.transform);
                newTile.transform.localScale = new Vector3(1.0f, baseTileHeight, 1.0f);
                newTile.name = x + "," + z;
                TileController tCon = newTile.GetComponent<TileController>();
                if (!tCon) {
                    Debug.LogError("Tile lacks controller");
                }
                tCon.cover = 0.0f;
                tCon.gridPos = new Vector2Int(x, z);
            }
        }
    }

    private void ClearLOS() {
        if(!clearLOS) {
            return;
        }
        for (int x = 0; x < width; x++) {
            for (int z = 0; z < height; z++) {
                GetTile(new Vector2Int(x, z)).GetComponent<Renderer>().material = baseMaterial;
            }
        }
        if (tileStart) {
            tileStart.GetComponent<Renderer>().material = startMaterial;
        }
        if (tileEnd) {
            tileEnd.GetComponent<Renderer>().material = endMaterial;
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            placingStart = !placingStart;
        }
        if(Input.GetKeyDown(KeyCode.LeftShift)) {
            ClearLOS();
        }
    }

    public void TileClicked(Vector2Int tile) {
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
        float cover;
        // los = HasLineOfSight(rayStart, rayEnd, out cover);
        bool los = HasLineOfSightDDA(rayStart, rayEnd, out cover);
        Debug.Log(los ? "LOS " + cover : "No LOS");
    }

    private GameObject GetTile(Vector2Int pos) {
        return GameObject.Find(pos.x + "," + pos.y);
    }
}
