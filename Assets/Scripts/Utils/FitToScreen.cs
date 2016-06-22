using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FitToScreen : MonoBehaviour {

    public Camera renderCamera = null;
	// Use this for initialization
	void Start ()
    {
        if (renderCamera == null)
            renderCamera = Camera.main;

        transform.SetAsFirstSibling();
	}
	
	// Update is called once per frame
	void Update ()
    {
        Quaternion camRotation = renderCamera.transform.rotation;
        renderCamera.transform.rotation = Quaternion.identity;

        // find corners of the cameras view frustrum at the distance of the gameobject
        float distance = Vector3.Distance(this.transform.position, renderCamera.transform.position);
        Vector3 viewBottomLeft = renderCamera.ViewportToWorldPoint(new Vector3(0, 0, distance));
        Vector3 viewTopRight = renderCamera.ViewportToWorldPoint(new Vector3(1, 1, distance));
        // scale the gameobject so it touches the cameras view frustrum
        Vector3 scale = viewTopRight - viewBottomLeft;
        scale.z = transform.localScale.z;
        transform.localScale = scale;
        //return the camera to it's original rotation
        renderCamera.transform.rotation = camRotation;
        transform.rotation = camRotation;
        transform.position = renderCamera.transform.position + renderCamera.transform.rotation * Vector3.forward * 800.0f;// (renderCamera.farClipPlane - renderCamera.nearClipPlane - 1.0f);
	}
}
