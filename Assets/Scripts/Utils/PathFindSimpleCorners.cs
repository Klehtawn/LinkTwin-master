using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockCorner
{
    public Vector3 position;
    public static bool operator ==(BlockCorner a, BlockCorner b)
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        Vector3 d = a.position - b.position; d.y = 0.0f;
        return d.sqrMagnitude < 0.005f;
    }

    public static bool operator !=(BlockCorner a, BlockCorner b)
    {
        return !(a == b);
    }

    public bool Equals(BlockCorner p)
    {
        // If parameter is null return false:
        if ((object)p == null)
        {
            return false;
        }

        return p == this;
    }

    public override bool Equals(System.Object obj)
    {
        // If parameter is null return false.
        if (obj == null)
        {
            return false;
        }

        // If parameter cannot be cast to Point return false.
        BlockCorner p = obj as BlockCorner;
        if ((System.Object)p == null)
        {
            return false;
        }

        // Return true if the fields match:
        return p == this;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public static BlockCorner getCenter(Block b)
    {
        BlockCorner ret = new BlockCorner();
        ret.position = b.position;
        return ret;
    }
    public static BlockCorner[] getCorners(Block b)
    {
        BlockCorner[] ret = new BlockCorner[4];
        ret[0] = new BlockCorner();
        ret[1] = new BlockCorner();
        ret[2] = new BlockCorner();
        ret[3] = new BlockCorner();

        float half = GameSession.gridUnit * 0.5f;
        ret[0].position = b.position + (Vector3.left + Vector3.forward) * half;
        ret[1].position = b.position + (Vector3.right + Vector3.forward) * half;
        ret[2].position = b.position + (Vector3.left + Vector3.back) * half;
        ret[3].position = b.position + (Vector3.right + Vector3.back) * half;
        return ret;
    }
};

public class PathFindSimpleCorners
{
    private static List<BlockCorner> corners = new List<BlockCorner>();
    private static float heuristic_cost_estimate(BlockCorner start, BlockCorner goal)
    {
        Vector3 d = start.position - goal.position;
        return Mathf.Abs(d.x) + Mathf.Abs(d.z);
    }

    private static List<BlockCorner> reconstruct_path(Dictionary<BlockCorner, BlockCorner> Came_From, BlockCorner current)
    {
        List<BlockCorner> total_path = new List<BlockCorner>();
        total_path.Add(current);

        while (Came_From.ContainsKey(current))
        {
            current = Came_From[current];
            total_path.Add(current);
        }

        return total_path;
    }

    private static BlockCorner getNeighbour(BlockCorner b, Vector3 dir)
    {
        BlockCorner n = new BlockCorner();
        n.position = b.position + dir * GameSession.gridUnit;
        if (corners.Contains(n))
            return n;
        
        return null;
    }

    private static void getNeighbours(BlockCorner b, ref List<BlockCorner> res)
    {
        res.Clear();

        BlockCorner n = getNeighbour(b, Vector3.right);
        if (n != null) res.Add(n);

        n = getNeighbour(b, Vector3.left);
        if (n != null) res.Add(n);

        n = getNeighbour(b, Vector3.forward);
        if (n != null) res.Add(n);

        n = getNeighbour(b, Vector3.back);
        if (n != null) res.Add(n);

        n = getNeighbour(b, Vector3.left * 0.5f + Vector3.forward * 0.5f);
        if (n != null) res.Add(n);

        n = getNeighbour(b, Vector3.right * 0.5f + Vector3.forward * 0.5f);
        if (n != null) res.Add(n);

        n = getNeighbour(b, Vector3.left * 0.5f + Vector3.back * 0.5f);
        if (n != null) res.Add(n);

        n = getNeighbour(b, Vector3.right * 0.5f + Vector3.back * 0.5f);
        if (n != null) res.Add(n);
    }

    private static float dist_between(BlockCorner b1, BlockCorner b2)
    {
        Vector3 d = b1.position - b2.position;
        d.y = 0.0f;
        return d.magnitude;
    }

    // A* from wikipedia
    public static List<BlockCorner> Find(Block start, Block goal)
    {
        LevelRoot root = start.transform.GetComponentInParent<LevelRoot>();
        if (root == null) return null;

        corners.Clear();

        foreach (Block b in root.blocks.ground)
        {
            BlockCorner[] bcs = BlockCorner.getCorners(b);
            foreach (BlockCorner bc in bcs)
                if (corners.Contains(bc) == false)
                    corners.Add(bc);
        }

        BlockCorner startCenter = BlockCorner.getCenter(start);
        BlockCorner goalCenter = BlockCorner.getCenter(goal);

        corners.Add(startCenter);
        corners.Add(goalCenter);


        List<BlockCorner> ClosedSet = new List<BlockCorner>();
        List<BlockCorner> OpenSet = new List<BlockCorner>();

        OpenSet.Add(BlockCorner.getCenter(start));

        Dictionary<BlockCorner, BlockCorner> Came_From = new Dictionary<BlockCorner, BlockCorner>();

        Dictionary<BlockCorner, float> g_score = new Dictionary<BlockCorner, float>();
        Dictionary<BlockCorner, float> f_score = new Dictionary<BlockCorner, float>();

        g_score[startCenter] = 0.0f;
        f_score[startCenter] = heuristic_cost_estimate(startCenter, goalCenter);

        List<BlockCorner> neighbours = new List<BlockCorner>();

        float _infinity = 1000000.0f;

        while(OpenSet.Count > 0)
        {
            float min = _infinity * 2.0f;
            BlockCorner current = null;
            foreach(BlockCorner b in OpenSet)
            {
                float v = f_score.ContainsKey(b) ? f_score[b] : _infinity;
                if(v < min)
                {
                    current = b;
                    min = v;
                }
            }

            if(current == goalCenter)
            {
                return reconstruct_path(Came_From, goalCenter);
            }

            OpenSet.Remove(current);
            ClosedSet.Add(current);

            getNeighbours(current, ref neighbours);

            foreach(BlockCorner n in neighbours)
            {
                if (ClosedSet.Contains(n)) continue;

                float tentative_g_score = (g_score.ContainsKey(current) ? g_score[current] : _infinity) + 1.0f;// dist_between(current, n);

                if (OpenSet.Contains(n) == false)
                    OpenSet.Add(n);
                else if (tentative_g_score >= g_score[n])
                    continue;

                Came_From[n] = current;
                g_score[n] = tentative_g_score;
                f_score[n] = g_score[n] + heuristic_cost_estimate(n, goalCenter);
            }
        }

        return null;
    }
}