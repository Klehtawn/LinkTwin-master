using UnityEngine;
using System.Collections;

public class MeshVertexColors : MonoBehaviour {

    Mesh myMesh = null;
	void Start () {
        getMyMesh();
	}
	
	public Color color
    {
        get
        {
            getMyMesh();

            if (myMesh.colors == null || myMesh.colors.Length == 0) return Color.white;
            return myMesh.colors[0];
        }
        set
        {
            getMyMesh();

            Color[] c = new Color[myMesh.vertexCount];
            for (int i = 0; i < myMesh.vertexCount; i++)
                c[i] = value;
            myMesh.colors = c;
        }
    }

    void getMyMesh()
    {
        if (myMesh == null)
            myMesh = GetComponentInChildren<MeshFilter>().mesh;
    }
}
