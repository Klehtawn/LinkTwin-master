using UnityEngine;
using System.Collections;

public class GameplayBackground : MonoBehaviour {

    public Camera renderCamera;
    public Transform scene;

    private Camera myCamera;

	// Use this for initialization
	void Start () {

        GameObject camObj = new GameObject();
        camObj.name = "backgroundRenderCamera";
        camObj.transform.SetParent(transform);

        myCamera = camObj.AddComponent<Camera>();
        myCamera.depth = renderCamera.depth - 100.0f;
        myCamera.orthographic = true;
        myCamera.orthographicSize = 1920.0f * 0.25f;
        myCamera.transform.position = scene.transform.position + Vector3.back * 5000.0f;
        myCamera.farClipPlane = 10000.0f;
        myCamera.targetTexture = renderCamera.targetTexture;
        myCamera.clearFlags = CameraClearFlags.SolidColor;
        myCamera.backgroundColor = Color.red;

        int layerToUse = LayerMask.NameToLayer("IntroSequence");
        myCamera.cullingMask = 1 << layerToUse;

        Widget.SetLayer(scene.gameObject, layerToUse);

        renderCamera.clearFlags = CameraClearFlags.Nothing;

        float refAspect = 1920.0f / 1080.0f;
        float aspect = (float)Screen.height / (float)Screen.width;

        scene.localScale = new Vector3(-1.0f, 1.0f, -10.0f) * 100.0f * refAspect / aspect;

        myCamera.transform.position = myCamera.transform.position + Vector3.up * (1920.0f * 0.5f * (refAspect / aspect - 1.0f)) * 0.5f;
	}

    void Update()
    {
    }
}
