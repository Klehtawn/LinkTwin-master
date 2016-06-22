using UnityEngine;
using System.Collections;
using System;

public class StateChangerOnEnter : BlockModifier {

    Block block;

    public Block.BlockState stateToSet = Block.BlockState.State_On;
    public float delay = 3.0f;

    public Action<Player> OnActivated;

	void Start ()
    {
        block = GetComponent<Block>();
        block.OnPlayerInTheMiddle += APlayerEntered;

        if (stateToSet == Block.BlockState.State_On)
            block.SetState(Block.BlockState.State_Off);
        else
            block.SetState(Block.BlockState.State_On);
	}

    float stateTimer = 0.0f;
	void Update ()
    {
	    if(aPlayerEntered)
        {
            if (stateTimer < delay)
            {
                stateTimer += Time.deltaTime;
                if (stateTimer >= delay)
                {
                    block.SetState(stateToSet);
                }
            }
        }
	}

    bool aPlayerEntered = false;
    void APlayerEntered(Player p)
    {
        stateTimer = 0.0f;
        aPlayerEntered = true;
        if (OnActivated != null)
            OnActivated(p);
    }

    public override void WriteSignificantInfo(ref System.IO.BinaryWriter bw)
    {
        base.WriteSignificantInfo(ref bw);
        WriteSignificantValue(ref bw, "delay", delay);
        WriteSignificantValue(ref bw, "state", (byte)stateToSet);
    }

     public override void ReadSignificantInfo(ref System.IO.BinaryReader br)
     {
         base.ReadSignificantInfo(ref br);

         delay = ReadSignificantValueFloat(ref br, "delay");
         stateToSet = (Block.BlockState)ReadSignificantValueByte(ref br, "state");
     }

     public bool IsActivating()
     {
         return aPlayerEntered && stateTimer < delay;
     }
}
