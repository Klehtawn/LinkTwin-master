using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameplaySteps : MonoBehaviour {

    public static LeanTweenType animationType = LeanTweenType.linear;

    public class BlockMinimalState
    {
        public Block source;
        Vector3 position;
        Vector3 scale;
        Block.BlockState state;
        bool active;
        bool snap;

        public BlockMinimalState(Block b)
        {
            source = b;
            position = b.position;
            scale = b.transform.localScale;
            state = b.state;
            active = b.gameObject.activeSelf;
            snap = true;

            SnapToGrid sg = b.GetComponent<SnapToGrid>();
            if (sg != null)
                snap = sg.enabled;
        }

        public void CopyToBlock()
        {
            if (source == null)
            {
                return;
            }

            SnapToGrid sg = source.GetComponent<SnapToGrid>();
            if (sg != null)
                sg.enabled = snap;

            source.position = position;
            source.transform.localScale = scale;
            source.SetState(state, true);
            source.gameObject.SetActive(active);
        }



        public void CopyToBlockAnimated(float duration)
        {
            if(source == null)
            {
                return;
            }

            source.gameObject.SetActive(active);

            SnapToGrid sg = source.GetComponent<SnapToGrid>();
            if (sg != null)
                sg.enabled = false;

            LeanTween.cancel(source.gameObject);
            LeanTween.scale(source.gameObject, scale, duration).setEase(animationType);
            source.DisableBehaviour();
            LeanTween.move(source.gameObject, position, duration).setEase(animationType).setOnComplete(() =>
                {
                    source.SetState(state, true);
                    if (sg != null)
                        sg.enabled = snap;
                    source.EnableBehaviour();
                });
        }
    }

    public class PlayerMinimalState
    {
        Player source;
        Vector3 position;

        public PlayerMinimalState(Player p)
        {
            source = p;
            position = p.position;
        }

        public void CopyToPlayer()
        {
            source.position = position;
        }

        public void CopyToPlayerAnimated(float duration)
        {
            LeanTween.cancel(source.gameObject);
            LeanTween.move(source.gameObject, position, duration).setEase(animationType).setOnComplete(() =>
                {

                });
        }
    }

    public class GameplayStep
    {
        public List<PlayerMinimalState> playerStates = new List<PlayerMinimalState>();
        public List<BlockMinimalState> blockStates = new List<BlockMinimalState>();
    }

    List<GameplayStep> recordedSteps = new List<GameplayStep>();
    int recordingCursor = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Clear()
    {
        recordedSteps.Clear();
        recordingCursor = 0;
    }

    public void RecordStep(TheGame game)
    {
        recordedSteps.RemoveRange(recordingCursor, recordedSteps.Count - recordingCursor);

        game.blocks.GenerateIDs();

        GameplayStep gs = new GameplayStep();

        foreach(Player p in game.players)
        {
            gs.playerStates.Add(new PlayerMinimalState(p));
        }

        foreach(Block b in game.blocks.items)
        {
            gs.blockStates.Add(new BlockMinimalState(b));
        }

        recordedSteps.Add(gs);
        recordingCursor = recordedSteps.Count;
    }

    public void PlayStep(TheGame game, int step, float duration = 0.0f)
    {
        if (step < 0) return;
        if (step >= recordedSteps.Count) return;

        GameplayStep gs = recordedSteps[step];

        if(true)//duration == 0.0f)
        {
            foreach(PlayerMinimalState pms in gs.playerStates)
            {
                pms.CopyToPlayer();
            }

            foreach(BlockMinimalState bms in gs.blockStates)
            {
                bms.CopyToBlock();
            }
        }
        else
        {
            foreach (PlayerMinimalState pms in gs.playerStates)
            {
                pms.CopyToPlayerAnimated(duration);
            }

            foreach (BlockMinimalState bms in gs.blockStates)
            {
                bms.CopyToBlockAnimated(duration);
            }
        }
    }

    public int Count
    {
        get
        {
            return recordedSteps.Count;
        }
    }

    public int Cursor
    {
        get
        {
            return recordingCursor;
        }
        set
        {
            if(value != recordingCursor)
            {
                recordingCursor = Mathf.Clamp(value, 0, recordedSteps.Count);
                PlayStep(TheGame.Instance, recordingCursor, 0.25f);
            }
        }
    }
}
