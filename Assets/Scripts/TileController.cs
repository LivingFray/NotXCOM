using UnityEngine;

public class TileController : MonoBehaviour {

    [System.Serializable]
    public class Cover {
        //I would use an array but this way looks nicer in the editor
        public float negativeX, positiveX, negativeZ, positiveZ;
        public float GetCover(int side) {
            switch (side) {
                case 0:
                    return negativeX;
                case 1:
                    return positiveX;
                case 2:
                    return negativeZ;
                case 3:
                    return positiveZ;
                default:
                    return 0;
            }
        }
        public void SetCover(int side, float cover) {
            switch (side) {
                case 0:
                    negativeX = cover;
                    break;
                case 1:
                    positiveX = cover;
                    break;
                case 2:
                    negativeZ = cover;
                    break;
                case 3:
                    positiveZ = cover;
                    break;
                default:
                    break;
            }
        }
    }

    public GameObject controllerObject;

    public Cover cover;

    public float minHeight = 0.05f;
    public float baseHeight = 0.5f;

    [HideInInspector]
    public Vector2Int gridPos;

    GameController controller;

    // Use this for initialization
    void Start() {
        if (!controllerObject) {
            controllerObject = GameObject.FindGameObjectWithTag("GameController");
        }
        controller = controllerObject.GetComponent<GameController>();
        if (!controller) {
            Debug.LogError("No Controller script in controller");
        }
    }

    private void OnMouseOver() {
        if (Input.GetMouseButtonDown(1)) {
            controller.TileClicked(gridPos);
        }
    }

    private void OnMouseDown() {
        //Get position clicked
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
        Vector3 mousePos = hit.point;
        //Determine closest edge
        mousePos -= transform.position;
        int edge;
        string edgeName;
        if (Mathf.Abs(mousePos.x) > Mathf.Abs(mousePos.z)) {
            if (mousePos.x > 0) {
                edge = 1;
                edgeName = "PosX";
            } else {
                edge = 0;
                edgeName = "NegX";
            }
        } else {
            if (mousePos.z > 0) {
                edge = 3;
                edgeName = "PosZ";
            } else {
                edge = 2;
                edgeName = "NegZ";
            }
        }
        float c = cover.GetCover(edge);
        Transform wall = transform.Find(edgeName);
        //Update height of corresponding edge
        if (c == 0.0f) {
            SetWallHeight(wall, 0.5f, edge);
        } else if (c == 0.5f) {
            SetWallHeight(wall, 1.0f, edge);
        } else {
            SetWallHeight(wall, 0.0f, edge);
        }
    }

    void SetWallHeight(Transform wall, float height, int edge) {
        Vector3 newScale = wall.localScale;
        newScale.y = Mathf.Max(minHeight, height);
        wall.localScale = newScale;
        Vector3 newPosition = wall.localPosition;
        newPosition.y = newScale.y / 2.0f + baseHeight;
        wall.localPosition = newPosition;
        cover.SetCover(edge, height);
    }
}