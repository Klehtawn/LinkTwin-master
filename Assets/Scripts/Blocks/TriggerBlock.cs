using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[SelectionBase]
[ExecuteInEditMode]
public class TriggerBlock : Block {

    public Block[] targets;

    public Material connectionMaterial;
    public Material connectionDottedMaterial;
    public Material connectionGlowMaterial;
    public Material connectionDotGlowMaterial;

    public float connectionOffsetY = 0.0f;
    public float lineWidth = 2.0f;

    public GameObject lineCap;

	// Use this for initialization
	public override void Start ()
    {
        base.Start();

        OnPlayerInTheMiddle += OnPlayerEnteredFunc;
        OnPlayerLeft += OnPlayerLeftFunc;

        if (GetComponent<Collider>() != null)
            GetComponent<Collider>().isTrigger = true;

        navigable = true;

        StateChanged(BlockState.State_Undefined, state);

        //state = BlockState.State_Off;
	}



    Block movableBox = null;

    int numLinesInGroup = 4;

    public override void Update()
    {
        base.Update();

        if (lines.Count / numLinesInGroup != targets.Length || mustGenerateLines)
        {
            GenerateLines();
            mustGenerateLines = false;
        }

        if(hasMoved == false)
        {
            foreach(Block t in targets)
            {
                if(t.hasMoved)
                {
                    UpdateLines();
                    break;
                }
            }
        }
        else
        {
            UpdateLines();
        }

        Block b = getMovableBox();

        if(b == null && movableBox != null)
        {
            OnPlayerLeftFunc(null);
        }

        if(b != null && movableBox == null)
        {
            OnPlayerEnteredFunc(null);
        }
        
        movableBox = b;

        if (lateTogglesNeeded > 0)
        {
            if(CanChangeState())
            {
                lateTogglesNeeded = lateTogglesNeeded % 2;
                while (lateTogglesNeeded > 0)
                {
                    MyToggleState();
                    lateTogglesNeeded--;
                }
            }
        }
    }
    
    protected override void StateChanged(BlockState prev, BlockState current)
    {
        foreach (Block t in targets)
            if (t != null)
            {
                t.SetState(state);
                t.EnableBehaviour(state == BlockState.State_On);
                if (TheGame.Instance != null)
                    TheGame.Instance.mustRefreshShadows = true;
            }
    }

    int lateTogglesNeeded = 0;

    void OnPlayerEnteredFunc(Player p)
    {
        if (CanChangeState() == false)
        {
            if (blockType == BlockType.PushButton)
                lateTogglesNeeded++;
            return;
        }
        MyToggleState();
    }

    void OnPlayerLeftFunc(Player p)
    {
        if (blockType == BlockType.PushButton)
        {
            if (CanChangeState() == false)
            {
                lateTogglesNeeded++;
                return;
            }
            MyToggleState();
        }
    }

    public override void WriteSignificantInfo(ref System.IO.BinaryWriter bw)
    {
        base.WriteSignificantInfo(ref bw);

        WriteSignificantValue(ref bw, "numTargets", (byte)targets.Length);
        for(int i = 0; i < targets.Length; i++)
        {
            WriteSignificantValue(ref bw, "tgt" + i.ToString(), targets[i].blockIdentifier);
        }

        WriteSignificantValue(ref bw, "state", state == BlockState.State_On ? true : false);
    }

    int[] targetsIds;
    public override void ReadSignificantInfo(ref System.IO.BinaryReader br)
    {
        base.ReadSignificantInfo(ref br);

        byte numTargets = ReadSignificantValueByte(ref br, "numTargets");
        targetsIds = new int[numTargets];
        for (int i = 0; i < numTargets; i++)
        {
            targetsIds[i] = ReadSignificantValueByte(ref br, "tgt" + i.ToString());
        }

        bool b = ReadSignificantValueBool(ref br, "state");
        SetState(b ? BlockState.State_On : BlockState.State_Off, true);
    }

