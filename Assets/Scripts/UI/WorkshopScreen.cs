using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class WorkshopScreen : GameScreen {

    WorkshopTableResize tableResize;

    public WorkshopItems workshopItems;
    public Widget tableSelect;

    RawImage tableRender;
    RawImage tableRenderMask;

    public WorkshopItemButton recycleBin, resizeButton;
    public Widget playButton, undoButton, redoButton;

    public Widget undoPlayRedoItems;

    public Widget backButton;

    public Widget resizeButtons;

    public WorkshopGizmos gizmos;

    public Widget devButtons;

    Widget background;

    static UndoSystemWorkshop undoSystem = new UndoSystemWorkshop();

    public static int maxTableCells = 10;
    public static int minTableCells = 2;

	protected override void Start () {

        base.Start();

        GameSession.gameState = GameSession.GameState.Workshop;

        background = transform.Find("Background").GetComponent<Widget>();
        background.OnTouchUp += OnBackgroundTouchUp;

        gameLayer = LayerMask.NameToLayer("Game");

        tableResize = null;// GetComponent<WorkshopTableResize>();
        if(tableResize != null)
            tableResize.OnDragFinished += OnTableResizeFinished;

        OnWindowStartClosing += OnScreenStartClosing;
        OnWindowClosed += OnScreenClosed;

        tableSelect.OnTouchUp += OnTableSelect;
        tableSelect.OnTouchDown += OnTablePress;
        tableSelect.OnTouchMoved += OnTableTouchMove;

        recycleBin.selected = false;
        recycleBin.OnTouchUp += OnRecycleBinPressed;
        recycleBin.OnLongPress += OnRecycleBinLongPressed;

        resizeButton.selected = false;
        resizeButton.OnTouchUp += OnResizeButtonPressed;
        resizeButton.OnItemSelected += OnResizeButtonSelected;
        resizeButtons.active = resizeButton.selected;

        playButton.active = true;
        undoButton.active = false;
        redoButton.active = false;

        playButton.OnTouchUp += OnPlayButtonPressed;
        undoButton.OnTouchUp += OnUndoButtonPressed;
        redoButton.OnTouchUp += OnRedoButtonPressed;

        workshopItems.OnItemSelected += OnWorkshopItemSelected;

        tableRender = transform.Find("TableProjection").GetComponent<RawImage>();
        tableRenderMask = transform.Find("TableProjectionMask").GetComponent<RawImage>();

        InitTable();

        //OnFinishedShowing += OnScreenAppeared;

        undoSystem.ClearRedo();

        Desktop.main.sounds.PauseMusic();
	}

    void OnDestroy()
    {
        GameSession.gameState = GameSession.GameState.Default;
    }

    void OnScreenStartClosing(WindowA w, int retValue)
    {
        GameSession.gameState = GameSession.GameState.Default;

        table.gameObject.SetActive(false);

        Desktop.main.sounds.ResumeMusic();
    }

    void OnScreenClosed(WindowA w, int retValue)
    {
        Destroy(tableCamera.targetTexture);
        undoSystem.SetUndoPoint(table);
        gizmos.Clear();
        GameObject.Destroy(table.gameObject);

    }

    void OnScreenAppeared(Widget w)
    {
        InitTable();
    }

	
	protected override void Update () {

        base.Update();

        if (tableInited == false)
            _InitTable();

        UpdateTableSelect();

        if(centerCameraFix < 0)
        {
            CenterCamera();
            if(tableResize != null)
                tableResize.Refresh();
            centerCameraFix++;
        }

        if (workshopItems.selected)
            recycleBin.selected = false;

        /*if (contextMenuShown == false)
        {
            if (workshopItems.selected)
            {
                tableMask.HighlightBlocks(ref Blocks.ground);
            }
            else
            {
                tableMask.ClearHighlights();
            }
        }*/

        tableRenderMask.gameObject.SetActive(tableMask.GetMaskAlpha() > 0.05f);
        tableRenderMask.color = new Color(1.0f, 1.0f, 1.0f, tableMask.GetMaskAlpha());

        undoButton.active = undoSystem.canUndo && placementMaskActive == false;
        redoButton.active = undoSystem.canRedo && placementMaskActive == false;

        CheckBrokenLinks();

        if(tableCamera != null && setTableOrthographicSize > 0.0f)
        {
            if (Mathf.Abs(setTableOrthographicSize - tableCamera.orthographicSize) < 0.05f)
            {
                tableCamera.orthographicSize = setTableOrthographicSize;
            }
            else
            {
                tableCamera.orthographicSize = Mathf.Lerp(tableCamera.orthographicSize, setTableOrthographicSize, 7.0f * Time.deltaTime);
            }
        }


	}

    bool contextMenuShown = false;
    Block contextMenuRoot = null;

    Transform table = null;

    [HideInInspector]
    [NonSerialized]
    public LevelRoot tableRoot = null;

    TableMask tableMask = null;

    [HideInInspector]
    [NonSerialized]
    public Camera tableCamera = null;

    private float setTableOrthographicSize = 1.0f;

    Transform tableGrid = null;

    int gameLayer = -1;

    [NonSerialized]
    public byte[] levelSourceBytes = null;

    bool tableInited = true;
    void InitTable()
    {
        tableInited = false;
    }

    void _InitTable()
    {
        tableInited = true;

        CreateTableRoot();
        CreateGround();
        CreateTableCamera();

        Widget.SetLayer(table.gameObject, gameLayer);

        CreateTableMask();

        if(tableResize != null)
            tableResize.RepositionResizeButtons();

        RenderSettings.ambientLight = Color.white;
        RenderSettings.ambientIntensity = 1.0f;

        undoSystem.LoadLast(table);
        RefreshTable();
    }

    public void AppendTableHelpers()
    {    
        CreateTableCamera();

        Widget.SetLayer(table.gameObject, gameLayer);

        CreateTableMask();

        if (tableResize != null)
            tableResize.RepositionResizeButtons();

        RenderSettings.ambientLight = Color.white;
        RenderSettings.ambientIntensity = 1.0f;

        tableRoot.blocks.Fill(table);

        for (int i = 0; i < tableRoot.blocks.items.Count; i++)
        {
            OnBlockInserted(tableRoot.blocks.items[i]);
        }

        centerCameraFix = -5;

        undoSystem.Clear();

        GetMovingPlatforms();
    }

    public void ResetTable()
    {
        undoSystem.Clear();
        _InitTable();
        centerCameraFix = -5;
        GetMovingPlatforms();
    }

    void CreateTableMask()
    {
        GameObject obj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("UI/TableMask"));
        obj.transform.SetParent(table);
        obj.name = "Table Highlights";

        tableMask = obj.GetComponent<TableMask>();
        tableMask.gameCamera = tableCamera;

        tableRenderMask.texture = tableMask.GetMaskTexture();
    }

    void CreateTableRoot()
    {
        if (table == null)
        {
            GameObject obj = new GameObject();
            obj.name = "Table";
            table = obj.transform;
            table.localScale = Vector3.one;
            table.localPosition = Vector3.zero;

            tableRoot = obj.AddComponent<LevelRoot>();
        }
        else
        {
            DeleteTable();
        }

        tableRoot.CreateStructure();
    }

    void DeleteTable()
    {
        tableRoot.DestroyStructure();

        if (tableCamera != null)
        {
            Destroy(tableCamera.targetTexture);
            Destroy(tableCamera.gameObject);
        }

        if(tableMask != null)
            Destroy(tableMask.gameObject);
        tableCamera = null;
        tableMask = null;
    }

    int centerCameraFix = -5;
    void CreateGround()
    {
        int cols = 3;
        int rows = 3;
        tableRoot.Ground.CreateBlocks(cols, rows, GridCreator.offset(cols, rows));

        tableRoot.blocks.Fill(table);

        centerCameraFix = -5;
    }

    void CreateTableCamera()
    {
        GameObject camObj = new GameObject();
        camObj.transform.SetParent(table);
        camObj.name = "Table Camera";
        camObj.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

        Camera.main.cullingMask &= ~(1 << gameLayer);
        Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("TableMask"));

        tableCamera = camObj.AddComponent<Camera>();
        tableCamera.orthographic = true;
        tableCamera.cullingMask = 1 << gameLayer;
        tableCamera.clearFlags = CameraClearFlags.Nothing;
        tableCamera.nearClipPlane = -1000.0f;
        tableCamera.farClipPlane = 1000.0f;

        setTableOrthographicSize = 0.0f;
        CenterCamera();

        if (tableResize != null)
            tableResize.tableCamera = tableCamera;
        gizmos.tableCamera = tableCamera;

        //if(true)
        //{
            RenderTexture rt = new RenderTexture(Desktop.main.width, Desktop.main.height, 16, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Bilinear;
            rt.generateMips = false;
            rt.Create();

            tableCamera.targetTexture = rt;
            tableCamera.clearFlags = CameraClearFlags.Color;
            tableCamera.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);

            tableRender.texture = rt;
        //}
        //else
        //{
        //}

        GameObject bkg = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Blocks/WorkshopBackground"));
        bkg.transform.SetParent(table);
        bkg.GetComponent<FitToScreen>().renderCamera = tableCamera;
    }

    public void CenterCamera(bool reposition = false)
    {
        if (tableCamera == null) return;

        tableRoot.blocks.Fill(table.gameObject);

        Vector3 gc = tableRoot.blocks.GetGroundCenter();
        gc.y = 0.0f;
        tableCamera.transform.position = gc + Vector3.up * 236.0f;
        
        //if(resizeButton.selected == false)
        //    tableCamera.transform.position += Vector3.back * 2.5f;

        Vector3 size = tableRoot.blocks.GetGroundSize() / GameSession.gridUnit;

        float aspect = Desktop.main.aspect;

        float tableWidth = size.x;
        //if(tableWidth < (float)maxTableCells)
            tableWidth += 2.0f;
        float tableHeight = size.z;
        //if (tableHeight < (float)maxTableCells)
            tableHeight += 2.0f;

        tableWidth = Mathf.Max(tableWidth, tableHeight);

        if (setTableOrthographicSize == 0.0f)
        {
            setTableOrthographicSize = tableWidth * 0.5f * GameSession.gridUnit / aspect;
            tableCamera.orthographicSize = setTableOrthographicSize;
        }
        else
        {
            setTableOrthographicSize = tableWidth * 0.5f * GameSession.gridUnit / aspect;
        }

        if (aspect >= 2.8f / 4.0f)
            setTableOrthographicSize *= 1.35f;
        

        UpdateGrid();
    }

    void OnTableResizeFinished()
    {
        CenterCamera();
    }

    void AttachGrid()
    {
        if (tableGrid != null) return;

        GameObject grid = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("UI/WorkshopGrid"));
        grid.transform.SetParent(table);
        grid.transform.localPosition = Vector3.down * GameSession.gridUnit;// *2.0f;

        Widget.SetLayer(grid, gameLayer);

        tableGrid = grid.transform;
    }

    void UpdateGrid()
    {
        if (tableRoot == null) return;

        AttachGrid();

        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        tableRoot.blocks.GetGroundMinMax(ref min, ref max);

        min -= Vector3.one * GameSession.gridUnit * 1.5f;
        max += Vector3.one * GameSession.gridUnit * 1.5f;

        Vector3 sz = max - min;
        Vector3 center = (max + min) * 0.5f;
        center.y = tableGrid.localPosition.y;

        tableGrid.localScale = new Vector3(sz.x, 1.0f, sz.z);
        tableGrid.localPosition = center;

        Renderer r = tableGrid.GetComponentInChildren<Renderer>();
        if(r != null)
        {
            r.material.SetTextureScale("_MainTex", new Vector2(sz.x / GameSession.gridUnit, sz.z / GameSession.gridUnit) * 1.0f);
        }
    }

    void UpdateTableSelect()
    {
        if (tableRoot == null) return;

        Vector3 min = tableRoot.blocks.GetGroundMin() - Vector3.one * GameSession.gridUnit * 1.5f;
        Vector3 max = tableRoot.blocks.GetGroundMax() + Vector3.one * GameSession.gridUnit * 1.5f;

        Vector3 p = tableCamera.WorldToViewportPoint(min);
        min = Desktop.main.ViewportToDesktop(p);

        p = tableCamera.WorldToViewportPoint(max);
        max = Desktop.main.ViewportToDesktop(p);

        tableSelect.pos = min;
        tableSelect.size = max - min;
    }

    Block getItemOnTable(Vector2 pos)
    {
        return tableRoot.blocks.ItemAt(getPosOnTable(pos));
    }

    Vector3 getPosOnTable(Vector2 pos)
    {
        Vector3 p = Desktop.main.DesktopToViewport(tableSelect.pos + pos);

        p = tableCamera.ViewportToWorldPoint(p);

        p.y = tableRoot.blocks.ground[0].transform.position.y;

        return p;
    }

    Block tableSelectedBlock = null;
    Vector3 tableSelectedBlockInitialPosition = Vector3.zero;
    bool thereWasAMove = false;

    void OnTablePress(MonoBehaviour sender, Vector2 pos)
    {
        tableSelectedBlock = getItemOnTable(pos);
        if(tableSelectedBlock != null)
            tableSelectedBlockInitialPosition = tableSelectedBlock.position;
        thereWasAMove = false;
    }

    void OnTableTouchMove(MonoBehaviour sender, Vector2 start, Vector2 current)
    {
        //if (recycleBin.selected) return;
        //if (workshopItems.selected != null) return;

        if (tableSelectedBlock == null) return;
        if (contextMenuShown) return;
        if (Vector2.Distance(start, current) < 6.0f) return;

        Vector3 p = getPosOnTable(current);
        if (tableSelectedBlock as GroundBlock == null)
        {
            if (thereWasAMove == false)
                undoSystem.SetUndoPoint(table);

            tableSelectedBlock.transform.position = SnapToGrid.Snap(p);
            if (tableSelectedBlock as MovingPlatform2 != null)
            {
                MovingPlatform2 mp2 = tableSelectedBlock as MovingPlatform2;
                mp2.Reset(true);
            }
        }

        thereWasAMove = true;
    }

    List<Block> blocksToHighlight = new List<Block>();

    void OnTableSelect(MonoBehaviour sender, Vector2 pos)
    {
        if (thereWasAMove)
        {
            // check validity of placement
            if(tableSelectedBlock != null)
            {
                Block underneath = tableRoot.blocks.ItemAt(getPosOnTable(pos), tableSelectedBlock);
                if(underneath == null || underneath.blockType != Block.BlockType.Ground)
                {
                    // placement not valid, move it back to initial position
                    ShowFailedInsert(tableSelectedBlock.position);
                    
                    tableSelectedBlock.position = tableSelectedBlockInitialPosition;
                }
            }
            
            return;
        }

        if (resizeButtons.active) return;

        Block b = tableSelectedBlock;
        Vector3 p = getPosOnTable(pos);

        Block blockToInsert = null;
        if(workshopItems.selected != null)
            blockToInsert = workshopItems.selected.source.GetComponent<Block>();

        if (b == null)
        {
            if (placementMaskActive == false)
            {
                if (contextMenuShown)
                {
                    ClearContextMenu();
                    return;
                }

                if (recycleBin.selected == false)
                {
                    if (blockToInsert != null && blockToInsert.blockType == Block.BlockType.Wall) // insert wall
                    {
                        Block wall = InsertObject(workshopItems.selected.source, tableRoot.Table.transform, p);
                        if (wall.GetNeighbours().Length == 0)
                        {
                            DeleteBlock(wall);
                            tableRoot.RefreshStructure();

                            ShowFailedInsert(p);
                        }
                    }
                    else
                    {
                        InsertObject(Resources.Load("GroundBlock") as GameObject, tableRoot.Ground.transform, SnapToGrid.Snap(p));

                        CenterCamera();

                        if (tableResize != null)
                            tableResize.Refresh();
                    }
                }
            }

            if(placementMaskActive)
            {
                workshopItems.ClearSelection();
            }

            return;
        }

        if (recycleBin.selected)
        {
            DeleteBlock(b);

            CenterCamera();
            if (tableResize != null)
                tableResize.Refresh();

            return;
        }

        if (b != null && contextMenuShown)
        {
            if (blocksToHighlight.Contains(b) == false)
            {
                ClearContextMenu();
                return;
            }
            else
            {
                HandleContextMenu(b);
                return;
            }
        }

        if(b.blockType == Block.BlockType.Button)
        {
            b.ToggleState();
        }
        else
        if (b.blockType == Block.BlockType.Door)
        {
            Vector3 angles = b.transform.rotation.eulerAngles;
            angles.y += 90.0f;
            b.transform.rotation = Quaternion.Euler(angles);
        }
        else
        if(b as GroundBlock == null) // there is an item, check if it has context menu
        {
            if (ShowContextMenu(b))
                return;
        }

        if(blockToInsert != null && ValidPlacement(b.position))
        {
            if (b.GetType() != blockToInsert.GetType() && blockToInsert.blockType != Block.BlockType.Wall)
            {
                Vector3 bp = b.transform.position;
                if ((b as GroundBlock) == null)
                {
                    //DeleteBlock(b);
                    return;
                }

                Block insertedBlock = InsertObject(workshopItems.selected.source, tableRoot.Table.transform, bp);

                if(insertedBlock != null && insertedBlock.blockType == Block.BlockType.Button)
                {
                    if(ShowContextMenu(insertedBlock) == false)
                    {
                        DesktopUtils.ShowLocalizedMessageBox("NOTHING_TO_CONNECT_TO");
                        DeleteBlock(insertedBlock, false);
                    }
                }

                if(insertedBlock != null && insertedBlock.blockType == Block.BlockType.Piston)
                {
                    ShowContextMenu(insertedBlock);
                }
            }
        }

        if(ValidPlacement(b.position) == false)
        {
            workshopItems.ClearSelection();
        }
    }

    Block InsertObject(GameObject src, Transform par, Vector3 pos)
    {
        undoSystem.SetUndoPoint(table);

        GameObject obj = GameObject.Instantiate<GameObject>(src);
        obj.transform.SetParent(par);
        obj.name = src.name;
        obj.transform.position = pos;

        Widget.SetLayer(obj, gameLayer);

        tableRoot.blocks.Fill(table);

        Block b = obj.GetComponent<Block>();
        OnBlockInserted(b);

        RefreshTableItems();

        return b;
    }

    void DeleteBlock(Block b, bool silent = true)
    {
        if (b == null) return;

        if (silent)
            undoSystem.SetUndoPoint(table);
        else
            ShowFailedInsert(b.position);

        b.transform.parent = null;
        OnBlockDeleted(b);
        GameObject.Destroy(b.gameObject);

        tableRoot.RefreshStructure();
        RefreshTableItems();

        if (b as MovingPlatform2 != null)
        {
            GetMovingPlatforms();
        }
    }

    void OnRecycleBinPressed(MonoBehaviour sender, Vector2 pos)
    {
        if (contextMenuShown) return;

        recycleBin.selected = !recycleBin.selected;
        if (recycleBin.selected && workshopItems.selected != null)
        {
            workshopItems.ClearSelection();
        }
    }

    void OnRecycleBinLongPressed(MonoBehaviour sender, Vector2 pos)
    {
        if (contextMenuShown) return;

        undoSystem.SetUndoPoint(table);

        foreach(Block b in tableRoot.blocks.items)
        {
            b.transform.parent = null;
            GameObject.Destroy(b.gameObject);
        }

        tableRoot.RefreshStructure();
        GetMovingPlatforms();
    }

    bool ShowContextMenu(Block b)
    {
        blocksToHighlight.Clear();
        if (b as TriggerBlock != null)
        {
            tableRoot.blocks.GetBlocksOfType(Block.BlockType.Door, ref blocksToHighlight);
            tableRoot.blocks.GetBlocksOfType(Block.BlockType.Trap, ref blocksToHighlight);
            //tableRoot.blocks.GetBlocksOfType(Block.BlockType.Crumbling, ref blocksToHighlight);
            tableRoot.blocks.GetBlocksOfType(Block.BlockType.Finish, ref blocksToHighlight);
            tableRoot.blocks.GetBlocksOfType(Block.BlockType.Default, ref blocksToHighlight);
        }
        else
        if (b as MovingPlatform2 != null)
        {
            HighlightNeighbours(b, Vector3.forward);
            HighlightNeighbours(b, Vector3.back);
            HighlightNeighbours(b, Vector3.left);
            HighlightNeighbours(b, Vector3.right);

            if (blocksToHighlight.Count > 0)
            {
                b.EnableBehaviour(true);
            }
        }
/*        else
        if (b as Portal != null)
        {
            tableRoot.blocks.GetBlocksOfType(Block.BlockType.Portal, ref blocksToHighlight);
            blocksToHighlight.Remove(b);
        }*/
        /*else
        if(b as Door != null)
        {
            HighlightNeighbours(b, Vector3.forward, 1, true);
            HighlightNeighbours(b, Vector3.back, 1, true);
            HighlightNeighbours(b, Vector3.left, 1, true);
            HighlightNeighbours(b, Vector3.right, 1, true);

            if (blocksToHighlight.Count > 0)
            {
                AutomaticStateChanger asc = b.gameObject.AddComponent<AutomaticStateChanger>();
                asc.onDuration = asc.offDuration = 1.0f;
            }
        }*/

        contextMenuShown = blocksToHighlight.Count > 0;

        if (contextMenuShown)
        {
            tableMask.HighlightBlocks(ref blocksToHighlight);
            ShowControls(false);
        }

        contextMenuRoot = b;

        return contextMenuShown;
    }

    void HighlightNeighbours(Block b, Vector3 dir, int maxItems = 100000, bool ignoreOtherBlocks = false)
    {
        Vector3 p = b.transform.position + dir * GameSession.gridUnit;
        Block neighbour = tableRoot.blocks.ItemAt(p);

        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        tableRoot.blocks.GetGroundMinMax(ref min, ref max);

        int numItems = 0;
        while (true)
        {
            if(neighbour != null)
            {
                if (IsInMovingPlatformsRange(neighbour, b as MovingPlatform2))
                    break;
            
                 if((neighbour as GroundBlock) == null && !ignoreOtherBlocks)
                    break;

                blocksToHighlight.Add(neighbour);
                numItems++;
                if (numItems == maxItems)
                    break;
            }
            p += dir * GameSession.gridUnit;
            neighbour = tableRoot.blocks.ItemAt(p);

            if (p.x < min.x) break;
            if (p.z < min.z) break;

            if (p.x > max.x) break;
            if (p.z > max.z) break;
        }
    }

    void ClearContextMenu()
    {
        if(contextMenuShown)
        {
            ShowControls(true);

            if(contextMenuRoot.blockType == Block.BlockType.Door)
            {
                AutomaticStateChanger asc = contextMenuRoot.gameObject.GetComponent<AutomaticStateChanger>();
                if (asc)
                    Destroy(asc);
                contextMenuRoot.SetState(Block.BlockState.State_On);
            }

            if(contextMenuRoot.blockType == Block.BlockType.Button)
            {
                TriggerBlock tb = contextMenuRoot as TriggerBlock;
                if (tb.targets == null || tb.targets.Length == 0)
                    DeleteBlock(tb, false);
            }

            contextMenuRoot.EnableBehaviour(false);

            tableMask.ClearHighlights();
            contextMenuShown = false;
            contextMenuRoot = null;

            workshopItems.ClearSelection();
        }
    }

    void HandleContextMenu(Block b)
    {
        undoSystem.SetUndoPoint(table);

        if(contextMenuRoot as TriggerBlock != null)
        {
            RefreshButtonsLinks(b);

            TriggerBlock tb = contextMenuRoot as TriggerBlock;
            if(tb.IsLinkedWith(b))
            {
                //tb.BreakLinkWith(b);
            }
            else
            {
                tb.LinkWith(b);
            }

            gizmos.ShowExclamation(tb, tb.targets == null || tb.targets.Length == 0);

            tb.Refresh();

            if(tableMask.Count == tb.targets.Length)
            {
                ClearContextMenu();
            }
        }
        else
        if(contextMenuRoot as MovingPlatform2 != null)
        {
            MovingPlatform2 mp2 = contextMenuRoot.GetComponent<MovingPlatform2>();
            Vector3 d = b.position - mp2.origin; d.y = 0.0f;
            mp2.range = d.magnitude / GameSession.gridUnit;
            d.Normalize();
            mp2.transform.localRotation = Quaternion.LookRotation(d, Vector3.up);
            mp2.Reset();

            gizmos.ShowExclamation(mp2, mp2.range == 0.0f);

            ShowMovingPlatformPath(mp2);
        }
        else
        if (contextMenuRoot as Door != null)
        {
            Vector3 d = b.position - contextMenuRoot.position; d.y = 0.0f;
            d.Normalize();
            contextMenuRoot.transform.rotation = Quaternion.LookRotation(d, Vector3.up) * Quaternion.Euler(0.0f, -90.0f, 0.0f);
        }
        else
        if(contextMenuRoot as Portal != null)
        {
            Portal start = contextMenuRoot as Portal;
            Portal end = b as Portal;

            if (start.target == end)
            {
                start.target = null;
                end.target = null;
                start.AssignColors();
                end.AssignColors();
            }
            else
            {
                if (start.target != null)
                {
                    start.target.target = null;
                    gizmos.ShowExclamation(start.target);
                }

                if (end.target != null)
                {
                    end.target.target = null;
                    gizmos.ShowExclamation(end.target);
                }

                start.target = end;
                end.target = start;
                start.AssignColors();
            }

            ClearContextMenu();

            gizmos.ShowExclamation(start, start.target == null);
            gizmos.ShowExclamation(end, end.target == null);
        }
    }

    void OnBackgroundTouchUp(MonoBehaviour sender, Vector2 p)
    {
        //tableMask.ClearHighlights();
        if (contextMenuShown)
        {
            ClearContextMenu();
            return;
        }

        if(placementMaskActive)
        {
            workshopItems.ClearSelection();
        }
    }

    List<MovingPlatform2> movingPlatformns = new List<MovingPlatform2>();
    void GetMovingPlatforms()
    {
        movingPlatformns.Clear();
        foreach (Block b in tableRoot.blocks.items)
        {
            MovingPlatform2 mp = b as MovingPlatform2;
            if (mp != null)
                movingPlatformns.Add(mp);
        }
    }

    bool IsInMovingPlatformsRange(Block b, MovingPlatform2 excepts = null)
    {
        foreach(MovingPlatform2 mp2 in movingPlatformns)
        {
            if (mp2 == excepts) continue;
            if (mp2.BlockInRange(b))
                return true;
        }

        return false;
    }

    void OnBlockInserted(Block b)
    {
        b.EnableBehaviour(false);

        if(b as MovingPlatform2 != null)
        {
            if((b as MovingPlatform2).range < 0.1f)
            {
                gizmos.ShowExclamation(b);
            }

            GetMovingPlatforms();
        }

        if(b as Portal != null)
        {
            Portal p = b as Portal;
            gizmos.ShowExclamation(b, p.target == null);
            Portal[] portals = tableRoot.blocks.GetBlocksOfType<Portal>();
            if(portals.Length % 2 == 0)
            {
                for(int i = 0; i < portals.Length; i += 2)
                {
                    Portal p1 = portals[i];
                    Portal p2 = portals[i + 1];
                    if (p1.target != null && p2.target != null) continue;
                    p1.target = p2;
                    p2.target = p1;
                    p1.AssignColors();
                    gizmos.RemoveExclamation(p1);
                    gizmos.RemoveExclamation(p2);
                }

                workshopItems.ClearSelection();
            }
        }

        if(b as TriggerBlock != null)
        {
            TriggerBlock tb = b as TriggerBlock;
            gizmos.ShowExclamation(b, tb.targets == null || tb.targets.Length == 0);
        }

        if((b as Spawner != null) || (b as FinishingPoint != null))
        {
            CheckSpawnersFinishingPointsPairs();
        }
    }

    void OnBlockDeleted(Block b)
    {
        // break references
        if (b.blockType == Block.BlockType.Portal)
        {
            // delete pair
            Portal other = (b as Portal).target;
            if (other != null)
            {
                other.target = null;
                ShowFailedInsert(other.position);
                DeleteBlock(other, false);
            }
        }
        else
        if (b.blockType == Block.BlockType.Button)
        {
            foreach(Block t in (b as TriggerBlock).targets)
            {
                t.SetState(Block.BlockState.State_On);
            }
        }
        else
        {
            RefreshButtonsLinks(b);
        }

        if ((b as Spawner != null) || (b as FinishingPoint != null))
        {
            CheckSpawnersFinishingPointsPairs();
        }
    }

    //bool tableCompleted = false;
    int numSpawners = 0;
    int numFinishingPoints = 0;
    void CheckSpawnersFinishingPointsPairs()
    {
        tableRoot.blocks.Fill(table);

        List<Block> spawners = new List<Block>();
        List<Block> finishingPoints = new List<Block>();
        tableRoot.blocks.GetBlocksOfType<Spawner>(ref spawners);
        tableRoot.blocks.GetBlocksOfType<FinishingPoint>(ref finishingPoints);

        int minc = Mathf.Min(spawners.Count, finishingPoints.Count);
        int maxc = Mathf.Max(spawners.Count, finishingPoints.Count);
        for (int i = 0; i < maxc; i++)
        {
            if (i < minc)
            {
                gizmos.RemoveExclamation(spawners[i]);
                gizmos.RemoveExclamation(finishingPoints[i]);
            }
            else
            {
                if (i < spawners.Count)
                    gizmos.ShowExclamation(spawners[i]);
                if (i < finishingPoints.Count)
                    gizmos.ShowExclamation(finishingPoints[i]);
            }
        }

//        tableCompleted = spawners.Count == finishingPoints.Count && spawners.Count > 0;
        numSpawners = spawners.Count;
        numFinishingPoints = finishingPoints.Count;
    }

    void CheckBrokenLinks()
    {
        foreach (Block b in tableRoot.blocks.items)
        {
            TriggerBlock tb = b as TriggerBlock;
            if (tb != null)
            {
                if (tb.targets == null || tb.targets.Length == 0)
                {
                    if(gizmos.HasGizmo(b, WorkshopGizmo.GizmoType.Exclamation) == false)
                        gizmos.ShowExclamation(b);
                }
                else
                {
                    gizmos.RemoveExclamation(b);
                }
            }
        }
    }

    public Transform GetGameTable()
    {
        return table;
    }

    void OnPlayButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        if(numSpawners == 0)
        {
            DesktopUtils.ShowLocalizedMessageBox("WORKSHOP_NO_SPAWN");
            return;
        }
        else
        if(numSpawners < numFinishingPoints)
        {
            DesktopUtils.ShowLocalizedMessageBox("WORKSHOP_NOT_ENOUGH_SPAWN");
            return;
        }
        else
        if (numSpawners > numFinishingPoints)
        {
            DesktopUtils.ShowLocalizedMessageBox("WORKSHOP_NOT_ENOUGH_FINISHES");
            return;
        }
        Close();
        undoSystem.SetUndoPoint(table);
        GameSession.customLevelBytes = undoSystem.Peek();
        GameSession.customLevelId = "TEST";
        WindowA.Create("UI/WorkshopPlayScreen").Show();
    }

    void OnUndoButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        undoSystem.Undo(table);

        RefreshTable();

        CenterCamera();
    }

    void OnRedoButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        undoSystem.Redo(table);

        RefreshTable();

        CenterCamera();
    }

    void RefreshTable()
    {
        tableRoot.RefreshStructure();

        Widget.SetLayer(tableRoot.Ground.transform.gameObject, gameLayer);
        Widget.SetLayer(tableRoot.Table.transform.gameObject, gameLayer);
        for (int i = 0; i < tableRoot.blocks.items.Count; i++)
        {
            Block b = tableRoot.blocks.items[i];

            gizmos.RemoveGizmos(b);

            MovingPlatform2 mp = b as MovingPlatform2;
            if (mp != null)
                ShowMovingPlatformPath(mp);

            OnBlockInserted(b);
        }

        if (tableResize != null)
            tableResize.RepositionResizeButtons();
        CheckSpawnersFinishingPointsPairs();
    }

    void ShowMovingPlatformPath(MovingPlatform2 mp)
    {
        gizmos.RemoveGizmo(mp, WorkshopGizmo.GizmoType.GreenDot);

        float r = 1.0f;
        while(r <= mp.range)
        {
            WorkshopGizmo g = gizmos.ShowGizmo(mp, WorkshopGizmo.GizmoType.GreenDot);
            g.offset = mp.transform.rotation * Vector3.forward * r;
            r += 1.0f;
        }
    }

    public enum LevelSource
    {
        Default,
        FromPlaying
    };

    public LevelSource levelSource = LevelSource.Default;

    protected override void HandleCreateArgs(string args)
    {
        base.HandleCreateArgs(args);

        if(args != null)
        {
            string cmd = "editplayed";
            if(args.IndexOf(cmd) == 0)
            {
                args = args.Remove(0, cmd.Length + 1);
                LoadLevelFromPath(args);
                playButton.gameObject.SetActive(false);
                levelSource = LevelSource.FromPlaying;
            }
        }
        else if (levelSourceBytes != null)
        {
            LoadLevelFromBytes(levelSourceBytes);
            levelSource = LevelSource.Default;
            levelSourceBytes = null;
        }
    }

    string _loadedLevelPath;
    public string LoadedLevelPath
    {
        get
        {
            return _loadedLevelPath;
        }
    }

    public void LoadLevelFromBytes(byte[] bytes)
    {
        if (tableInited == false)
            _InitTable();

        TableDescription td = null;
        Widget.DeleteAllChildren(table);

        LevelRoot root = table.gameObject.GetComponent<LevelRoot>();
        root.CreateStructure();

        TableLoadSave.LoadFromMemory(bytes, ref td, root);

        Transform items = TableLoadSave.ConvertMapDescriptionToScene(td, root);
        items.SetParent(table);

        AppendTableHelpers();

        _loadedLevelPath = null;
    }

    public void LoadLevelFromPath(string path)
    {
#if UNITY_EDITOR

        if (tableInited == false)
            _InitTable();

        TableDescription td = null;
        Widget.DeleteAllChildren(table);

        LevelRoot root = table.gameObject.GetComponent<LevelRoot>();
        root.CreateStructure();

        if (path.IndexOf(".bytes") > 0)
            path = path.Remove(path.IndexOf(".bytes"));
        if (GameSession.IsUserLevel())
            TableLoadSave.LoadFromMemory(GameSession.customLevelBytes, ref td, root);
        else
            TableLoadSave.LoadAbsolutePath(path, ref td, root);

        Transform items = TableLoadSave.ConvertMapDescriptionToScene(td, root);
        items.SetParent(table);

        AppendTableHelpers();

        _loadedLevelPath = path;
#endif
    }

    void OnResizeButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        resizeButton.selected = !resizeButton.selected;
    }

    void OnResizeButtonSelected(MonoBehaviour sender)
    {
        resizeButtons.active = resizeButton.selected;

        ShowControls(!resizeButtons.active, false);
    }


    public float GetGridScreenSize()
    {
        Vector3 p = tableCamera.WorldToViewportPoint(Vector3.right * GameSession.gridUnit) - tableCamera.WorldToViewportPoint(Vector3.zero);
        return p.x * Desktop.main.width;
    }

    void ShowControls(bool show = true, bool includeResizeButton = true)
    {
        workshopItems.active = show;
        playButton.active = show;
        backButton.active = show;
        undoPlayRedoItems.active = show;
        if (devButtons.visible)
            devButtons.active = show;

        if(includeResizeButton)
            resizeButton.active = show;
    }

    bool placementMaskActive = false;

    void OnWorkshopItemSelected(WorkshopItemButton sender)
    {
        HidePlacementMask();

        if(sender != null && sender.selected == false)
        {
            Block insertedBlock = sender.source.GetComponent<Block>();
            if(insertedBlock.blockType == Block.BlockType.Portal)
            {
                Portal[] portals = tableRoot.blocks.GetBlocksOfType<Portal>();
                if(portals.Length % 2 == 1) // odd number, delete last
                {
                    Portal last = portals[portals.Length - 1];
                    DeleteBlock(last, false);
                }
            }
        }

        if (sender != null && sender.selected)
            ShowPlacementMask(sender.source.GetComponent<Block>());
    }

    void ShowFailedInsert(Vector3 pos)
    {
        WorkshopGizmo g = gizmos.ShowBigX(SnapToGrid.Snap(pos), true, 2.0f);
        g.transform.localScale = Vector3.one * GetGridScreenSize() / 80.0f;
    }

    void RefreshTableItems()
    {
        foreach (Block item in tableRoot.blocks.items)
        {
            item.Refresh();
        }
    }

    void ShowPlacementMask(Block b)
    {
        placementMaskActive = false;

        List<Block> list = null;

        if (b.blockType == Block.BlockType.Portal)
        {
            list = new List<Block>(tableRoot.blocks.ground);

            /*for (int i = 0; i < list.Count; i++)
            {
                Block[] n = list[i].GetNeighbours();
                if (n.Length != 4) // is on the edge
                {
                    list.RemoveAt(i);
                    i--;
                }
            }*/

            foreach (Block bb in tableRoot.blocks.items)
            {
                Block g = tableRoot.blocks.GroundAt(bb.position);
                if (g != null)
                    list.Remove(g);

                if(bb.navigable == false)
                {
                    Block n = bb.GetGroundNeighbour(Vector3.left);
                    if (n != null) list.Remove(n);

                    n = bb.GetGroundNeighbour(Vector3.right);
                    if (n != null) list.Remove(n);

                    n = bb.GetGroundNeighbour(Vector3.forward);
                    if (n != null) list.Remove(n);

                    n = bb.GetGroundNeighbour(Vector3.back);
                    if (n != null) list.Remove(n);
                }
            }
        }
        else
        {
            /*list = new List<Block>(tableRoot.blocks.ground);
            foreach (Block bb in tableRoot.blocks.items)
            {
                Block g = tableRoot.blocks.GroundAt(bb.position);
                if (g != null)
                    list.Remove(g);
            }

            if(b.blockType == Block.BlockType.Piston)
            {
            }*/
        }

        if (list != null)
        {
            tableMask.HighlightBlocks(ref list);
            placementMaskActive = list.Count > 0;
        }

        if (list != null && placementMaskActive == false)
        {
            DesktopUtils.ShowLocalizedMessageBox("WORKSHOP_NO_PLACE");
            workshopItems.ClearSelection();
        }

        if(placementMaskActive)
        {
            playButton.active = false;
            backButton.active = false;
            resizeButton.active = false;
            devButtons.active = false;
            recycleBin.active = false;
        }
    }

    void HidePlacementMask()
    {
        tableMask.ClearHighlights();
        placementMaskActive = false;

        playButton.active = true;
        backButton.active = true;
        resizeButton.active = true;
        devButtons.active = true;
        recycleBin.active = true;
    }

    bool ValidPlacement(Vector3 pos)
    {
        if (placementMaskActive == false) return true;
        return tableMask.Contains(pos);
    }

    void RefreshButtonsLinks(Block b)
    {
        TriggerBlock[] allButtons = tableRoot.blocks.GetBlocksOfType<TriggerBlock>();
        foreach (TriggerBlock tb in allButtons)
        {
            if (b != null)
            {
                if (tb.IsLinkedWith(b))
                {
                    tb.BreakLinkWith(b);
                    if (tb.targets.Length == 0)
                    {
                        DeleteBlock(tb, false);
                    }
                }
            }
            else
            {
                if (tb.targets.Length == 0)
                {
                    DeleteBlock(tb, false);
                }
            }
        }
    }
}
