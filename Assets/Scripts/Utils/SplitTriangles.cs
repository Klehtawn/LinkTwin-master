using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
public class SplitTriangles : MonoBehaviour {

	// Use this for initialization

    public float perturb = 0.0f;

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

        m.SetIndices(newIndices, MeshTopology.Triangles, 0);
    }
}
