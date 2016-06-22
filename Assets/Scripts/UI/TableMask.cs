using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class TableMask : MonoBehaviour {

    [HideInInspector]
    public Camera gameCamera;    

    Transform highlightMask;

    Transform highlightsRoot;
    Camera cam;
	void Awake ()
    {
        cam = transform.GetComponentInChildren<Camera>();

        highlightMask = transform.Find("HighlightMask");
        highlightMask.gameObject.SetActive(false);

        GameObject o = new GameObject();
        o.transform.SetParent(transform);
        o.name = "Highlights";
        highlightsRoot = o.transform;
        o.layer = highlightMask.gameObject.layer;

        CreateTexture();
	}

    void OnDestroy()
    {
        RenderTexture rt = cam.targetTexture;
        Destroy(rt);
    }
	
	// Update is called once per frame
	void Update ()
    {
        cam.orthographicSize = gameCamera.orthographicSize;
        cam.transform.position = gameCamera.transform.position;
        cam.transform.rotation = gameCamera.transform.rotation;

        cam.enabled = maskAlpha > 0.0f;

        if(Mathf.Abs(targetAlpha - maskAlpha) > 0.01f)
        {
            maskAlpha = Mathf.Lerp(maskAlpha, targetAlpha, 7.0f * Time.deltaTime);

            if (maskAlpha < 0.01f && targetAlpha == 0.0f)
                Widget.DeleteAllChildren(highlightsRoot);
        }
	}

    void CreateTexture()
    {
        RenderTexture maskTex = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        maskTex.filterMode = FilterMode.Point;
        maskTex.generateMips = false;
        maskTex.Create();
        cam.targetTexture = maskTex;
    }

    public void ClearHighlights()
    {
        Show(false);
    }

    public void HighlightBlocks(ref List<Block> list)
    {
        if(list.Count == 0)
        {
            ClearHighlights();
            return;
        }

        Show();

        Widget.DeleteAllChildren(highlightsRoot);

        for(int i = 0; i < list.Count; i++)
        {
            GameObject o = GameObject.Instantiate<GameObject>(highlightMask.gameObject);
            o.transform.SetParent(highlightsRoot);
            o.transform.position = list[i].transform.position;
            o.SetActive(true);
        }

        Widget.SetLayer(highlightsRoot.gameObject, LayerMask.NameToLayer("TableMask"));
        Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("TableMask"));
    }

    public void HighlightBlocks(ref List<Vector3> positions)
    {
        if (positions.Count == 0)
        {
            ClearHighlights();
            return;
        }

        Show();

        Widget.DeleteAllChildren(highlightsRoot);

        for (int i = 0; i < positions.Count; i++)
        {
            GameObject o = GameObject.Instantiate<GameObject>(highlightMask.gameObject);
            o.transform.SetParent(highlightsRoot);
            o.transform.position = positions[i];
            o.SetActive(true);
        }

        Widget.SetLayer(highlightsRoot.gameObject, LayerMask.NameToLayer("TableMask"));
    }

    public bool Contains(Vector3 pos)
    {
        if (highlightsRoot.childCount == 0)
            return true;

        for(int i = 0; i < highlightsRoot.childCount; i++)
        {
            Transform c = highlightsRoot.GetChild(i);
            Vector3 d = pos - c.position; d.y = 0.0f;
            if (d.sqrMagnitude < 0.05f)
                return true;
        }

        return false;
    }

    float maskAlpha = 0.0f;
    float targetAlpha = 0.0f;
    const float maskTransparency = 150.0f / 255.0f;

    void Show(bool show = true)
    {
        targetAlpha = show ? maskTransparency : 0.0f;
    }

    public float GetMaskAlpha()
    {
        return maskAlpha;
    }

    public Texture GetMaskTexture()
    {
        return cam.targetTexture;
    }

    public int Count
    {
        get
        {
            return highlightsRoot.childCount;
        }
    }

    public bool GetHighlightAtPos(Vector3 pos, ref Vector3 ret)
    {
        float halfGrid = GameSession.gridUnit * 0.5f;
        for (int i = 0; i < highlightsRoot.childCount; i++)
        {
            Transform c = highlightsRoot.GetChild(i);
            Vector3 d = c.transform.position - pos;
            if(Mathf.Abs(d.x) <= halfGrid && Mathf.Abs(d.z) <= halfGrid)
            {
                ret = c.position;
                return true;
            }
        }

        return false;
    }
}
