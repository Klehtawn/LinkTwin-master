using UnityEngine;
using System.Collections;

public class BlockModifier : SerializerSimple
{
    protected bool behaviourActive = true;

    public virtual void EnableBehaviour(bool enable = true)
    {
        behaviourActive = enable;
    }

    public void DisableBehaviour()
    {
        EnableBehaviour(false);
    }

    protected Block FindBlockByID(Block[] blocks, byte id)
    {
        foreach(Block b in blocks)
        {
            if (b.blockIdentifier == id)
                return b;
        }
        return null;
    }
}
