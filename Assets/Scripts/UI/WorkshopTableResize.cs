using UnityEngine;
using System.Collections;
using System;

public class WorkshopTableResize : MonoBehaviour {

    public WorkshopResizeTableButton resizeTopRight, resizeBottomLeft;

    [HideInInspector]
    public Action OnDragFinished;

    [HideInInspector]
    public Camera tableCamera;

    Blocks blocks = new Blocks();
	// Use this for initialization
	void Start () {

        resizeBottomLeft.OnTouchMoved += OnResizeBottomLeftTouchMoved;
        resizeTopRight.OnTouchMoved += OnResizeTopRightTouchMoved;
        resizeBottomLeft.OnTouchDown += OnResizeBottomLeftTouchDown;
        resizeTopRight.OnTouchDown += OnResizeTopRightTouchDown;
        resizeBottomLeft.OnTouchUp += OnResizeBottomLeftTouchUp;
        resizeTopRight.OnTouchUp += OnResizeTopRightTouchUp;

        resizeBottomLeft.visible = false;
        resizeTopRight.visible = false;

        //blocks.Fill(tableCamera.transform.parent);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void RepositionResizeButtons()
    {
        if (blocks.ground.Count == 0)
        {
            resizeTopRight.visible = false;
            resizeBottomLeft.visible = false;
            return;
        }

        resizeTopRight.visible = true;
        resizeBottomLeft.visible = true;

        Vector3 min = blocks.GetGroundMin() - Vector3.one * GameSession.gridUnit * 0.5f;
        Vector3 max = blocks.GetGroundMax() + Vector3.one * GameSession.gridUnit * 0.5f;

        Vector3 p = tableCamera.WorldToViewportPoint(min);
        p.x *= Desktop.main.width;
        p.y *= Desktop.main.height;
        resizeBottomLeft.pos = p - Vector3.one * resizeBottomLeft.width * 0.5f;

        p = tableCamera.WorldToViewportPoint(max);
        p.x *= Desktop.main.width;
        p.y *= Desktop.main.height;
        resizeTopRight.pos = p + Vector3.one * resizeBottomLeft.width * 0.5f;
    }

//    Vector3 tableStartSize;
    Vector2 resizeBottomLeftButtonPos, resizeTopRightButtonPos;
    Vector2 startDragPosBottomLeft, startDragPosTopRight;
    void OnResizeBottomLeftTouchDown(MonoBehaviour sender, Vector2 currentPos)
    {
//        tableStartSize = blocks.GetGroundSize() / GameSession.gridUnit;

        resizeBottomLeftButtonPos = resizeBottomLeft.pos;
        startDragPosBottomLeft = resizeBottomLeftButtonPos;

        ComputeGridScreenSize();
    }

    void OnResizeBottomLeftTouchUp(MonoBehaviour sender, Vector2 currentPos)
    {
        if (OnDragFinished != null)
            OnDragFinished();
        RepositionResizeButtons();
    }

    void OnResizeTopRightTouchDown(MonoBehaviour sender, Vector2 currentPos)
    {
//        tableStartSize = blocks.GetGroundSize() / GameSession.gridUnit;

        resizeTopRightButtonPos = resizeTopRight.pos;
        startDragPosTopRight = resizeTopRightButtonPos;

        ComputeGridScreenSize();
    }
    void OnResizeTopRightTouchUp(MonoBehaviour sender, Vector2 currentPos)
    {
        if (OnDragFinished != null)
            OnDragFinished();
        RepositionResizeButtons();
    }

    void OnResizeBottomLeftTouchMoved(MonoBehaviour sender, Vector2 startPos, Vector2 currentPos)
    {
        resizeBottomLeft.pos = currentPos - startPos + resizeBottomLeftButtonPos;

        Vector2 d = resizeBottomLeft.pos - startDragPosBottomLeft;

        if (Mathf.Abs(d.x) < gridScreenSize && Mathf.Abs(d.y) < gridScreenSize) return;

        Vector3 min = blocks.GetGroundMin();
        Vector3 max = blocks.GetGroundMax();

        if (Vector3.Distance(min, max) < GameSession.gridUnit) return;

        float threshold = GameSession.gridUnit * 0.7f;

        if (d.x < -threshold)
        {
            Vector3 p = min;
            while(p.z < max.z + GameSession.gridUnit)
            {
                InsertGroundBlock(p, Vector3.left);
                p.z += GameSession.gridUnit;
            }

            min = blocks.GetGroundMin();
            max = blocks.GetGroundMax();
        }
        
        if (d.y < -threshold)
        {
            Vector3 p = min;
            while (p.x < max.x + GameSession.gridUnit)
            {
                InsertGroundBlock(p, Vector3.back);
                p.x += GameSession.gridUnit;
            }

            min = blocks.GetGroundMin();
            max = blocks.GetGroundMax();
        }

        Vector3 sz = (max - min) / GameSession.gridUnit;
        if (d.x > threshold && sz.x > 2.0f)
        {
            Vector3 p = min;
            while (p.z < max.z + GameSession.gridUnit)
            {
                DeleteGroundBlock(p);
                p.z += GameSession.gridUnit;
            }
        }

        if (d.y > threshold && sz.x > 2.0f)
        {
            Vector3 p = min;
            while (p.x < max.x + GameSession.gridUnit)
            {
                DeleteGroundBlock(p);
                p.x += GameSession.gridUnit;
            }
        }

        startDragPosBottomLeft = resizeBottomLeft.pos;
        blocks.Fill(tableCamera.transform.parent);
    }

    void OnResizeTopRightTouchMoved(MonoBehaviour sender, Vector2 startPos, Vector2 currentPos)
    {
        resizeTopRight.pos = currentPos - startPos + resizeTopRightButtonPos;

        Vector2 d = resizeTopRight.pos - startDragPosTopRight;

        if (Mathf.Abs(d.x) < gridScreenSize && Mathf.Abs(d.y) < gridScreenSize) return;

        Vector3 min = blocks.GetGroundMin();
        Vector3 max = blocks.GetGroundMax();

        float threshold = GameSession.gridUnit * 0.7f;

        Vector3 maxSz = new Vector3(20.0f, 0.0f, 20.0f);
        Vector3 sz = (max - min) / GameSession.gridUnit;

        if (d.x > threshold)
        {
            Vector3 p = max;
            while (p.z > min.z - GameSession.gridUnit && sz.x < maxSz.x)
            {
                InsertGroundBlock(p, Vector3.right);
                p.z -= GameSession.gridUnit;
            }

            blocks.Fill(tableCamera.transform.parent);
            min = blocks.GetGroundMin();
            max = blocks.GetGroundMax();
            sz = (max - min) / GameSession.gridUnit;
        }
        
        if (d.y > threshold)
        {
            Vector3 p = max;
            while (p.x > min.x - GameSession.gridUnit && sz.z < maxSz.z)
            {
                InsertGroundBlock(p, Vector3.forward);
                p.x -= GameSession.gridUnit;
            }

            blocks.Fill(tableCamera.transform.parent);
            min = blocks.GetGroundMin();
            max = blocks.GetGroundMax();
            sz = (max - min) / GameSession.gridUnit;
        }

        if (d.x < -threshold && sz.x > 2.0f)
        {
            Vector3 p = max;
            while (p.z > min.z - GameSession.gridUnit)
            {
                DeleteGroundBlock(p);
                p.z -= GameSession.gridUnit;
            }
        }

        if (d.y < -threshold && sz.z > 2.0f)
        {
            Vector3 p = max;
            while (p.x > min.x - GameSession.gridUnit)
            {
                DeleteGroundBlock(p);
                p.x -= GameSession.gridUnit;
            }
        }

        startDragPosTopRight = resizeTopRight.pos;
        blocks.Fill(tableCamera.transform.parent);
    }

    float gridScreenSize;
    void ComputeGridScreenSize()
    {
        Vector3 p = tableCamera.WorldToViewportPoint(Vector3.right * GameSession.gridUnit) - tableCamera.WorldToViewportPoint(Vector3.zero);
        gridScreenSize = p.x * Desktop.main.width;
    }

    void InsertGroundBlock(Vector3 pos, Vector3 side)
    {
        Block original = blocks.GroundAt(pos);
        if (original != null)
        {
            Vector3 newPos = original.transform.position + side * GameSession.gridUnit;
            if (blocks.GroundAt(newPos) == null)
            {
                GameObject newBlock = GameObject.Instantiate<GameObject>(original.gameObject);
                newBlock.transform.SetParent(original.transform.parent);
                newBlock.transform.position = newPos;

                blocks.ground.Add(newBlock.GetComponent<Block>());
            }
        }
    }

    void DeleteGroundBlock(Vector3 pos)
    {
        Block b = blocks.GroundAt(pos);
        if (b != null)
        {
            b.transform.parent = null;
            GameObject.Destroy(b.gameObject);
        }
    }

    public void Refresh()
    {
        if (tableCamera == null) return;
        blocks.Fill(tableCamera.transform.parent);
        RepositionResizeButtons();
    }
}