    public override void LinkSignificantValues(Block[] blocks)
    {
        targets = new Block[targetsIds.Length];
        for(int i = 0; i < targets.Length; i++)
            targets[i] = FindBlockByID(blocks, (byte)targetsIds[i]);
    }

    float capRadius = 0.0f;
    void GenerateLineTo(Block target)
    {
        if (target == null) return;

        for (int i = 0; i < numLinesInGroup; i++)
        {
            GameObject obj = new GameObject();
            obj.transform.SetParent(transform);
            obj.name = "Line";

            LineRenderer2 lr = obj.AddComponent<LineRenderer2>();
            lr.material = connectionMaterial;

            lr.vScale = 0.0f;
            lr.vOffset = 0.5f;

            if (i == 1)
            {
                lr.material = connectionGlowMaterial;
                obj.name = "Line Glow";
            }

            if (i == 2)
            {
                lr.material = connectionDottedMaterial;
                obj.name = "Line Dotted";
                lr.vScale = 0.8f;
                lr.vOffset = 0.0f;
            }

            if (i == 3) // little glow moving
            {
                lr.material = connectionDotGlowMaterial;
                obj.name = "Line Dot Glow";
                lr.vScale = 0.15f;
                lr.vOffset = 0.0f;

                lr.gameObject.AddComponent<UVTranslate>();
            }

            lines.Add(lr);

            if (i == 0)
            {
                for (int j = 0; j < 2; j++)
                {
                    GameObject donut = GameObject.Instantiate<GameObject>(lineCap);
                    donut.transform.parent = lr.transform;
                    donut.name = "cap" + (j + 1).ToString();
                    capRadius = donut.GetComponentInChildren<Renderer>().bounds.extents.x;
                }
            }
        }

        UpdateLines();
    }

    void DeleteLines()
    {
        LineRenderer2[] lrs = GetComponentsInChildren<LineRenderer2>();
        foreach(LineRenderer2 lr in lrs)
        {
            if (Application.isPlaying)
                Destroy(lr.gameObject);
            else
                DestroyImmediate(lr.gameObject);
        }

        lines.Clear();

        /*Highlight[] hs = GetComponentsInChildren<Highlight>();
        foreach(Highlight h in hs)
        {
            if (Application.isPlaying)
                Destroy(h.gameObject);
            else
                DestroyImmediate(h.gameObject);
        }

        targetsHighlights.Clear();*/
    }

    List<LineRenderer2> lines = new List<LineRenderer2>();
    //List<GameObject> targetsHighlights = new List<GameObject>();
    void GenerateLines()
    {
        DeleteLines();
        foreach (Block b in targets)
        {
            GenerateLineTo(b);

            /*GameObject g = GameObject.Instantiate<GameObject>(Resources.Load("Highlight") as GameObject);
            g.transform.SetParent(transform);
            g.transform.position = b.transform.position;

            targetsHighlights.Add(g);*/

            Widget.SetLayer(transform.gameObject, transform.gameObject.layer);
        }


    }

