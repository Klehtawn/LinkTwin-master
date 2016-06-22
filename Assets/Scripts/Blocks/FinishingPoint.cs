using UnityEngine;
using System.Collections;

public class FinishingPoint : Block {
	// Use this for initialization

    public override void Awake()
    {
        base.Awake();

        blockType = BlockType.Finish;

        OnPlayerInTheMiddle += OnPlayerEntered;
    }

	public override void Start ()
    {
        base.Start();
	}

    void OnPlayerEntered(Player who)
    {
        GameEvents.Send(who.kind, GameEvents.EventType.ReachedFinish);
    }

    float timer = 0.0f;
    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        if (playersOnSite.Count > 1)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0.0f;
        }

        if (timer > 0.7f)
        {
            TheGame.Instance.TriggerGameLost();
        }
	}

    protected override void StateChanged(BlockState prev, BlockState current)
    {
        //EnableRendering(current == BlockState.State_On);
    }
}
