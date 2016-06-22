using System.Collections.Generic;
using UnityEngine;

namespace LevelSimulation
{
    public class Solver
    {
        private MoveDir[] directions =
        {
            new MoveDir("U",  0,  1, 1),
            new MoveDir("D",  0, -1, 2),
            new MoveDir("L", -1,  0, 3),
            new MoveDir("R",  1,  0, 4),
        };

        private Board cleanedBoard;

        private Board startingBoard;
        public List<Solution> Solutions
        {
            get; set;
        }

        public void Init(Board b)
        {
            startingBoard = new Board(b);
            startingBoard.SearchSpecialEntities();
        }

        public void Init(LevelRoot root)
        {
            startingBoard = new Board(root);
            startingBoard.SearchSpecialEntities();
        }

        Board NewPosition(Board b, MoveDir dir)
        {
            //Debug.Log("can move " + dir.name + ": " + b.CanMove(dir.dr, dir.dc));
            if (!b.CanMove(dir.dr, dir.dc))
                return null;

            Board nb = new Board(b);
            nb.Move(dir.dr, dir.dc);

            return nb;
        }

        private class Node
        {
            public MoveDir dir;
            public Board board;
            public Node parent;
            public Node(Board board, MoveDir dir, Node parent)
            {
                this.board = board;
                this.dir = dir;
                this.parent = parent;
            }
        }

        private void BreadthFirstSearch(Node root, float maxtime, int maxsolutions)
        {
            float timelimit = (float)maxtime;
            float t = Time.realtimeSinceStartup;
            SortList<Board> generatedPositions = new SortList<Board>();
            generatedPositions.Add(root.board);
            Queue<Node> q = new Queue<Node>();
            q.Enqueue(root);
            Node current = null;
            while (q.Count > 0)
            {
                current = q.Dequeue();
                //Debug.Log("current: " + current.board.PlayerPos() + " " + current.dir.name + " " + "q_size: " + q.Count);
                //PrintCurrent(current);
                for (int di = 0; di < directions.Length; di++)
                {
                    Board nb = NewPosition(current.board, directions[di]);
                    if (nb == null)
                        continue;

                    if (!generatedPositions.Contains(nb))
                    {
                        Node n = new Node(nb, directions[di], current);
                        if (nb.Won())
                        {
                            AddSolution(n);
                            if (Solutions.Count >= maxsolutions)
                            {
                                //Debug.Log("Simulation done in " + (Time.realtimeSinceStartup - t) + " seconds");
                                return;
                            }
                        }
                        else
                        {
                            generatedPositions.Add(nb);

                            q.Enqueue(n);
                            //Debug.Log("enqueue: " + n.board.PlayerPos() + " " + n.dir.name);
                            //PrintCurrent(n);
                            float d = (Time.realtimeSinceStartup - t);
                            if (d > timelimit)
                            {
                                //Debug.Log("reached search time limit, stopping");
                                //Debug.Log("done in " + (Time.realtimeSinceStartup - t) + " seconds");
                                return;
                            }
                        }
                    }
                }
            }
            //Debug.Log("Simulation done in " + (Time.realtimeSinceStartup - t) + " seconds");
        }

        private void AddSolution(Node n)
        {
            List<MoveDir> dirs = new List<MoveDir>();
            Node head = n;
            while (head != null)
            {
                dirs.Add(head.dir);
                head = head.parent;
            }
            dirs.Reverse();
            Solutions.Add(new Solution(dirs.Count - 1, dirs));
        }

        private void PrintCurrent(Node n)
        {
            List<MoveDir> dirs = new List<MoveDir>();
            Node head = n;
            while (head != null)
            {
                dirs.Add(head.dir);
                head = head.parent;
            }
            dirs.Reverse();

            string description = "";
            for (int i = 1; i < dirs.Count; i++)
            {
                if (i > 1)
                    description += "-";

                description += dirs[i].name;
            }
            Debug.Log("current: " + description);
        }

        public void RunSolver(float maxtime = 10, int maxsolutions = 1)
        {
            Node root = new Node(new Board(startingBoard), new MoveDir("START", 0, 0, 0), null);
            Solutions = new List<Solution>();

            BreadthFirstSearch(root, maxtime, maxsolutions);

            //if (Solutions.Count == 0)
            //{
            //    Debug.Log("no solution found");
            //}
            //else
            //{
            //    Debug.Log("found " + Solutions.Count + " solutions");
            //    foreach (LevelSolution s in Solutions)
            //        Debug.Log(" " + s.NumSteps + " moves");
            //}

            //if (Solutions.Count > 0)
            //{
            //    Solution s = Solutions[0];
            //    Board b = new Board(startingBoard);
            //    List<Board> history = new List<Board>();
            //    for (int i = 1; i < s.steplist.Count; i++)
            //    {
            //        history.Add(new Board(b));
            //        string pos = string.Format("Step {0}, Positions: ", i);
            //        foreach (Entity p in b.players)
            //            pos += string.Format(" {0},{1} ", p.pos.r, p.pos.c);
            //        Debug.Log(pos);
            //        MoveDir dir = s.steplist[i];
            //        Debug.Log(string.Format("Move {0}", dir.name));
            //        b.Move(dir.dr, dir.dc);
            //    }
            //}
        }

