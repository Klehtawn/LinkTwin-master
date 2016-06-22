using UnityEngine;
using System.Collections;
using System;

//[ExecuteInEditMode]

public class ExtrudedSprite : MonoBehaviour {

    bool hasMeshComponents = false;
    bool hasBlockMesh = false;

    public float size = 10.0f;

    public float extrusion = 20.0f;
    public Vector3 extrusionDir = Vector3.forward;

    public Material preferredMaterial;

	// Use this for initialization
	void Start () {
        hasMeshComponents = GetComponent<MeshFilter>() != null && GetComponent<MeshRenderer>() != null;
        hasBlockMesh = false;

        AutoCreateMeshData();
        AutoCreateBlockMesh();

        //if (GetComponent<Renderer>().material == null)
        //    GetComponent<Renderer>().material = preferredMaterial;
	}
	
	// Update is called once per frame
	void Update ()
    {

	}
    void AutoCreateMeshData()
    {
        if (hasMeshComponents) return;

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        hasMeshComponents = true;
    }

    public float density = 0.25f;

    void AutoCreateBlockMesh()
    {
        if (hasBlockMesh) return;

        Mesh m = new Mesh();

        int steps = Mathf.CeilToInt(extrusion * density);

        Vector3[] pos = new Vector3[4 * steps];
        Vector2[] uv = new Vector2[4 * steps];

        float half = size * 0.5f;

        //Vector3 dir = extrusionDir.normalized;

        // do only border eventually for faster blending
        for (int i = 0; i < steps; i++)
        {
            float depth = (float)i / (float)(steps - 1);
            depth *= -1.0f;
            pos[4 * i + 0] = new Vector3(-half, depth, half);
            pos[4 * i + 1] = new Vector3(half, depth, half);
            pos[4 * i + 2] = new Vector3(half, depth, -half);
            pos[4 * i + 3] = new Vector3(-half, depth, -half);

            uv[4 * i + 0] = new Vector2(1.0f, 0.0f);
            uv[4 * i + 1] = new Vector2(0.0f, 0.0f);
            uv[4 * i + 2] = new Vector2(0.0f, 1.0f);
            uv[4 * i + 3] = new Vector2(1.0f, 1.0f);
        }

        m.vertices = pos;
        m.uv = uv;

        int[] inds = new int[6 * steps];

        for (int i = 0; i < steps; i++)
        {
            inds[6 * i + 0] = 4 * i + 0;
            inds[6 * i + 1] = 4 * i + 1;
            inds[6 * i + 2] = 4 * i + 2;

            inds[6 * i + 3] = 4 * i + 0;
            inds[6 * i + 4] = 4 * i + 2;
            inds[6 * i + 5] = 4 * i + 3;
        }

        m.SetIndices(inds, MeshTopology.Triangles, 0);

        GetComponent<MeshFilter>().mesh = m;
        m.name = "extruded sprite";
        hasBlockMesh = true;
    }

    void OnValidate()
    {
        hasBlockMesh = false;
    }

    public void Regenerate()
    {
        hasBlockMesh = false;
    }
}