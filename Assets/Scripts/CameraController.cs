using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float edgeSize = 0.05f;

    public float cameraSpeed = 2.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.Q)) {
            transform.Rotate(new Vector3(0, 90, 0), Space.World);
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            transform.Rotate(new Vector3(0, 90, 0), Space.World);
        }

        float normX = Input.mousePosition.x / Screen.width;
        float normY = Input.mousePosition.y / Screen.height;

        if (normX < edgeSize) {
            transform.localPosition -= transform.right * Time.deltaTime * cameraSpeed;
        } else if (normX > 1.0 - edgeSize) {
            transform.localPosition += transform.right * Time.deltaTime * cameraSpeed;
        }

        if (normY < edgeSize) {
            transform.localPosition -= Vector3.Cross(transform.right, new Vector3(0, 1, 0)) * Time.deltaTime * cameraSpeed;
        } else if (normY > 1.0 - edgeSize) {
            transform.localPosition += Vector3.Cross(transform.right, new Vector3(0, 1, 0)) * Time.deltaTime * cameraSpeed;
        }
    }
}
