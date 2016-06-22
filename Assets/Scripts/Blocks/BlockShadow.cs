using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BlockShadow : MonoBehaviour
{
    public static float yOffset = 20.0f;

    static List<BlockShadow> allShadows = new List<BlockShadow>();

    public float currentY = 0.0f;

    private Block block;

    private MeshExtrude meshExtrude;

    void Awake()
    {
        block = GetComponentInParent<Block>();

        meshExtrude = GetComponent<MeshExtrude>();

        if(GameSession.gameState == GameSession.GameState.Workshop)
        {
            gameObject.SetActive(false);
        }

        //redo shadows when changing state
    }

    public void IncrementOffset()
    {
        if(allShadows.Contains(this) == false)
            allShadows.Add(this);
        
        currentY = yOffset;
        /*Vector3 p = transform.position;
        p.y += yOffset;
        transform.position = p;*/
        yOffset += 0.05f;

        if (meshExtrude != null &&  meshExtrude.collapsedAlready == false && meshExtrude.isDynamic == false)
        {
            List<BlockShadow> connected = new List<BlockShadow>();
            GetAllConnectedBlocks(this, ref connected);

            for(int i = 0; i < connected.Count; i++)
            {
                if(connected[i].GetComponent<MeshExtrude>().collapsedAlready)
                {
                    connected.RemoveAt(i);
                    i--;
                }
            }

            for(int i = 0; i < connected.Count; i++)
            {
                MeshExtrude ext = connected[i].GetComponent<MeshExtrude>();
                if (ext == meshExtrude) continue;
                if (ext.isDynamic) continue;

                meshExtrude.Append(ext);
            }

            meshExtrude.CreateExtrusion();

            meshExtrude.collapsedAlready = true;
        }
    }

    public static void RefreshShadows()
    {
        foreach(BlockShadow bs in allShadows)
        {
            bs.meshExtrude.Reset();
        }

        foreach(BlockShadow bs in allShadows)
        {
            if (bs.gameObject.activeInHierarchy == false) continue;
            bs.IncrementOffset();
        }
    }

    static public void Reset()
    {
        yOffset = 0.0f;
        allShadows.Clear();
    }

    void GetAllConnectedBlocks(BlockShadow me, ref List<BlockShadow> list)
    {
        if (list.Contains(me) == false)
            list.Add(me);

        Block[] n = me.block.GetNeighbours(false, false);

        foreach(Block nn in n)
        {
            BlockShadow bs = nn.GetComponentInChildren<BlockShadow>();
            if(bs != null && list.Contains(bs) == false)
            {
                if(bs.meshExtrude.isDynamic == false )
                    GetAllConnectedBlocks(bs, ref list);
            }
        }
    }

}
