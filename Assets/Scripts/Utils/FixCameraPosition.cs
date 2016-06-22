using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class FixCameraPosition : MonoBehaviour {

    public Vector3 offset = Vector3.up * 100.0f;

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
            myCamera.transform.localPosition = Vector3.Lerp(offset, Vector3.zero, Mathf.Clamp01(refAspect / aspect));
        }
	}
}
