using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFindSimple
{
    private static Blocks blocks = new Blocks();
    private static float heuristic_cost_estimate(Block start, Block goal)
    {
        Vector3 d = start.transform.position - goal.transform.position;
        return Mathf.Abs(d.x) + Mathf.Abs(d.z);
    }

    private static List<Block> reconstruct_path(Dictionary<Block, Block> Came_From, Block current)
    {
        List<Block> total_path = new List<Block>();
        total_path.Add(current);

        while (Came_From.ContainsKey(current))
        {
            current = Came_From[current];
            total_path.Add(current);
        }

        return total_path;
    }

    private static Block getNeighbour(Block b, Vector3 dir, Block goal)
    {
        Vector3 pos = b.transform.position + dir * GameSession.gridUnit;
        Block n = blocks.GroundAt(pos);
        if (n == null) return null;
        if (n == goal)
            return n;
        if ((n as GroundBlock) != null) return n;

        return null;
    }

    private static void getNeighbours(Block b, ref List<Block> res, Block goal)
    {
        res.Clear();

        Block n = getNeighbour(b, Vector3.right, goal);
        if (n != null) res.Add(n);

        n = getNeighbour(b, Vector3.left, goal);
        if (n != null) res.Add(n);

        n = getNeighbour(b, Vector3.forward, goal);
        if (n != null) res.Add(n);

        n = getNeighbour(b, Vector3.back, goal);
        if (n != null) res.Add(n);
    }

    private static float dist_between(Block b1, Block b2)
    {
        Vector3 d = b1.transform.position - b2.transform.position;
        d.y = 0.0f;
        return d.magnitude;
    }

    // A* from wikipedia
    public static List<Block> Find(Block start, Block goal)
    {
        blocks.Fill(start.transform.GetComponentInParent<LevelRoot>().transform);

        List<Block> ClosedSet = new List<Block>();
        List<Block> OpenSet = new List<Block>();
        OpenSet.Add(start);

        Dictionary<Block, Block> Came_From = new Dictionary<Block, Block>();

        Dictionary<Block, float> g_score = new Dictionary<Block, float>();
        Dictionary<Block, float> f_score = new Dictionary<Block, float>();

        g_score[start] = 0.0f;
        f_score[start] = heuristic_cost_estimate(start, goal);

        List<Block> neighbours = new List<Block>();

        float _infinity = 1000000.0f;

        while(OpenSet.Count > 0)
        {
            float min = _infinity * 2.0f;
            Block current = null;
            foreach(Block b in OpenSet)
            {
                float v = f_score.ContainsKey(b) ? f_score[b] : _infinity;
                if(v < min)
                {
                    current = b;
                    min = v;
                }
            }

            if(current == goal)
            {
                return reconstruct_path(Came_From, goal);
            }

            OpenSet.Remove(current);
            ClosedSet.Add(current);

            getNeighbours(current, ref neighbours, goal);
            foreach(Block n in neighbours)
            {
                if (ClosedSet.Contains(n)) continue;

                float tentative_g_score = (g_score.ContainsKey(current) ? g_score[current] : _infinity) + 1.0f;// dist_between(current, n);

                if (OpenSet.Contains(n) == false)
                    OpenSet.Add(n);
                else if (tentative_g_score >= g_score[n])
                    continue;

                Came_From[n] = current;
                g_score[n] = tentative_g_score;
                f_score[n] = g_score[n] + heuristic_cost_estimate(n, goal);
            }
        }

        return null;
    }
}