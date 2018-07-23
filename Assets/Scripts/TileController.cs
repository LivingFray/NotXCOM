using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour {

    public GameObject controllerObject;

    public float cover;

    [HideInInspector]
    public Vector2Int gridPos;

    GameController controller;

	// Use this for initialization
	void Start () {
        if(!controllerObject) {
            controllerObject = GameObject.FindGameObjectWithTag("GameController");
        }
        controller = controllerObject.GetComponent<GameController>();
        if(!controller) {
            Debug.LogError("No Controller script in controller");
        }
	}

    private void OnMouseOver() {
        if(Input.GetMouseButtonDown(1)) {
            controller.TileClicked(gridPos);
        }
    }

    private void OnMouseDown() {
        if(cover == 0.0f) {
            controller.SetTileHeight(gridPos, 0.5f);
        } else if (cover == 0.5f) {
            controller.SetTileHeight(gridPos, 1.0f);
        } else {
            controller.SetTileHeight(gridPos, 0.0f);
        }
    }
}