    void UpdateLines()
    {
        for (int k = 0; k < lines.Count; k++)
        {
            int lineGroup = k / numLinesInGroup;
            if (targets[lineGroup] == null) continue;

            Vector3[] pos = new Vector3[3];

            pos[0] = transform.position;
            pos[2] = targets[lineGroup].position;

            if (Mathf.Abs(pos[0].x - pos[2].x) < 0.001f || Mathf.Abs(pos[0].z - pos[2].z) < 0.001f)
            {
                pos[1] = (pos[0] + pos[2]) * 0.5f;
            }
            else
                pos[1] = new Vector3(pos[0].x, 0.0f, pos[2].z);

            for (int i = 0; i < pos.Length; i++ )
            {
                pos[i].y = 0.0f;
            }

            Vector3[] d = new Vector3[2];

            d[0] = pos[1] - pos[0];
            if (d[0].sqrMagnitude < 0.001f)
            {
                d[0] = pos[2] - pos[0];
            }

            d[0].Normalize();
            
            pos[0] += d[0] * GameSession.gridUnit * 0.45f;

            d[1] = pos[1] - pos[2];

            if (d[1].sqrMagnitude < 0.001f)
            {
                d[1] = pos[0] - pos[2];
            }

            d[1].Normalize();

            pos[2] += d[1] * capRadius;

            int lineIndex = k % numLinesInGroup;

            lines[k].transform.position = Vector3.up * (connectionOffsetY - (float)(lineIndex));

            lines[k].SetPositions(pos, GameSession.gridUnit * 0.2f);
            float baseW = lineWidth;

            if (lineIndex == 1) // glow
            {
                baseW *= 2.5f;
            }

            if (lineIndex == 2) // dotted
            {
            }

            if (lineIndex == 3) // glow dot
            {
                baseW *= 2.0f;
                lines[k].transform.position += Vector3.up * 4.0f;
                UVTranslate uvt = lines[k].gameObject.GetComponent<UVTranslate>();
                uvt.loopAt = 40.0f;// lines[k].totalLength * 0.4f;
                uvt.speed = new Vector2(0.0f, -10.0f);
            }

            lines[k].SetWidths(baseW, baseW, baseW);

            if (lineIndex == 0)
            {
                for (int j = 0; j < 2; j++)
                {
                    Transform c = lines[k].transform.GetChild(j);
                    Vector3 p = j == 0 ? pos[0] : pos[2];
                    p.y = transform.position.y + 0.5f - 5.0f;
                    c.transform.position = p - d[j] * capRadius;
                }
            }
        }

        /*for (int i = 0; i < targetsHighlights.Count; i++)
        {
            targetsHighlights[i].transform.position = targets[i].transform.position;
        }*/
    }

    Block getMovableBox()
    {
        if (TheGame.Instance == null) return null;

        getRoot();

        Block b = root.blocks.ItemAt(transform.position, this);
        if (b != null && (b as MovableBox) != null)
            return b;
        return null;
    }

    public bool IsLinkedWith(Block b)
    {
        if (targets == null || targets.Length == 0) return false;

        foreach (Block bb in targets)
            if (bb == b) return true;

        return false;
    }

    public void LinkWith(Block b)
    {
        Array.Resize<Block>(ref targets, targets.Length + 1);
        targets[targets.Length - 1] = b;
        b.OnBlockDeleted += BreakLinkWith;

        mustGenerateLines = true;
    }

    bool mustGenerateLines = false;
    public void BreakLinkWith(Block b)
    {
        if (targets == null) return;
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == b)
            {
                if (targets.Length > 1)
                {
                    targets[i] = targets[targets.Length - 1];
                }
                Array.Resize<Block>(ref targets, targets.Length - 1);
                break;
            }
        }

        mustGenerateLines = true;
    }

    public override void Refresh()
    {
        base.Refresh();
        StateChanged(BlockState.State_Undefined, state);
    }

    bool CanChangeState()
    {
        float halfBlock = GameSession.gridUnit * 0.5f;
        float halfBlock2 = halfBlock * halfBlock;
        foreach (Block b in targets)
        {
            if (b.navigable == false && (b.GetPlayerAtLocation() != null || b.GetPlayerAtFutureLocation() != null))
                return false;

            foreach (MovableBox mb in allMovableBoxes)
            {
                if (mb == null) continue;
                Vector3 d = mb.position - b.position; d.y = 0.0f;
                if (d.sqrMagnitude < halfBlock2)
                    return false;

                d = mb.futurePosition - b.position; d.y = 0.0f;
                if (d.sqrMagnitude < halfBlock2)
                    return false;
            }
        }
        return true;
    }

    MovableBox[] allMovableBoxes = null;
    public override void OnGameStarted()
    {
        base.OnGameStarted();
        getRoot();
        allMovableBoxes = root.blocks.GetBlocksOfType<MovableBox>();
    }

    void MyToggleState()
    {
        GameEvents.Send(GameEvents.EventType.BoardButtonPressed);
        ToggleState();
    }
}
