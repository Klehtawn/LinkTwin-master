using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[SelectionBase]
public class TutorialBlock : Block {

    public Vector2 areaSize = Vector2.one;

    public Block[] blocksToHighlight;
    private int[] blocksToHighlightIDs;

    public string textToShow;
    public NarrationBlock narration;

    public bool alwaysOn = false;

    public Transform indicator;

	public override void Start ()
    {
        base.Start();
        OnPlayerHit += APlayerEntered;
        OnPlayerLeft += APlayerLeft;

#if !UNITY_EDITOR
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        if (mr != null)
            mr.enabled = false;
#endif

        UpdateArea();
	}
	
	// Update is called once per frame
	public override void Update () {
        base.Update();

        //UpdateArea();

        if(mustShowTutorial && TheGame.Instance != null && TheGame.Instance.introFinished)
        {
            mustShowTutorial = false;

            if(textToShow != null && textToShow.Length > 0 )
            {
                if(textToShow.StartsWith("cmd:"))
                {
                    string s = textToShow.Remove(0, 4);
                    s = s.TrimStart(' ');
                    if(s == "ghosts")
                    {
                        TheGame.Instance.mustPlayGhostSimulation = true;
                    }
                }
                else
                if (Desktop.main != null)
                {
                    windowsShown = new List<WindowA>();

                    WindowA w = WindowA.Create("UI/TutorialMessage");
                    Text txt = w.GetComponentInChildren<Text>();
                    if (txt != null)
                        txt.text = textToShow;

                    w.Show();

                    windowsShown.Add(w);
                }
            }

            tutorialShown = true;
        }

        RefreshHighlightList();
	}

    void APlayerEntered(Player p)
    {
        if (narration == null)
            ShowTutorial();
        else
            ShowNarration(p.kind);
    }

    void APlayerLeft(Player p)
    {
        if(narration == null)
            HideTutorial();
    }

    public override void WriteSignificantInfo(ref System.IO.BinaryWriter bw)
    {
        base.WriteSignificantInfo(ref bw);

        int c = blocksToHighlight != null ? blocksToHighlight.Length : 0;
        base.WriteSignificantValue(ref bw, "numHighlights", c);
        for(int i = 0; i < c; i++)
        {
            WriteSignificantValue(ref bw, "hl" + i.ToString(), blocksToHighlight[i].blockIdentifier);
        }

        WriteSignificantValue(ref bw, "text", textToShow);

        WriteSignificantValue(ref bw, "szx", (byte)areaSize.x);
        WriteSignificantValue(ref bw, "szy", (byte)areaSize.y);

        byte id = 255;
        if (narration != null)
            id = narration.blockIdentifier;
        WriteSignificantValue(ref bw, "narration", id);
    }

    byte narrationBlockID = 255;
    public override void ReadSignificantInfo(ref System.IO.BinaryReader br)
    {
        base.ReadSignificantInfo(ref br);

        int c = ReadSignificantValueInt32(ref br, "numHighlights");

        if (c > 0)
        {
            blocksToHighlightIDs = new int[c];
            for (int i = 0; i < c; i++)
            {
                blocksToHighlightIDs[i] = ReadSignificantValueByte(ref br, "hl" + i.ToString());
            }
        }

        textToShow = ReadSignificantValueString(ref br, "text");

        areaSize.x = (float)ReadSignificantValueByte(ref br, "szx");
        areaSize.y = (float)ReadSignificantValueByte(ref br, "szy");

        UpdateArea();

        narrationBlockID = ReadSignificantValueByte(ref br, "narration");
    }
    public override void LinkSignificantValues(Block[] blocks)
    {
        if (blocksToHighlightIDs != null)
        {
            blocksToHighlight = new Block[blocksToHighlightIDs.Length];
            for (int i = 0; i < blocksToHighlightIDs.Length; i++)
            {
                blocksToHighlight[i] = FindBlockByID(blocks, (byte)blocksToHighlightIDs[i]);
            }
        }

        if (narrationBlockID != 255)
            narration = FindBlockByID(blocks, narrationBlockID) as NarrationBlock;
    }

    List<WindowA> windowsShown;
    bool tutorialShown = false;
    bool mustShowTutorial = false;

    void ShowTutorial()
    {
        if (state == BlockState.State_Off) return;

        if(alwaysOn == false)
            state = BlockState.State_Off;

        mustShowTutorial = true;
    }

    void RefreshHighlightList()
    {
        if (tutorialShown == false) return;

        List<Block> list = new List<Block>();

        if (blocksToHighlight == null || blocksToHighlight.Length == 0) return;

        for (int i = 0; i < blocksToHighlight.Length; i++)
        {
            if (blocksToHighlight[i] != null)
                list.Add(blocksToHighlight[i]);
        }

        foreach (Player p in TheGame.Instance.players)
        {
            Block b = p.blockUnderneath;
            if (b != null)
                list.Add(b);
        }

        //if(TheGame.Instance.highlightMask != null)
        //    TheGame.Instance.highlightMask.HighlightBlocks(ref list);
    }

    void HideTutorial()
    {
        if(windowsShown != null)
        {
            foreach (WindowA w in windowsShown)
                w.Close();
            windowsShown = null;
        }

        tutorialShown = false;

        if (TheGame.Instance.highlightMask != null)
            TheGame.Instance.highlightMask.ClearHighlights();
    }

    void UpdateArea()
    {
        Vector3 min = new Vector3(1.0f, 0.0f, 1.0f) * (-GameSession.gridUnit * 0.5f);
        Vector3 max = min + new Vector3(areaSize.x, 1.0f, areaSize.y) * GameSession.gridUnit;
        Bounds b = new Bounds();
        b.SetMinMax(min, max);

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.center = b.center;
        boxCollider.size = b.size;

        if (indicator != null)
        {
            Renderer r = indicator.GetComponentInChildren<Renderer>();
#if UNITY_EDITOR

            if (r != null && Application.isPlaying == false)
            {
                r.gameObject.transform.localScale = new Vector3(boxCollider.size.x, 1.0f, boxCollider.size.z) / GameSession.gridUnit;
                r.gameObject.transform.localPosition = new Vector3(boxCollider.center.x, 0.5f, boxCollider.center.z);
                r.sharedMaterial.SetTextureScale("_MainTex", new Vector2(boxCollider.size.x / GameSession.gridUnit * 0.5f, boxCollider.size.z / GameSession.gridUnit * 0.5f));
            }
            else
            {
                if (r != null)
                    r.enabled = false;
            }
#else
            if(r != null)
                r.enabled = false;

#endif
        }
    }

    void OnValidate()
    {
        UpdateArea();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (windowsShown != null)
        {
            foreach (WindowA w in windowsShown)
                w.Close();
            windowsShown = null;
        }
    }

    void ShowNarration(Player.Kind who)
    {
        if (narration != null && state == BlockState.State_On)
            narration.Show(who);

        if (alwaysOn == false)
            state = BlockState.State_Off;
    }
}
