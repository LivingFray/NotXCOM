using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float edgeSize = 0.05f;

    public float cameraSpeed = 4.0f;

    public float zoom = 10.0f;

    Vector3 lookingAt;

    void UpdateCamera() {
        transform.position = lookingAt - transform.forward * zoom;
    }

	// Use this for initialization
	void Start () {
        lookingAt = new Vector3(0, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.Q)) {
            transform.Rotate(new Vector3(0, 90, 0), Space.World);
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            transform.Rotate(new Vector3(0, -90, 0), Space.World);
        }

        float borderSize = edgeSize * Screen.width;

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

        UpdateCamera();
    }
}
