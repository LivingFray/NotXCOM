using UnityEditor;
using UnityEngine;

public class TileWizard : ScriptableWizard {

    [Range(0, 2)]
    public byte negativeX;
    [Range(0, 2)]
    public byte positiveX;
    [Range(0, 2)]
    public byte negativeY;
    [Range(0, 2)]
    public byte positiveY;
    [Range(0, 2)]
    public byte negativeZ;
    [Range(0, 2)]
    public byte positiveZ;

    [MenuItem("NotXCOM/Autogenerate Tiles", true)]
    static bool CreateTilesValidate() {
        if (!Selection.activeTransform || GameObject.Find("Board") == null) {
            return false;
        }
        return true;
    }

    [MenuItem("NotXCOM/Generate Tiles")]
    static void CreateTiles() {
        DisplayWizard<TileWizard>("Generate tiles", "Generate");
    }

    private void OnWizardCreate() {
        Transform board = GameObject.Find("Board").transform;
        foreach (GameObject selected in Selection.gameObjects) {
            Transform transform = selected.transform;
            BoxCollider boxCollider = selected.GetComponent<BoxCollider>();
            if (!boxCollider) {
                continue;
            }
            //Get edges of bounding box
            Vector3 minPos = transform.position + boxCollider.center - Vector3.Scale(transform.localScale, boxCollider.size) / 2.0f;
            Vector3 maxPos = transform.position + boxCollider.center + Vector3.Scale(transform.localScale, boxCollider.size) / 2.0f;
            //Move in from edges to centre
            minPos += new Vector3(0.5f, 0.0f, 0.5f);
            maxPos -= new Vector3(0.5f, 1.0f, 0.5f);
            Vector3Int minIntPos = Vector3Int.RoundToInt(minPos);
            Vector3Int maxIntPos = Vector3Int.RoundToInt(maxPos);
            GameObject holder = new GameObject(transform.name);
            holder.transform.parent = board;
            //For each tile inside bounding box, attempt to place a tile
            for (int x = minIntPos.x; x <= maxIntPos.x; x++) {
                for (int y = minIntPos.y; y <= maxIntPos.y; y++) {
                    for (int z = minIntPos.z; z <= maxIntPos.z; z++) {
                        PlaceTile(holder.transform, new Vector3Int(x, y, z), minIntPos, maxIntPos);
                    }
                }
            }
            Undo.RegisterCreatedObjectUndo(holder, "Create tiles");
        }
    }

    void PlaceTile(Transform parent, Vector3Int pos, Vector3Int minPos, Vector3Int maxPos) {
        //Only place a tile if it is on the edge
        bool isNegX = negativeX != 0 && pos.x == minPos.x;
        bool isPosX = positiveX != 0 && pos.x == maxPos.x;
        bool isNegY = negativeY != 0 && pos.y == minPos.y;
        bool isPosY = positiveY != 0 && pos.y == maxPos.y;
        bool isNegZ = negativeZ != 0 && pos.z == minPos.z;
        bool isPosZ = positiveZ != 0 && pos.z == maxPos.z;
        //Fully enclosed tile, don't waste resources on a hitbox
        if (!isNegX && !isNegY && !isNegZ && !isPosX && !isPosY && !isPosZ) {
            return;
        }
        GameObject tilePrefab = Resources.Load<GameObject>("Tile");
        GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, parent);
        TileInput tileInput = tile.GetComponent<TileInput>();
        //Default edges that exist to high cover, user can change this on a per instance basis if wrong
        tileInput.negativeX = isNegX ? negativeX : (byte)0;
        tileInput.positiveX = isPosX ? positiveX : (byte)0;
        tileInput.negativeY = isNegY ? negativeY : (byte)0;
        tileInput.positiveY = isPosY ? positiveY : (byte)0;
        tileInput.negativeZ = isNegZ ? negativeZ : (byte)0;
        tileInput.positiveZ = isPosZ ? positiveZ : (byte)0;
        tileInput.UpdateFaces();
    }
}
