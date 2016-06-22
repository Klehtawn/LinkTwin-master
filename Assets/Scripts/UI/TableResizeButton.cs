using UnityEngine;
using System.Collections;

public class TableResizeButton : Widget {

    WorkshopScreen workshopScreen;

    public enum Position
    {
        Top,
        Down,
        Left,
        Right
    };

    public Position positioning = Position.Top;
	
	protected override void Start ()
    {
        base.Start();

        workshopScreen = GetComponentInParent<WorkshopScreen>();

        OnTouchUp += _OnTouchUp;
        OnTouchDown += _OnTouchDown;
        OnTouchMoved += _OnTouchMoved;
	}

    Vector3 groundMin = Vector3.zero;
    Vector3 groundMax = Vector3.zero;
	
	protected override void Update ()
    {
        base.Update();

        if (workshopScreen.tableRoot != null)
            workshopScreen.tableRoot.blocks.GetGroundMinMax(ref groundMin, ref groundMax);

        Reposition();
	}

    Bounds tableRect = new Bounds();

    void Reposition()
    {
        if (touchDown) return;

        if (workshopScreen.tableCamera == null) return;

        Vector3 min = groundMin - Vector3.one * GameSession.gridUnit * 0.5f;
        Vector3 max = groundMax + Vector3.one * GameSession.gridUnit * 0.5f;

        min = Desktop.main.ViewportToDesktop(workshopScreen.tableCamera.WorldToViewportPoint(min));
        max = Desktop.main.ViewportToDesktop(workshopScreen.tableCamera.WorldToViewportPoint(max));


        Vector3 center = (max + min) * 0.5f;
        if(positioning == Position.Top) // is up
        {
            pos = new Vector3(center.x, max.y, 0.0f);
        }
        else
            if (positioning == Position.Down) // is down
        {
            pos = new Vector3(center.x, min.y, 0.0f);
        }
        else
        if (positioning == Position.Left) // is left
        {
            pos = new Vector3(min.x, center.y, 0.0f);
        }
        else
        if (positioning == Position.Right) // is right
        {
            pos = new Vector3(max.x, center.y, 0.0f);
        }

        min.z = max.z = 0.0f;

        tableRect.SetMinMax(min, max);
    }

    void _OnTouchUp(MonoBehaviour sender, Vector2 p)
    {
        touchDown = false;
        workshopScreen.CenterCamera();
    }

    Vector2 startTouch = Vector2.zero;
    Vector2 startPos = Vector2.zero;
    bool touchDown = false;
    void _OnTouchDown(MonoBehaviour sender, Vector2 p)
    {
        startTouch = screenPos + p;
        touchDown = true;
        startPos = pos;
    }

    void _OnTouchMoved(MonoBehaviour sender, Vector2 start, Vector2 current)
    {
        if(touchDown)
        {
            Vector2 p = current + screenPos;

            Vector2 newPos = startPos + p - startTouch;

            if (isHorizontal)
                newPos.x = startPos.x;
            else
                newPos.y = startPos.y;

            pos = newPos;

            if(Vector2.Distance(pos, startPos) > workshopScreen.GetGridScreenSize())
            {
                Vector3 sz = (groundMax - groundMin) / GameSession.gridUnit;
                if(tableRect.Contains(pos)) // is inside -> must delete
                {
                    if(isHorizontal)
                    {
                        if (sz.z >= 3.0f)
                        {
                            if (positioning == Position.Top)
                                DeleteRow(groundMax.z);
                            else
                                DeleteRow(groundMin.z);
                        }
                    }
                    else
                    {
                        if (sz.x >= 3.0f)
                        {
                            if (positioning == Position.Left)
                                DeleteColumn(groundMin.x);
                            else
                                DeleteColumn(groundMax.x);
                        }
                    }
                }
                else
                {
                    if (isHorizontal)
                    {
                        if (sz.z < 20.0f)
                        {
                            if (positioning == Position.Top)
                                DuplicateRow(groundMax.z);
                            else
                                DuplicateRow(groundMin.z);
                        }
                    }
                    else
                    {
                        if (sz.x < 20.0f)
                        {
                            if (positioning == Position.Left)
                                DuplicateColumn(groundMin.x);
                            else
                                DuplicateColumn(groundMax.x);
                        }
                    }
                }

                startPos = pos;
                startTouch = current + screenPos;
            }
        }
    }

    bool isHorizontal
    {
        get
        {
            return positioning == Position.Top || positioning == Position.Down;
        }
    }

    bool isVertical
    {
        get
        {
            return positioning == Position.Left || positioning == Position.Right;
        }
    }

    void DeleteRow(float zPos)
    {
        Vector3 p = groundMin; p.z = zPos;
        while(p.x <= groundMax.x)
        {
            DeleteGroundBlock(p);
            p.x += GameSession.gridUnit;
        }

        workshopScreen.tableRoot.RefreshStructure();
    }

    void DeleteColumn(float xPos)
    {
        Vector3 p = groundMin; p.x = xPos;
        while (p.z <= groundMax.z)
        {
            DeleteGroundBlock(p);
            p.z += GameSession.gridUnit;
        }

        workshopScreen.tableRoot.RefreshStructure();
    }

    void DuplicateRow(float zPos)
    {
        Vector3 center = (groundMin + groundMax) * 0.5f;

        Vector3 p = groundMin; p.z = zPos;
        while (p.x <= groundMax.x)
        {
            InsertGroundBlock(p, zPos > center.z ? Vector3.forward : Vector3.back);
            p.x += GameSession.gridUnit;
        }

        workshopScreen.tableRoot.RefreshStructure();
    }

    void DuplicateColumn(float xPos)
    {
        Vector3 center = (groundMin + groundMax) * 0.5f;
        Vector3 p = groundMin; p.x = xPos;
        while (p.z <= groundMax.z)
        {
            InsertGroundBlock(p, xPos > center.x ? Vector3.right : Vector3.left);
            p.z += GameSession.gridUnit;
        }

        workshopScreen.tableRoot.RefreshStructure();
    }

    void DeleteGroundBlock(Vector3 pos)
    {
        Block b = workshopScreen.tableRoot.blocks.GroundAt(pos);
        if (b != null)
        {
            b.transform.parent = null;
            GameObject.Destroy(b.gameObject);
        }
    }

    void InsertGroundBlock(Vector3 pos, Vector3 side)
    {
        Block original = workshopScreen.tableRoot.blocks.GroundAt(pos);
        if (original != null)
        {
            Vector3 newPos = original.transform.position + side * GameSession.gridUnit;
            if (workshopScreen.tableRoot.blocks.GroundAt(newPos) == null)
            {
                GameObject newBlock = GameObject.Instantiate<GameObject>(original.gameObject);
                newBlock.transform.SetParent(original.transform.parent);
                newBlock.transform.position = newPos;
            }
        }
    }
}