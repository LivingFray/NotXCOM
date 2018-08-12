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
}