        public void CleanUnused()
        {
            if (Solutions == null || Solutions.Count == 0)
                return;

            Solution s = Solutions[0];
            Board b = new Board(startingBoard);
            b.InitUnusedTables();
            for (int i = 1; i < s.steplist.Count; i++)
            {
                MoveDir dir = s.steplist[i];
                b.Move(dir.dr, dir.dc);
            }

            cleanedBoard = new Board(startingBoard);

            for (int r = 0; r < b.rows; r++)
                for (int c = 0; c < b.cols; c++)
                    if (!b.usedGround[r, c])
                        cleanedBoard.grid[r, c] = false;

            //for (int i = 0; i < b.entities.Count; i++)
            //{
            //    if (b.usedEntities[i])
            //        Debug.Log("Used " + b.entities[i].type + " at " + b.entities[i].pos.r + ", " + b.entities[i].pos.c);
            //    else
            //        Debug.Log("Not used " + b.entities[i].type + " at " + b.entities[i].pos.r + ", " + b.entities[i].pos.c);
            //}

            //for (int c = b.cols - 1; c >= 0; c--)
            //{
            //    string line = "";
            //    for (int r = 0; r < b.rows; r++)
            //    {
            //        if (!b.grid[r, c])
            //            line += " ";
            //        else if (!b.usedGround[r, c])
            //            line += "X";
            //        else
            //            line += "O";
            //    }
            //    Debug.Log(line);
            //}

            List<Entity> toremove = new List<Entity>();
            for (int i = 0; i < b.entities.Count; i++)
                if (!b.usedEntities[i])
                    toremove.Add(cleanedBoard.entities[i]);

            foreach (Entity e in toremove)
            {
                cleanedBoard.entities.RemoveAt(e.index);
                foreach (Entity other in cleanedBoard.entities)
                {
                    for (int i = other.targets.Count - 1; i >= 0; i--)
                        if (other.targets[i] == e.index)
                            other.targets.Remove(e.index);
                    for (int i = 0; i < other.targets.Count; i++)
                        if (other.targets[i] > e.index)
                            other.targets[i]--;
                }
                for (int i = e.index; i < cleanedBoard.entities.Count; i++)
                    cleanedBoard.entities[i].index--;
            }
        }

        public void ConvertCleanedBoardToLevel(LevelRoot root)
        {
            cleanedBoard.ConvertToLevel(root);
        }

        public int c_dist_variation;
        public int c_local_dist;
        public int c_switches;
        public int c_moves;

        public float GetFirstSolutionScore()
        {
            if (Solutions == null || Solutions.Count == 0)
                return -1;

            c_dist_variation = 0;
            c_local_dist = 0;
            c_moves = 0;
            c_switches = 0;

            float score = 1;

            Solution s = Solutions[0];
            Board b = new Board(startingBoard);
            b.InitUnusedTables();
            MoveDir olddir = s.steplist[0];
            for (int i = 1; i < s.steplist.Count; i++)
            {
                Board a = new Board(b);
                MoveDir dir = s.steplist[i];
                b.Move(dir.dr, dir.dc);

                int ar = a.players[1].pos.r - a.players[0].pos.r;
                int ac = a.players[1].pos.c - a.players[0].pos.c;
                int br = b.players[1].pos.r - b.players[0].pos.r;
                int bc = b.players[1].pos.c - b.players[0].pos.c;

                int dr = Mathf.Abs(br - ar);
                int dc = Mathf.Abs(bc - ac);

                c_dist_variation += (dr + dc);

                c_local_dist += (Mathf.Abs(br) + Mathf.Abs(bc));

                if (i > 1)
                {
                    if (dir.name != olddir.name)
                        c_switches++;
                }

                olddir = dir;

                c_moves++;
            }
            score += 0.1511f * c_dist_variation;
            score += 0.1848f * c_local_dist;
            score += 0.2005f * c_switches;
            score += 0.0220f * c_moves;

            return score * 1f;
        }

        public Vector2 GetBoardSize()
        {
            return new Vector2(startingBoard.rows, startingBoard.cols);
        }

        public string GetBoardSizeString()
        {
            return string.Format("{0}x{1}", startingBoard.rows, startingBoard.cols);
        }
    }
}