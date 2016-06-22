using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MeshExtrude : MonoBehaviour {

    public Vector3 extrusionDirection = Vector3.right;
    public float extrusionLength = 10.0f;

    public Vector3 offset = Vector3.zero;

    public MeshFilter sourceMesh;

    public Material material;

    [NonSerialized]
    public bool collapsedAlready = false;

    [NonSerialized]
    private List<MeshExtrude> collapseWith = new List<MeshExtrude>();

    public bool isDynamic = false;

    class MeshEdge
    {
        public Vector3 p1, p2;

        public List<Vector3> orientations = new List<Vector3>();

        public int refCount = 1;

        public MeshEdge(Vector3 p1, Vector3 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.refCount = 1;
        }

        public Vector3 vector
        {
            get
            {
                Vector3 d = p1 - p2;
                return d.normalized;
            }
        }

        public bool Equals(MeshEdge other)
        {
            Vector3 d1 = p1 - other.p1;
            Vector3 d2 = p2- other.p2;

            float limit = 0.001f;

            d1.y = d2.y = 0.0f;

            if (d1.sqrMagnitude < limit && d2.sqrMagnitude < limit) return true;

            d1 = p1 - other.p2;
            d2 = p2 - other.p1;

            d1.y = d2.y = 0.0f;

            return (d1.sqrMagnitude < limit && d2.sqrMagnitude < limit);
        }

        static bool ContainsEdge(ref List<MeshEdge> list, MeshEdge edge)
        {
            int find = FindEdge(ref list, edge);
            return find >= 0;
        }

        static int FindEdge(ref List<MeshEdge> list, MeshEdge edge)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Equals(edge))
                    return i;
            }

            return -1;
        }

        static public void GetMeshEdges(MeshFilter source, ref List<MeshEdge> res)
        {
            if (source.sharedMesh == null)
                return;

            int[] inds = source.sharedMesh.GetIndices(0);
            Vector3[] pos = source.sharedMesh.vertices;

            for(int i = 0; i < pos.Length; i++)
            {
                pos[i] = source.transform.TransformPoint(pos[i]);
            }

            Vector3[] points = new Vector3[3];

            for (int i = 0; i < inds.Length; i+=3 )
            {
                points[0] = pos[inds[i]];
                points[1] = pos[inds[i + 1]];
                points[2] = pos[inds[i + 2]];

                Vector3 n1 = points[1] - points[0]; n1.Normalize();
                Vector3 n2 = points[2] - points[1]; n2.Normalize();

                Vector3 trinormal = Vector3.Cross(n1, n2); trinormal.Normalize();

                for(int k = 0; k < 3; k++)
                {
                    Vector3 p1 = points[k];
                    Vector3 p2 = points[(k + 1) % 3];

                    MeshEdge e = new MeshEdge(p1, p2);
                    e.orientations.Add(Vector3.Cross(e.vector, trinormal).normalized);

                    int already = FindEdge(ref res, e);

                    if (already >= 0)
                    {
                        res[already].refCount++;
                    }
                    else
                    {
                        res.Add(e);
                    }
                }
            }

            // remove shared edges
            for (int i = 0; i < res.Count; i++)
            {
                if(res[i].refCount > 1)
                {
                    res.RemoveAt(i);
                    i--;
                }
            }
        }

        static public List<MeshEdge> GetSilouhette2d(List<MeshEdge> source, Vector3 viewdDir)
        {
            List<MeshEdge> res = new List<MeshEdge>();

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].refCount > 1) continue;

                int facingEdges = 0;
                int backfacingEdges = 0;
                for (int k = 0; k < source[i].orientations.Count; k++)
                {
                    if (Vector3.Dot(viewdDir, source[i].orientations[k]) > 0.0f)
                        backfacingEdges++;
                    else
                        facingEdges++;
                }

                if (facingEdges > 0 && backfacingEdges == 0)
                    res.Add(source[i]);
            }

            return res;
        }
    };

	// Use this for initialization
	void Start ()
    {
        CreateComponents();

        CreateExtrusion();
	}
	
    void CreateComponents()
    {
        if(gameObject.GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        if (gameObject.GetComponent<MeshRenderer>() == null)
        {
            MeshRenderer r = gameObject.AddComponent<MeshRenderer>();
            r.material = material;
        }
    }

    public void CreateExtrusion()
    {
        if (sourceMesh == null) return;

        CreateComponents();

        Mesh m = new Mesh();

        gameObject.GetComponent<MeshRenderer>().material = material;

        List<MeshEdge> edges = new List<MeshEdge>();
            
        MeshEdge.GetMeshEdges(sourceMesh, ref edges);

        for(int i = 0; i < collapseWith.Count; i++)
        {
            MeshEdge.GetMeshEdges(collapseWith[i].sourceMesh, ref edges);
            collapseWith[i].GetComponent<MeshRenderer>().enabled = false;
            //Debug.Log("Collapsed mesh from " + collapseWith[i].transform.parent.parent.gameObject.name + "/" + collapseWith[i].transform.parent.gameObject.name + " with " + transform.parent.parent.gameObject.name + "/" + transform.parent.gameObject.name);
        }

        Vector3 dir = extrusionDirection.normalized;

        edges = MeshEdge.GetSilouhette2d(edges, dir);

        if(edges.Count == 0)
            gameObject.GetComponent<MeshRenderer>().enabled = false;

        if (edges.Count == 0) return;

        int vertexCount = edges.Count * 4;

        Vector3[] pos = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];

        Vector3 center = Vector3.zero;
       
        for (int i = 0; i < edges.Count; i++)
        {
            int index = 4 * i;
            pos[index] = edges[i].p1;
            pos[index + 1] = edges[i].p2;

            pos[index + 2] = edges[i].p1 + dir * extrusionLength;
            pos[index + 3] = edges[i].p2 + dir * extrusionLength;

            uv[index] = new Vector2(0.0f, 0.0f);
            uv[index + 1] = new Vector2(0.0f, 0.0f);

            uv[index + 2] = new Vector2(0.0f, 1.0f);
            uv[index + 3] = new Vector2(0.0f, 1.0f);

            for (int k = 0; k < 4; k++)
                center += pos[index + k];
        }

        center /= (float)vertexCount;
        for (int i = 0; i < vertexCount; i++ )
        {
            pos[i] -= center;
        }

        m.vertices = pos;
        m.uv = uv;

        int[] inds = new int[6 * edges.Count];

        for (int i = 0; i < edges.Count; i++)
        {
            inds[6 * i + 0] = 4 * i + 1;
            inds[6 * i + 1] = 4 * i + 0;
            inds[6 * i + 2] = 4 * i + 2;

            inds[6 * i + 3] = 4 * i + 1;
            inds[6 * i + 4] = 4 * i + 2;
            inds[6 * i + 5] = 4 * i + 3;
        }

        m.SetIndices(inds, MeshTopology.Triangles, 0);

        m.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = m;
        m.name = "extruded mesh";

        transform.position = offset + center;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public void Append(MeshExtrude other)
    {
        collapseWith.Add(other);
        CreateExtrusion();
        other.collapsedAlready = true;
    }

    void OnValidate()
    {
        if (Application.isPlaying == false)
            CreateExtrusion();
    }

    public void Reset()
    {
        collapsedAlready = false;
        collapseWith.Clear();
        GetComponent<MeshRenderer>().enabled = false;
    }
}
