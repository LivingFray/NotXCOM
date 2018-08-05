using UnityEngine;

public class CameraController : MonoBehaviour {

    public float edgeSize = 0.05f;

    public float cameraSpeed = 4.0f;

    public float zoom = 10.0f;

    //When true camera goes into "over the shoulder" view
    bool closeUp = false;

    public Vector3 offset;

    public Vector3Int from;

    public Vector3Int to;

    Vector3 lookingAt;

    Quaternion previousRotation;

    public bool DEBUG_TRIGGER;

	void Start () {
        lookingAt = new Vector3(0, 0, 0);
        previousRotation = transform.rotation;
	}
	
	void Update () {
        if (DEBUG_TRIGGER) {
            if (closeUp) {
                LeaveCloseUp();
            } else {
                EnterCloseUp(from, to);
            }
            DEBUG_TRIGGER = false;
        }
        //Only move camera if not in close up mode
        if (!closeUp) {
            //Rotate camera 90 degrees
            if (Input.GetKeyDown(KeyCode.Q)) {
                transform.Rotate(new Vector3(0, 90, 0), Space.World);
            }
            if (Input.GetKeyDown(KeyCode.E)) {
                transform.Rotate(new Vector3(0, -90, 0), Space.World);
            }

            float borderSize = edgeSize * Screen.width;
            //Check if mouse is at edge of screen or direction is pressed and pan camera
            if (Input.GetKey(KeyCode.A) || Input.mousePosition.x < borderSize) {
                lookingAt -= transform.right * Time.deltaTime * cameraSpeed;
            } else if (Input.GetKey(KeyCode.D) || Input.mousePosition.x > Screen.width - borderSize) {
                lookingAt += transform.right * Time.deltaTime * cameraSpeed;
            }
            if (Input.GetKey(KeyCode.S) || Input.mousePosition.y < borderSize) {
                lookingAt -= Vector3.Cross(transform.right, new Vector3(0, 1, 0)) * Time.deltaTime * cameraSpeed;
            } else if (Input.GetKey(KeyCode.W) || Input.mousePosition.y > Screen.height - borderSize) {
                lookingAt += Vector3.Cross(transform.right, new Vector3(0, 1, 0)) * Time.deltaTime * cameraSpeed;
            }

            //Update new camera position
            transform.position = lookingAt - transform.forward * zoom;
        }
    }

    public void EnterCloseUp(Vector3Int from, Vector3Int to) {
        if(closeUp) {
            return;
        }
        previousRotation = transform.rotation;
        transform.position = from;
        transform.rotation = Quaternion.LookRotation(to - transform.position, new Vector3(0, 1, 0));
        transform.position += transform.forward * offset.z + transform.right * offset.x + transform.up * offset.y;
        transform.rotation = Quaternion.LookRotation(to - transform.position, new Vector3(0, 1, 0));
        closeUp = true;
    }

    public void LeaveCloseUp() {
        if(!closeUp) {
            return;
        }
        //Restore camera information
        //Don't worry about position, update is about to deal with it
        transform.rotation = previousRotation;
        closeUp = false;
    }
}
