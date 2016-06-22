using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LineRenderer2 : MonoBehaviour {

    public Material material;

    public int pointsCount;

    public bool isClosed = false;

    private MeshFilter myMeshFilter = null;
    private MeshRenderer myMeshRenderer = null;

    void Start()
    {
        
    }

    void AutoCreateMeshData()
    {       
        if (myMeshFilter != null && myMeshRenderer != null) return;

        myMeshFilter = gameObject.GetComponent<MeshFilter>();
        myMeshRenderer = gameObject.GetComponent<MeshRenderer>();

        if (myMeshFilter != null && myMeshFilter.sharedMesh == null)
        {
            myMeshFilter.sharedMesh = new Mesh();
            myMeshFilter.sharedMesh.name = "line";
        }

        if (myMeshFilter != null && myMeshRenderer != null) return;

        myMeshFilter = gameObject.AddComponent<MeshFilter>();
        myMeshFilter.sharedMesh = new Mesh();
        myMeshFilter.sharedMesh.name = "line";

        myMeshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    [NonSerialized]
    [HideInInspector]

    public Vector3[] pointsToGenerate = null;
    float maxSegmentSize = -1.0f;

    [NonSerialized]
    [HideInInspector]
    public float totalLength = 0.0f;

    private float _vScale = 1.0f;
    public float vScale
    {
        get
        {
            return _vScale;
        }
        set
        {
            if (_vScale != value)
            {
                _vScale = value;
                UpdateLine();
            }
        }
    }

    private float _vOffset = 0.0f;
    public float vOffset
    {
        get
        {
            return _vOffset;
        }
        set
        {
            if (_vOffset != value)
            {
                _vOffset = value;
                UpdateLine();
            }
        }
    }

    public Vector3 upVector = Vector3.up;

    public void SetPositions(Vector3[] points, float _maxSegmentSize = -1.0f, bool dontUpdate = false)
    {
        maxSegmentSize = _maxSegmentSize;

        List<Vector3> validPoints = new List<Vector3>(points);

        if (maxSegmentSize > 0.0f && points != null && points.Length > 1)
        {
            validPoints.Clear();

            int count = points.Length - 1;
            if (isClosed)
                count++;

            for (int i = 0; i < count; i++ )
            {
                Vector3 dir = points[(i + 1) % points.Length] - points[i % points.Length];
                float len = dir.magnitude;

                if(len < _maxSegmentSize)
                {
                    validPoints.Add(points[i]);
                    continue;
                }

                dir /= len;

                float d = 0.0f;
                while(d < len)
                {
                    validPoints.Add(points[i] + dir * d);
                    d += _maxSegmentSize;
                }
            }

            if(isClosed == false)
                validPoints.Add(points[points.Length - 1]);
        }

        // remove duplicate points

        for (int i = 0; i < validPoints.Count; i++)
        {
            Vector3 d = validPoints[(i + 1) % validPoints.Count] - validPoints[i % validPoints.Count];
            if(d.sqrMagnitude < 0.0001f)
            {
                validPoints.RemoveAt(i % validPoints.Count);
                i--;
            }
        }

        pointsToGenerate = validPoints.ToArray();

        totalLength = 0.0f;
        if (validPoints.Count > 1)
        {
            Vector3 prev = validPoints[0];
            foreach (Vector3 p in validPoints)
            {
                totalLength += Vector3.Distance(p, prev);
                prev = p;
            }
        }

        if(!dontUpdate)
            UpdateLine();
    }

    float startWidth = 1.0f;
    float middleWidth = 1.0f;
    float endWidth = 1.0f;

    public void SetWidth(float w, bool dontUpdate = false)
    {
        startWidth = middleWidth = endWidth = w;

        if(!dontUpdate)
            UpdateLine();
    }

    public void SetWidths(float start, float middle, float end)
    {
        startWidth = start;
        middleWidth = middle;
        endWidth = end;
        UpdateLine();
    }

    public void UpdateLine(bool updateVertices = true, bool updateUVs = true, bool updateIndices = true)
    {
        AutoCreateMeshData();

        if(pointsToGenerate == null || pointsToGenerate.Length < 2)
        {
            myMeshRenderer.enabled = false;
            return;
        }


        myMeshRenderer.enabled = true;

        Mesh myMesh = myMeshFilter.sharedMesh;

        updateVertices = updateVertices || myMesh.vertexCount == 0;
        updateUVs = updateUVs || myMesh.uv == null ||myMesh.uv.Length == 0;
        updateIndices = updateIndices || myMesh.GetIndices(0).Length == 0;

        Vector3[] pos = updateVertices ? new Vector3[2 * pointsToGenerate.Length] : null;
        Vector2[] uv = updateUVs ? new Vector2[2 * pointsToGenerate.Length] : null;

        int numPoints = pointsToGenerate.Length;

        Vector3 prevRight = Vector3.zero;

        float dist = 0.0f;
        float uvv = 0.0f;

        int count = numPoints;
        if (isClosed)
            count++;

        for (int i = 0; i < count; i++)
        {
            Vector3 dir = Vector3.forward;
            if(i == numPoints - 1 && !isClosed)
            {
                dir = pointsToGenerate[numPoints - 1] - pointsToGenerate[numPoints - 2];
            }
            else
            {
                dir = pointsToGenerate[(i + 1) % numPoints] - pointsToGenerate[i % numPoints];
            }

            float d = dir.magnitude;
            dir /= d;

            int index = i % numPoints;

            if (updateVertices)
            {
                Vector3 right = Vector3.Normalize(Vector3.Cross(dir, upVector));

                if (Vector3.Dot(right, prevRight) < -0.1f)
                    right *= -1.0f;

                Vector3 rr = (right + prevRight); rr.Normalize();
                prevRight = right;

                float ff = 1.0f / Vector3.Dot(prevRight, rr);

                float w = getWidth(dist / totalLength);

                pos[2 * index] = pointsToGenerate[index] + rr * w * 0.5f * ff;
                pos[2 * index + 1] = pointsToGenerate[index] - rr * w * 0.5f * ff;
            }

            if (updateUVs)
            {
                float w = getWidth(dist / totalLength);

                uv[2 * index] = new Vector2(0.0f, _vScale * dist / middleWidth + _vOffset);
                uv[2 * index + 1] = new Vector2(1.0f, uv[2 * index].y);
                dist += d;
                uvv += w / middleWidth;
            }
        }

        if(updateVertices)
            myMesh.vertices = pos;

        if(updateIndices)
            myMesh.uv = uv;

        if (updateIndices)
        {
            int indexCount = 6 * (numPoints - 1);
            if (isClosed)
                indexCount += 6;

            int[] inds = new int[indexCount];

            for (int i = 0; i < indexCount / 6; i++)
            {
                int vertexIndex1 = i % numPoints;
                int vertexIndex2 = (i + 1) % numPoints;

                inds[6 * i + 0] = 2 * vertexIndex1 + 1;
                inds[6 * i + 1] = 2 * vertexIndex1 + 0;
                inds[6 * i + 2] = 2 * vertexIndex2;

                inds[6 * i + 3] = 2 * vertexIndex1 + 1;
                inds[6 * i + 4] = 2 * vertexIndex2 + 0;
                inds[6 * i + 5] = 2 * vertexIndex2 + 1;
            }

            myMesh.SetIndices(inds, MeshTopology.Triangles, 0);

            myMeshRenderer.material = material;
        }

        pointsCount = numPoints;
    }

    float getWidth(float t)
    {
        float tt = Mathf.Abs(t - 0.5f) / 0.5f;
        tt = tt * tt;

        if (t < 0.5f)
            return Mathf.Lerp(startWidth, middleWidth, 1.0f - tt);
        else
            if (t > 0.5f)
                return Mathf.Lerp(middleWidth, endWidth, tt);

        return middleWidth;
    }
}
