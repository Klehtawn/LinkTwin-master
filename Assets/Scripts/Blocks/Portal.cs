using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[SelectionBase]
public class Portal : Block {

    public Portal target;
    private int targetIdentifier = -1;
	// Use this for initialization
	public override void Start ()
    {
        base.Start();
        OnPlayerHit += APlayerEntered;
        OnBlockHit += ABlockEntered;

        AssignColors();
	}
	
	// Update is called once per frame
	public override void Update () {
        base.Update();
	}

    bool teleportFailed = false;

    void APlayerEntered(Player p)
    {
        if (target == null) return;

        if (p.isTeleporting) return;

        if (state == BlockState.State_Off) return;

        p.isTeleporting = true;
        teleportFailed = false;

        GameEvents.Send(p.kind, GameEvents.EventType.EnterPortal);

        LeanTween.scale(p.gameObject, Vector3.one * 0.001f, 0.3f).setEase(LeanTweenType.easeInBack).setDelay(0.0f).setOnComplete(() =>
            {
                Vector3 pos = target.transform.position;
                pos.y = p.transform.position.y;
                p.transform.position = pos;

                /*LeanTween.scale(p.gameObject, Vector3.one, 0.2f).setEase(LeanTweenType.easeOutBack).setOnComplete(() =>
                    {
                        Vector3 newPos = new Vector3(p.lastMove.x, 0.0f, p.lastMove.y);
                        newPos *= GameSession.gridUnit;
                        newPos += SnapToGrid.Snap(p.transform.position);
                        p.MoveTo(newPos, OnMovingCompleted);
                    });*/

                LeanTween.scale(p.gameObject, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBack).setOnComplete(()=>
                        {
                            if(teleportFailed)
                            {
                                p.lastMove *= -1.0f;
                                p.isTeleporting = false;
                                target.APlayerEntered(p);
                            }
                        }
                    );
                {
                    Vector3 newPos = new Vector3(p.lastMove.x, 0.0f, p.lastMove.y);
                    newPos *= GameSession.gridUnit;
                    newPos += SnapToGrid.Snap(p.transform.position);

                    if (p.CanMoveTo(newPos) == false || p.CanPushPlayersFromPos(newPos) == false)
                    {
                        teleportFailed = true;
                    }
                    else
                    {
                        p.MoveTo(newPos, OnMovingCompleted);
                        p.PushPlayersFromPos(newPos);
                    }
                }
            });
    }

    void ABlockEntered(Block b, Vector3 enterDir)
    {
        if (b.isTeleporting) return;

        teleportFailed = false;
        b.isTeleporting = true;
        b.isEnteringTeleport = true;

        LeanTween.scale(b.gameObject, Vector3.one * 0.001f, 0.3f).setEase(LeanTweenType.easeInBack).setDelay(0.0f).setOnComplete(() =>
        {
            b.isEnteringTeleport = false;

            b.ModifyPosition(target.position);

            b.isLeavingTeleport = true;

            Vector3 newPos = b.position + enterDir * GameSession.gridUnit;
            Block nextBlock = root.blocks.ItemAt(newPos);

            if (nextBlock != null && nextBlock.CanBeWalkedOn(enterDir))
            {
                if (b.ModifyPosition(newPos, true) == false)
                    teleportFailed = true;
            }
            else // cannot walk there
            {
                teleportFailed = true;
            }

            LeanTween.scale(b.gameObject, Vector3.one, 0.3f).setEase(LeanTweenType.easeOutBack).setOnComplete(() =>
                {
                    b.isTeleporting = false;
                    b.isLeavingTeleport = false;
                    if (teleportFailed)
                    {
                        target.ABlockEntered(b, -enterDir);
                    }
                });
        });
    }

    void OnMovingCompleted(Player p)
    {
        p.isTeleporting = false;
        p.FinishedMoving(p.transform.position);

        if (root != null)
        {
            Block b = root.blocks.ItemAt(p.position);
            if(b != null && b.blockType == BlockType.Portal)
            {
                Portal portal = b as Portal;
                portal.APlayerEntered(p);
            }
        }
    }

    public override void WriteSignificantInfo(ref System.IO.BinaryWriter bw)
    {
        base.WriteSignificantInfo(ref bw);

        if(target != null)
            WriteSignificantValue(ref bw, "tgt", target.blockIdentifier);
    }

    public override void ReadSignificantInfo(ref System.IO.BinaryReader br)
    {
        base.ReadSignificantInfo(ref br);

        targetIdentifier = ReadSignificantValueByte(ref br, "tgt", 255);
    }
    public override void LinkSignificantValues(Block[] blocks)
    {
        if (target != null) return;

        if (targetIdentifier > 0)
        {
            Block b = FindBlockByID(blocks, (byte)targetIdentifier);
            if (b != null)
                target = b.GetComponent<Portal>();
        }

        if (Application.isPlaying)
            AssignColors();
    }

    public void AssignColors(bool useColorOptions = false)
    {
        Color colorToUse = Color.magenta;

        if (colorSet) return;

        if(availableColors == null)
        {
            availableColors = new List<int>();
            for(int i = 0; i < colorOptions.Length; i++)
            {
                availableColors.Add(i);
            }
        }


        if (target == null || availableColors.Count == 0 || useColorOptions == false)
        {
            HSBColor hsb = new HSBColor(UnityEngine.Random.Range(0.0f, 1.0f), 0.5f, 0.7f);
            colorToUse = hsb.ToColor();
        }
        else
        {
            int i = 0;// UnityEngine.Random.Range(0, availableColors.Count - 1);
            int k = availableColors[i];
            colorToUse = colorOptions[k];
            availableColors.RemoveAt(i);
        }

        MeshVertexColors mvc = GetComponentInChildren<MeshVertexColors>();
        if (mvc != null)
            mvc.color = colorToUse;

        if (target != null)
        {
            mvc = target.GetComponentInChildren<MeshVertexColors>();
            if (mvc != null)
                mvc.color = colorToUse;

            if (useColorOptions)
            {
                colorSet = true;
                target.colorSet = true;
            }
        }

        if(target == null || useColorOptions == false)
        {
            colorSet = false;
        }
    }

    /*void OnValidate()
    {
        AssignColors();
    }*/

    public Color[] colorOptions;

    [NonSerialized]
    protected static List<int> availableColors = null;

    [NonSerialized]
    public bool colorSet = false;

    public override void OnGamePreStart()
    {
        base.OnGamePreStart();

        availableColors = null;
    }

    public override void OnGameStarted()
    {
        base.OnGameStarted();

        AssignColors(true);
    }
}
