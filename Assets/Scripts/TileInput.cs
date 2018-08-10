using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TileInput : MonoBehaviour {

    [HideInInspector]
    public Tile tile;

    [Range(0,2)]
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

    public Material lowCoverMaterial;
    public Material highCoverMaterial;

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(1)) {
            tile.TileClicked();
        }
    }

    private void OnValidate() {
        UpdateFaces();
    }

    public void UpdateFaces() {
        UpdateFace(transform.Find("NegX").gameObject, negativeX);
        UpdateFace(transform.Find("PosX").gameObject, positiveX);
        UpdateFace(transform.Find("NegZ").gameObject, negativeZ);
        UpdateFace(transform.Find("PosZ").gameObject, positiveZ);
        transform.Find("NegY").gameObject.SetActive(negativeY != 0);
        transform.Find("PosY").gameObject.SetActive(positiveY != 0);
        //Handle floor hitbox
        GetComponent<BoxCollider>().enabled = negativeY != 0;
    }

    void UpdateFace(GameObject face, byte cover) {
        if(cover == 0) {
            face.SetActive(false);
        } else if(cover == 1) {
            face.SetActive(true);
            face.GetComponent<Renderer>().material = lowCoverMaterial;
        } else {
            face.SetActive(true);
            face.GetComponent<Renderer>().material = highCoverMaterial;
        }
    }

    [MenuItem("NotXCOM/Autogenerate Tiles", true)]
    static bool CreateTilesValidate() {
        if (!Selection.activeTransform || GameObject.Find("Board") == null) {
            return false;
        }
        return true;
    }
    [MenuItem("NotXCOM/Autogenerate Tiles")]
    static void CreateTiles() {
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

    static void PlaceTile(Transform parent, Vector3Int pos, Vector3Int minPos, Vector3Int maxPos) {
        //Only place a tile if it is on the edge
        bool isNegX = pos.x == minPos.x;
        bool isPosX = pos.x == maxPos.x;
        bool isNegY = pos.y == minPos.y;
        bool isPosY = pos.y == maxPos.y;
        bool isNegZ = pos.z == minPos.z;
        bool isPosZ = pos.z == maxPos.z;
        //Fully enclosed tile, don't waste resources on a hitbox
        if(!isNegX && !isNegY && !isNegZ && !isPosX && !isPosY && !isPosZ) {
            return;
        }
        GameObject tilePrefab = Resources.Load<GameObject>("Tile");
        GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, parent);
        TileInput tileInput = tile.GetComponent<TileInput>();
        //Default edges that exist to high cover, user can change this on a per instance basis if wrong
        tileInput.negativeX = isNegX ? (byte)2 : (byte)0;
        tileInput.positiveX = isPosX ? (byte)2 : (byte)0;
        tileInput.negativeY = isNegY ? (byte)2 : (byte)0;
        tileInput.positiveY = isPosY ? (byte)2 : (byte)0;
        tileInput.negativeZ = isNegZ ? (byte)2 : (byte)0;
        tileInput.positiveZ = isPosZ ? (byte)2 : (byte)0;
        tileInput.UpdateFaces();
    }
}
