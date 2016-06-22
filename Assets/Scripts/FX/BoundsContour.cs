using UnityEngine;
using System.Collections;

public class BoundsContour : MonoBehaviour {

    public MeshFilter source;
    public Material material;

    private LineRenderer2 lineRenderer;
	void Start () {

        if(GetComponent<LineRenderer2>() != null)
        {
            GameObject.DestroyImmediate(GetComponent<LineRenderer2>());
        }

        if (GetComponent<MeshRenderer>() != null)
            GameObject.DestroyImmediate(GetComponent<MeshRenderer>());

        if (GetComponent<MeshFilter>() != null)
            GameObject.DestroyImmediate(GetComponent<MeshFilter>());

        //lineRenderer = GetComponent<LineRenderer2>();
        //if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer2>();

        lineRenderer.material = material;

        lineRenderer.transform.rotation = Quaternion.identity;
        lineRenderer.transform.position = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
        UpdateLine();
	}

    public float lineWidth = 0.5f;

    void UpdateLine()
    {
        Vector3[] points = new Vector3[4];

        Bounds b = source.sharedMesh.bounds;
        points[0] = b.center + new Vector3(-b.extents.x, b.extents.y, 0.0f);
        points[1] = b.center + new Vector3(b.extents.x, b.extents.y, 0.0f);
        points[2] = b.center + new Vector3(b.extents.x, -b.extents.y, 0.0f);
        points[3] = b.center + new Vector3(-b.extents.x, -b.extents.y, 0.0f);

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = source.transform.TransformPoint(points[i]);
        }

        lineRenderer.isClosed = true;
        lineRenderer.SetPositions(points, -1.0f, true);
        lineRenderer.SetWidth(lineWidth, true);
        lineRenderer.vOffset = 0.5f;
        lineRenderer.vScale = 0.0f;

        lineRenderer.UpdateLine(true, false, false);
    }
}
