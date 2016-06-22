using UnityEngine;
using System.Collections;

public class AutomaticStateChanger : BlockModifier {

    Block block;

    public float onDuration = -1.0f;
    public float offDuration = -1.0f;

    public bool defaultStateIsOn = true;

    public bool loop = true;

	void Start () {
        block = GetComponent<Block>();

        if (defaultStateIsOn || onDuration < 0.0f)
            block.SetState(Block.BlockState.State_On);
        else
            block.SetState(Block.BlockState.State_Off);
	}

    float onOffTimer = 0.0f;
	void Update ()
    {
        if (behaviourActive == false) return;

        if(onOffTimer < onDuration || onOffTimer < offDuration)
            onOffTimer += Time.deltaTime;

        if(block.state == Block.BlockState.State_On && onOffTimer >= onDuration && onDuration >= 0.0f)
        {
            block.SetState(Block.BlockState.State_Off);
            if (loop)
                onOffTimer = 0.0f;
        }

        if (block.state == Block.BlockState.State_Off && onOffTimer >= offDuration)
        {
            block.SetState(Block.BlockState.State_On);
            if(loop)
                onOffTimer = 0.0f;
        }
	}

    public override void WriteSignificantInfo(ref System.IO.BinaryWriter bw)
    {
        base.WriteSignificantInfo(ref bw);

        WriteSignificantValue(ref bw, "default", defaultStateIsOn);
        WriteSignificantValue(ref bw, "onDuration", onDuration);
        WriteSignificantValue(ref bw, "offDuration", offDuration);
        WriteSignificantValue(ref bw, "loop", loop);
    }

    public override void ReadSignificantInfo(ref System.IO.BinaryReader br)
    {
        base.ReadSignificantInfo(ref br);

        defaultStateIsOn = ReadSignificantValueBool(ref br, "default");
        onDuration = ReadSignificantValueFloat(ref br, "onDuration");
        offDuration = ReadSignificantValueFloat(ref br, "offDuration");
        loop = ReadSignificantValueBool(ref br, "loop");
    }
}
