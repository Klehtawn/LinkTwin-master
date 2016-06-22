using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Blocks
{
    public List<Block> items = new List<Block>();
    public List<Block> ground = new List<Block>();

    public void Clear()
    {
        items.Clear();
        ground.Clear();
    }

    public void Fill(Transform root)
    {
        if (root == null)
            Fill((GameObject)null);
        else
            Fill(root.gameObject);
    }
    public void Fill(GameObject root = null)
    {
        Clear();

        Block[] result = null;

        if (root != null)
            result = root.GetComponentsInChildren<Block>();
        else
            result = GameObject.FindObjectsOfType<Block>();
        foreach(Block b in result)
        {
            if (b == null) continue;
            if (b as GroundBlock != null)
            {
                if (b.transform.parent == null) continue;
                ground.Add(b);
            }
            else
                items.Add(b);
        }
    }

    public Block ItemAt(Vector3 pos, Block ignore = null)
    {
        float halfGrid = GameSession.gridUnit * 0.5f;

        foreach (Block b in items)
        {
            if (b == null) continue;
            if (b == ignore) continue;
            if (b.gameObject.activeSelf == false) continue;
            if (b.ignoreOnGamePlay) continue;
            Vector3 center = b.position; center.y = 0.0f;
            Vector3 dist = pos - center;
            if (Mathf.Abs(dist.x) <= halfGrid && Mathf.Abs(dist.z) <= halfGrid)
            {
                return b;
            }
        }

        return GroundAt(pos);
    }

    public T ItemAt<T>(Vector3 pos) where T : Block
    {
        float halfGrid = GameSession.gridUnit * 0.5f;

        foreach (Block b in items)
        {
            if (b == null) continue;
            if (b.gameObject.activeSelf == false) continue;
            if (b as T == null) continue;
            if (b.ignoreOnGamePlay) continue;

            Vector3 center = b.position; center.y = 0.0f;
            Vector3 dist = pos - center;
            if(Mathf.Abs(dist.x) <= halfGrid && Mathf.Abs(dist.z) <= halfGrid)
            {
                return b as T;
            }
        }

        return null;
    }

    public Block GroundAt(Vector3 pos)
    {
        foreach (Block b in ground)
        {
            if (b == null) continue;
            Collider col = b.GetComponent<Collider>();
            if (col == null) continue;
            Vector3 p = pos;
            p.y = col.bounds.center.y;
            if (col.bounds.Contains(p))
                return b;
        }

        return null;
    }

    /*public void GetBlocksOfType(System.Type t, ref List<Block> list)
    {
        foreach(Block b in ground)
        {
            if (b.GetType().IsSubclassOf(t) || b.GetType() == t)
                list.Add(b);
        }

        foreach (Block b in items)
        {
            if (b.GetType().IsSubclassOf(t) || b.GetType() == t)
                list.Add(b);
        }
    }*/

    public void GetBlocksOfType(Block.BlockType bt, ref List<Block> list)
    {
        foreach (Block b in ground)
        {
            if (b.blockType == bt && b.gameObject.activeSelf)
                list.Add(b);
        }

        foreach (Block b in items)
        {
            if (b.blockType == bt && b.gameObject.activeSelf)
                list.Add(b);
        }
    }

    public T[] GetBlocksOfType<T>() where T : Block
    {
        List<Block> res = new List<Block>();

        foreach (Block b in ground)
        {
            T tb = b as T;
            if (tb != null && b.gameObject.activeSelf)
                res.Add(b);
        }

        foreach (Block b in items)
        {
            T tb = b as T;
            if (tb != null && b.gameObject.activeSelf)
                res.Add(b);
        }

        T[] ret = new T[res.Count];
        for (int i = 0; i < res.Count; i++ )
        {
            ret[i] = res[i] as T;
        }
        return ret;
    }

    public void GetBlocksOfType<T>(ref List<Block> res) where T : Block
    {
        res = new List<Block>();

        foreach (Block b in ground)
        {
            T tb = b as T;
            if (tb != null && b.gameObject.activeSelf)
                res.Add(b);
        }

        foreach (Block b in items)
        {
            T tb = b as T;
            if (tb != null && b.gameObject.activeSelf)
                res.Add(b);
        }
    }

    public bool Intersects(Vector3 startPos, Vector3 endPos)
    {
        Vector3 dir = endPos - startPos;
        float len = dir.magnitude;
        if(len <= 0.0001f)
        {
            return false;
        }

        float dist;
        foreach (Block b in items)
        {
            if (b == null) continue;
            if (b.navigable) continue;
            if (b.state == Block.BlockState.State_Off) continue;
            if (b.ignoreOnGamePlay) continue;

            Collider col = b.GetComponent<Collider>();
            if (col == null) continue;

            if (col.bounds.Contains(startPos) || col.bounds.Contains(endPos))
                return true;

            Vector3 origin = startPos;
            origin.y = col.bounds.center.y;
            Ray ray = new Ray(origin, dir);
            if (col.bounds.IntersectRay(ray, out dist) && dist <= len)
                return true;
        }

        return false;
    }

    public Vector3 GetGroundCenter()
    {
        /*Vector3 center = Vector3.zero;
        foreach(Block b in ground)
        {
            center += b.transform.position;
        }

        if(ground.Count > 0)
        {
            center /= (float)ground.Count;
        }

        return center;*/

        Vector3 min = Vector3.one * 100000.0f;
        Vector3 max = -min;
        foreach (Block b in ground)
        {
            if (b == null) continue;
            min = Vector3.Min(min, b.transform.position);
            max = Vector3.Max(max, b.transform.position);
        }

        min -= Vector3.one * GameSession.gridUnit * 0.5f;
        max += Vector3.one * GameSession.gridUnit * 0.5f;

        return (max + min) * 0.5f;
    }

    public Vector3 GetGroundSize()
    {
        if (ground.Count == 0)
            return Vector3.zero;

        Vector3 min = Vector3.one * 100000.0f;
        Vector3 max = -min;
        foreach (Block b in ground)
        {
            if (b == null) continue;
            min = Vector3.Min(min, b.transform.position);
            max = Vector3.Max(max, b.transform.position);
        }

        min -= Vector3.one * GameSession.gridUnit * 0.5f;
        max += Vector3.one * GameSession.gridUnit * 0.5f;

        return max - min;
    }

    public Vector3 GetGroundMin()
    {
        if (ground.Count == 0)
            return Vector3.zero;

        Vector3 min = Vector3.one * 100000.0f;
        foreach (Block b in ground)
        {
            min = Vector3.Min(min, b.transform.position);
        }
        return min;
    }

    public Vector3 GetGroundMax()
    {
        if (ground.Count == 0)
            return Vector3.zero;

        Vector3 max = -Vector3.one * 100000.0f;
        foreach (Block b in ground)
        {
            max = Vector3.Max(max, b.transform.position);
        }
        return max;
    }

    public void GetGroundMinMax(ref Vector3 min, ref Vector3 max)
    {
        max = min = Vector3.zero;
        if (ground.Count == 0)
            return;

        max = -Vector3.one * 100000.0f;
        min = Vector3.one * 100000.0f;
        foreach (Block b in ground)
        {
            max = Vector3.Max(max, b.transform.position);
            min = Vector3.Min(min, b.transform.position);
        }
    }

    public Block GetByIdentifier(byte id)
    {
        foreach(Block b in items)
        {
            if (b.blockIdentifier == id && b.gameObject.activeSelf)
                return b;
        }

        return null;
    }

    public List<Vector3> GetEmptyNeighbours()
    {
        List<Vector3> ret = new List<Vector3>();

        Vector3[] dirs = new Vector3[4];
        dirs[0] = Vector3.left;
        dirs[1] = Vector3.right;
        dirs[2] = Vector3.forward;
        dirs[3] = Vector3.back;
        foreach (Block b in TheGame.Instance.blocks.ground)
        {
            foreach (Vector3 d in dirs)
            {
                Vector3 p = b.position + d * GameSession.gridUnit;
                if(GroundAt(p) == null)
                {
                    if (ret.Contains(p) == false)
                        ret.Add(p);
                }
            }
        }

        return ret;
    }

    public void GenerateIDs()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].blockIdentifier = (byte)(i + 1);
        }
    }

    public void EnableBehaviour(bool enable = true)
    {
        foreach (Block b in items)
            b.EnableBehaviour(enable);
    }
}