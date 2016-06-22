using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class RandomizeMesh : MonoBehaviour {

	// Use this for initialization

    public bool splitTriangles = true;
    public float perturb = 0.0f;

    public bool randomizeColorRed = true;
    public Vector2 randomizeColorRedInterval = new Vector2(0.0f, 1.0f);

    public bool randomizeColorBlue = true;
    public Vector2 randomizeColorBlueInterval = new Vector2(0.0f, 1.0f);

    public bool randomizeColorGreen = true;
    public Vector2 randomizeColorGreenInterval = new Vector2(0.0f, 1.0f);

    public bool facesHaveTheSameColor = true;

    public bool sendFaceCenterAsTangent = true;

    void Start()
    {
        if (splitTriangles)
            Split();

        if (randomizeColorRed || randomizeColorBlue || randomizeColorGreen)
        {
            RandomizeColors();
        }

        if(facesHaveTheSameColor)
        {
            ColorizeFaces();
        }

        if(sendFaceCenterAsTangent)
        {
            SendFaceCentersAsTangents();
        }
    }

    public void Split()
    {
        Mesh m = GetComponent<MeshFilter>().mesh;

        int[] indices = m.GetIndices(0);
        int[] newIndices = new int[indices.Length];
        for (int i = 0; i < newIndices.Length; i++)
            newIndices[i] = i;

        int numTris = indices.Length / 3;
        int newVertsCount = numTris * 3;

        Vector3[] oldVertices = m.vertices;
        Vector2[] oldUVs = m.uv;

        Vector3[] newVertices = new Vector3[newVertsCount];
        Vector3[] newNormals = new Vector3[newVertsCount];
        Vector2[] newUVs = new Vector2[newVertsCount];
        for (int i = 0; i < numTris; i++)
        {
            Vector3 v1 = oldVertices[indices[3 * i]];
            Vector3 v2 = oldVertices[indices[3 * i + 1]];
            Vector3 v3 = oldVertices[indices[3 * i + 2]];
            v1 = v2 - v1; v1.Normalize();
            v3 = v3 - v2; v3.Normalize();

            Vector3 nrm = Vector3.Cross(v1, v3) + Random.insideUnitSphere * perturb;
            nrm.Normalize();

            for (int k = 0; k < 3; k++)
            {
                int index = indices[3 * i + k];
                newVertices[3 * i + k] = oldVertices[index];
                if (oldUVs != null)
                    newUVs[3 * i + k] = oldUVs[index];
                newNormals[3 * i + k] = nrm;
            }
        }

        m.vertices = newVertices;
        m.normals = newNormals;
        if (oldUVs != null)
            m.uv = newUVs;

        m.MarkDynamic();

        m.SetIndices(newIndices, MeshTopology.Triangles, 0);
    }

    public void RandomizeColors()
    {
        Mesh m = GetComponent<MeshFilter>().mesh;

        Color[] colors = new Color[m.vertexCount];

        for(int i = 0; i < colors.Length; i++)
        {
            if (randomizeColorRed)
                colors[i].r = Random.Range(randomizeColorRedInterval.x, randomizeColorRedInterval.y);

            if (randomizeColorGreen)
                colors[i].g = Random.Range(randomizeColorGreenInterval.x, randomizeColorGreenInterval.y);

            if (randomizeColorBlue)
                colors[i].b = Random.Range(randomizeColorBlueInterval.x, randomizeColorBlueInterval.y);
        }

        m.colors = colors;
    }

    public void ColorizeFaces()
    {
        Mesh m = GetComponent<MeshFilter>().mesh;

        Faces_t faces = new Faces_t(m);

        Color[] oldColors = m.colors;
        Color[] newColors = new Color[m.vertexCount];

        for (int i = 0; i < faces.faces.Count; i++)
        {
            Face_t f = faces.faces[i];

            for(int k = 0; k < f.indices.Count; k++)
            {
                newColors[f.indices[k]] = oldColors[f.indices[0]];
            }
        }

        m.colors = newColors;
    }

    class Face_t
    {
        public List<int> indices = new List<int>();

        public bool IsLinkedWithTriangle(int[] corners)
        {
            int count = 0;
            for (int i = 0; i < corners.Length; i++)
            {
                if (indices.Contains(corners[i]))
                    count++;
            }

            return count >= 2;
        }

        public void PushTriangle(int[] corners)
        {
            for (int i = 0; i < corners.Length; i++)
            {
                if (indices.Contains(corners[i]) == false)
                    indices.Add(corners[i]);
            }
        }
    };

    class Faces_t
    {
        public List<Face_t> faces = new List<Face_t>();

        public void PushTriangle(int[] corners)
        {
            bool added = false;
            for(int i = 0; i < faces.Count; i++)
            {
                if (faces[i].IsLinkedWithTriangle(corners))
                {
                    added = true;
                    faces[i].PushTriangle(corners);
                    break;
                }
            }

            if(added == false)
            {
                Face_t f = new Face_t();
                f.PushTriangle(corners);
                faces.Add(f);
            }
        }

        public void PushMesh(Mesh m)
        {
            int[] inds = m.GetIndices(0);
            for(int i = 0; i < inds.Length; i += 3)
            {
                int[] corners = new int[3];
                for(int k = 0; k < 3; k++)
                {
                    corners[k] = inds[i + k];
                }

                PushTriangle(corners);
            }
        }

        public Faces_t()
        {

        }

        public Faces_t(Mesh m)
        {
            PushMesh(m);
        }
    }

    public void SendFaceCentersAsTangents()
    {
        Mesh m = GetComponent<MeshFilter>().mesh;

        Faces_t faces = new Faces_t(m);

        Vector3[] verts = m.vertices;
        Vector4[] tangents = new Vector4[m.vertexCount];

        for (int i = 0; i < faces.faces.Count; i++)
        {
            Face_t face = faces.faces[i];
            Vector3 center = Vector3.zero;
            for (int k = 0; k < face.indices.Count; k++)
            {
                center += verts[face.indices[k]];
            }

            center /= (float)face.indices.Count;

            for (int k = 0; k < face.indices.Count; k++)
            {
                tangents[face.indices[k]] = center;
            }
        }

        m.tangents = tangents;
    }
}
