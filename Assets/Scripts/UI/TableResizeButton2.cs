using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TableResizeButton2 : Widget {

    WorkshopScreen workshopScreen;

    public enum Position
    {
        Top,
        Down,
        Left,
        Right
    };



    public Position positioning = Position.Top;

    RectTransform background;
    Image arrowLeft, arrowRight;
	
	protected override void Start ()
    {
        base.Start();

        workshopScreen = GetComponentInParent<WorkshopScreen>();

        OnTouchUp += _OnTouchUp;
        OnTouchDown += _OnTouchDown;
        OnTouchMoved += _OnTouchMoved;

        background = transform.Find("Bkg").GetComponent<RectTransform>();
        arrowLeft = transform.Find("ArrowLeft").GetComponent<Image>();
        arrowRight = transform.Find("ArrowRight").GetComponent<Image>();
	}

    Vector3 groundMin = Vector3.zero;
    Vector3 groundMax = Vector3.zero;
	
	protected override void Update ()
    {
        base.Update();

        if (workshopScreen.tableRoot != null)
            workshopScreen.tableRoot.blocks.GetGroundMinMax(ref groundMin, ref groundMax);

        Reposition();

        float minSize = (float)WorkshopScreen.minTableCells;
        float maxSize = (float)WorkshopScreen.maxTableCells - 1.0f;

        Vector3 sz = (groundMax - groundMin) / GameSession.gridUnit;

        ShowCompleteButton();

        if (isVertical)
        {
            if (sz.x >= maxSize)
            {
                HideArrowLeftOrPlus();
            }
            else if (sz.x < minSize)
            {
                HideArrowRightOrMinus();
            }
        }
        else
        {
            if (sz.z >= maxSize)
            {
                HideArrowLeftOrPlus();
            }
            else if (sz.z < minSize)
            {
                HideArrowRightOrMinus();
            }
        }

        UpdateButtonLayout();
	}

    Bounds tableRect = new Bounds();

    void Reposition()
    {
        if (touchDown) return;

        if (workshopScreen.tableCamera == null) return;

        Vector3 min = groundMin - Vector3.one * GameSession.gridUnit * 0.5f;
        Vector3 max = groundMax + Vector3.one * GameSession.gridUnit * 0.5f;

        //if (false)
        //{
        //    min = Desktop.main.ViewportToDesktop(workshopScreen.tableCamera.WorldToViewportPoint(min));
        //    max = Desktop.main.ViewportToDesktop(workshopScreen.tableCamera.WorldToViewportPoint(max));
        //}
        //else
        {
            min = Desktop.main.ViewportToDesktop(new Vector3(0.15f, 0.2f, 0.0f));
            max = Desktop.main.ViewportToDesktop(new Vector3(0.85f, 0.8f, 0.0f));
        }

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
        Vector2 pp = Desktop.main.DesktopToScreen(p) + screenPos;
        pp = Desktop.main.ScreenToDesktop(pp);

        if (Vector2.Distance(pp, startTouch) > Desktop.main.width * 0.05f) return;

        float minSize = (float)WorkshopScreen.minTableCells;
        float maxSize = (float)WorkshopScreen.maxTableCells - 1.0f;

        Vector3 sz = (groundMax - groundMin) / GameSession.gridUnit;

        if (tableRect.Contains(pp)) // is inside -> must delete
        {
            if (isHorizontal)
            {
                if (sz.z >= minSize)
                {
                    if (positioning == Position.Top)
                        DeleteRow(groundMax.z);
                    else
                        DeleteRow(groundMin.z);
                }
            }
            else
            {
                if (sz.x >= minSize)
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
                if (sz.z < maxSize)
                {
                    if (positioning == Position.Top)
                        DuplicateRow(groundMax.z);
                    else
                        DuplicateRow(groundMin.z);
                }
            }
            else
            {
                if (sz.x < maxSize)
                {
                    if (positioning == Position.Left)
                        DuplicateColumn(groundMin.x);
                    else
                        DuplicateColumn(groundMax.x);
                }
            }
        }

        touchDown = false;
        workshopScreen.CenterCamera();
    }

    Vector2 startTouch = Vector2.zero;
//    Vector2 startPos = Vector2.zero;
    bool touchDown = false;
    void _OnTouchDown(MonoBehaviour sender, Vector2 p)
    {
        startTouch = Desktop.main.DesktopToScreen(p) + screenPos;
        startTouch = Desktop.main.ScreenToDesktop(startTouch);
        touchDown = true;
//        startPos = pos;
    }

    void _OnTouchMoved(MonoBehaviour sender, Vector2 start, Vector2 current)
    {
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

    void DeleteRow(float zPos, bool deleteAlsoItems = true)
    {
        Vector3 center = (groundMin + groundMax) * 0.5f;

        Vector3 p = groundMin; p.z = zPos;

        Vector3 side = zPos > center.z ? Vector3.forward : Vector3.back;

        while(p.x <= groundMax.x)
        {
            DeleteGroundBlock(p);
            if (deleteAlsoItems)
                DeleteBlockExceptWall(p);

            MoveWall(p + side * GameSession.gridUnit, -side);

            p.x += GameSession.gridUnit;
        }

        workshopScreen.tableRoot.RefreshStructure();
    }

    void DeleteColumn(float xPos, bool deleteAlsoItems = true)
    {
        Vector3 center = (groundMin + groundMax) * 0.5f;

        Vector3 p = groundMin; p.x = xPos;

        Vector3 side = xPos > center.x ? Vector3.right : Vector3.left;

        while (p.z <= groundMax.z)
        {
            DeleteGroundBlock(p);
            if (deleteAlsoItems)
                DeleteBlockExceptWall(p);

            MoveWall(p + side * GameSession.gridUnit, -side);

            p.z += GameSession.gridUnit;
        }

        workshopScreen.tableRoot.RefreshStructure();
    }

    void DuplicateRow(float zPos)
    {
        Vector3 center = (groundMin + groundMax) * 0.5f;

        Vector3 p = groundMin; p.z = zPos;

        Vector3 side = zPos > center.z ? Vector3.forward : Vector3.back;

        while (p.x <= groundMax.x)
        {
            DuplicateGroundBlock(p, side);
            MoveWall(p + side * GameSession.gridUnit, side);
            p.x += GameSession.gridUnit;
        }

        workshopScreen.tableRoot.RefreshStructure();
    }

    void DuplicateColumn(float xPos)
    {
        Vector3 center = (groundMin + groundMax) * 0.5f;

        Vector3 p = groundMin; p.x = xPos;

        Vector3 side = xPos > center.x ? Vector3.right : Vector3.left;

        while (p.z <= groundMax.z)
        {
            DuplicateGroundBlock(p, side);
            MoveWall(p + side * GameSession.gridUnit, side);
            p.z += GameSession.gridUnit;
        }

        workshopScreen.tableRoot.RefreshStructure();
    }

    void DeleteGroundBlock(Vector3 pos)
    {
        Block b = workshopScreen.tableRoot.blocks.GroundAt(pos);
        DeleteBlock(b);
    }

    void DeleteBlock(Vector3 pos)
    {
        Block b = workshopScreen.tableRoot.blocks.ItemAt(pos);
        if (b != null && (b as GroundBlock) == null)
        {
            DeleteBlock(b);
        }
    }

    void DeleteBlockExceptWall(Vector3 pos)
    {
        Block b = workshopScreen.tableRoot.blocks.ItemAt(pos);
        if (b != null && (b as GroundBlock) == null && (b as Wall) == null)
        {
            DeleteBlock(b);
        }
    }

    void DeleteBlock(Block b)
    {
        if (b != null)
        {
            b.transform.parent = null;
            GameObject.Destroy(b.gameObject);
        }
    }

    void DuplicateGroundBlock(Vector3 pos, Vector3 side)
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

    void MoveWall(Vector3 pos, Vector3 side)
    {
        Block b = workshopScreen.tableRoot.blocks.ItemAt(pos);
        if (b != null && b.blockType == Block.BlockType.Wall)
        {
            b.position = b.transform.position + side * GameSession.gridUnit;
        }
    }

    float arrowLeftTargetAlpha = 1.0f;
    float arrowRightTargetAlpha = 1.0f;
    float bkgTargetAnchorMin = 0.0f;
    float bkgTargetAnchorMax = 0.0f;
    void UpdateButtonLayout()
    {
        float timeMultiplier = 10.0f * Time.deltaTime;
        if(Mathf.Abs(arrowLeft.color.a - arrowLeftTargetAlpha) > 0.05f)
        {
            Color c = arrowLeft.color;
            c.a = Mathf.Lerp(c.a, arrowLeftTargetAlpha, timeMultiplier);
            arrowLeft.color = c;
        }

        if (Mathf.Abs(arrowRight.color.a - arrowRightTargetAlpha) > 0.05f)
        {
            Color c = arrowRight.color;
            c.a = Mathf.Lerp(c.a, arrowRightTargetAlpha, timeMultiplier);
            arrowRight.color = c;
        }

        if(Mathf.Abs(background.anchorMin.x - bkgTargetAnchorMin) > 0.002f)
        {
            Vector2 min = background.anchorMin;
            min.x = Mathf.Lerp(min.x, bkgTargetAnchorMin, timeMultiplier);
            background.anchorMin = min;
        }

        if (Mathf.Abs(background.anchorMax.x - bkgTargetAnchorMax) > 0.002f)
        {
            Vector2 max = background.anchorMax;
            max.x = Mathf.Lerp(max.x, bkgTargetAnchorMax, timeMultiplier);
            background.anchorMax = max;
        }
    }

    void HideArrowLeftOrPlus()
    {
        bkgTargetAnchorMin = 0.5f;
        bkgTargetAnchorMax = 1.0f;
        arrowLeftTargetAlpha = 0.0f;
        arrowRightTargetAlpha = 1.0f;
    }

    void HideArrowRightOrMinus()
    {
        bkgTargetAnchorMin = 0.0f;
        bkgTargetAnchorMax = 0.5f;
        arrowLeftTargetAlpha = 1.0f;
        arrowRightTargetAlpha = 0.0f;
    }

    void ShowCompleteButton()
    {
        bkgTargetAnchorMin = 0.0f;
        bkgTargetAnchorMax = 1.0f;
        arrowLeftTargetAlpha = 1.0f;
        arrowRightTargetAlpha = 1.0f;
    }
}