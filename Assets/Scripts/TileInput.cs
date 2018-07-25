using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInput : MonoBehaviour {

    public TileController tileController;

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(1)) {
            tileController.TileClicked();
        }
    }
}
