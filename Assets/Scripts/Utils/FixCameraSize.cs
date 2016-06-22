using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FixCameraSize : MonoBehaviour {

    public float zoom = 1.0f;

    Camera myCamera;
	void Start () {
        myCamera = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {

        if(myCamera == null)
            myCamera = GetComponent<Camera>();
        if (myCamera != null && myCamera.orthographic)
        {
            float refAspect = 1080.0f / 1920.0f;
            float aspect = (float)Screen.width / (float)Screen.height;
            myCamera.orthographicSize = Screen.height * 0.5f * zoom * refAspect / aspect;
        }
	}
}
