using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]

public class ObliqueBlock : MonoBehaviour {

    bool hasMeshComponents = false;
    bool hasBlockMesh = false;
    public float blockWidth = 10.0f;
    public Vector3 obliqueness = Vector3.down;
    public float topOffset = 0.0f;
    public float height = 50.0f;


	// Use this for initialization
	void Start () {
        hasMeshComponents = GetComponent<MeshFilter>() != null && GetComponent<MeshRenderer>() != null;
        hasBlockMesh = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        AutoCreateMeshData();
        AutoCreateBlockMesh();
	}
    void AutoCreateMeshData()
    {
        if (hasMeshComponents) return;

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        hasMeshComponents = true;
    }

    void AutoCreateBlockMesh()
    {
        if (hasBlockMesh) return;

        Mesh m = new Mesh();

        Vector3[] pos = new Vector3[24];
        Vector3[] nrm = new Vector3[24];
        Vector2[] uv = new Vector2[24];

        Vector3[] topCap = new Vector3[4];
        Vector3[] bottomCap = new Vector3[4];
        Vector3[] rightSide = new Vector3[4];
        Vector3[] leftSide = new Vector3[4];
        Vector3[] fwdSide = new Vector3[4];
        Vector3[] backSide = new Vector3[4];

        float halfSz = blockWidth * 0.5f;
        topCap[0] = new Vector3(-halfSz, 0.0f, halfSz);
        topCap[1] = new Vector3(halfSz, 0.0f, halfSz);
        topCap[2] = new Vector3(halfSz, 0.0f, -halfSz);
        topCap[3] = new Vector3(-halfSz, 0.0f, -halfSz);

        Vector3 oblDir = obliqueness.normalized;
        for (int i = 0; i < 4; i++ )
        {
            bottomCap[i] = topCap[i] + oblDir * height;
        }

        for (int i = 0; i < 4; i++)
        {
            topCap[i] = topCap[i] + oblDir * topOffset;
        }

        rightSide[0] = topCap[1];
        rightSide[1] = topCap[2];
        rightSide[2] = bottomCap[2];
        rightSide[3] = bottomCap[1];

        leftSide[0] = topCap[0];
        leftSide[1] = topCap[3];
        leftSide[2] = bottomCap[3];
        leftSide[3] = bottomCap[0];
        

        fwdSide[0] = topCap[0];
        fwdSide[1] = topCap[1];
        fwdSide[2] = bottomCap[1];
        fwdSide[3] = bottomCap[0];

        backSide[0] = topCap[3];
        backSide[1] = topCap[2];
        backSide[2] = bottomCap[2];
        backSide[3] = bottomCap[3];
        

        Array.Copy(topCap, pos, 4);
        Array.Copy(bottomCap, 0, pos, 4, 4);
        Array.Copy(rightSide, 0, pos, 8, 4);
        Array.Copy(leftSide, 0, pos, 12, 4);
        Array.Copy(fwdSide, 0, pos, 16, 4);
        Array.Copy(backSide, 0, pos, 20, 4);

        for (int i = 0; i < 6; i++)
        {
            uv[4 * i] = new Vector2(0.0f, 0.0f);
            uv[4 * i + 1] = new Vector2(1.0f, 0.0f);
            uv[4 * i + 2] = new Vector2(1.0f, 1.0f);
            uv[4 * i + 3] = new Vector2(0.0f, 1.0f);
        }

        for (int i = 0; i < 24; i++ )
        {
            nrm[i] = Vector3.up;
        }

        m.vertices = pos;
        m.normals = nrm;
        m.uv = uv;

        int[] inds = new int[6 * 6];
        for (int i = 0; i < 6; i++ )
        {
            inds[6 * i] = 4 * i + 0;
            inds[6 * i + 1] = 4 * i + 1;
            inds[6 * i + 2] = 4 * i + 2;
            inds[6 * i + 3] = 4 * i + 2;
            inds[6 * i + 4] = 4 * i + 3;
            inds[6 * i + 5] = 4 * i + 0;
        }

        m.SetIndices(inds, MeshTopology.Triangles, 0);

        SwapFace(m, 2); // bottom
        SwapFace(m, 3);

        SwapFace(m, 4); // right
        SwapFace(m, 5);

        SwapFace(m, 8); // fwd
        SwapFace(m, 9);

        m.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = m;
        m.name = "obliqueBlock";
        hasBlockMesh = true;
    }

    void SwapFace(Mesh m, int faceIndex)
    {
        int[] oldInds = m.GetIndices(0);
        int[] newInds = new int[oldInds.Length];
        Array.Copy(oldInds, newInds, oldInds.Length);

        newInds[3 * faceIndex + 1] = oldInds[3 * faceIndex + 2];
        newInds[3 * faceIndex + 2] = oldInds[3 * faceIndex + 1];

        m.SetIndices(newInds, MeshTopology.Triangles, 0);
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
