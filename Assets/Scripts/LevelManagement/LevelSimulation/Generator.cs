using UnityEngine;
using System.Collections.Generic;

namespace LevelSimulation
{
    public class Generator
    {
#if UNITY_EDITOR
        private Board board;
        private List<GridPos> players;
        //private List<GridPos> randPositions = new List<GridPos>(256);
        private const int NUM_PLAYERS = 2;

        private bool Available(GridPos pos)
        {
            foreach (Entity e in board.entities)
                if (e.pos.Equals(pos))
                    return false;

            return true;
        }

        private GridPos RandPos()
        {
            GridPos pos;
            do
            {
                pos = new GridPos(Random.Range(0, board.rows), Random.Range(0, board.cols));
            }
            while (!Available(pos));
            return pos;
        }

        public void Generate(
            int rows,
            int cols,
            int holesPercent,
            int boxesPercent
            )
        {
            UnityEngine.Random.seed = (int)Time.realtimeSinceStartup;

            board = new Board(rows, cols);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    board.grid[r, c] = true;

            int numTiles = rows * cols;
            int numHoles = numTiles * holesPercent / 100;
            int numBoxes = numTiles * boxesPercent / 100;

            for (int i = 0; i < numHoles; i++)
            {
                GridPos pos = RandPos();
                board.grid[pos.r, pos.c] = false;
            }

            for (int i = 0; i < numBoxes; i++)
            {
                GridPos pos = RandPos();
                board.entities.Add(new Entity(board.entities.Count, Block.BlockType.Default, pos, false, true));
                board.grid[pos.r, pos.c] = true;
            }

            //for (int i = 0; i < 3; i++)
            //{
            //    GridPos pos = RandPos();
            //    board.entities.Add(new Entity(board.entities.Count, Block.BlockType.MovableBox, pos, false, true));
            //}
            //{
            //    GridPos pos = RandPos();
            //    board.entities.Add(new Entity(board.entities.Count, Block.BlockType.Portal, pos, false, true));
            //}
            //{
            //    GridPos pos = RandPos();
            //    board.entities.Add(new Entity(board.entities.Count, Block.BlockType.Portal, pos, false, true));
            //}
            //board.entities[board.entities.Count - 1].targets.Add(board.entities.Count - 2);
            //board.entities[board.entities.Count - 2].targets.Add(board.entities.Count - 1);

            players = new List<GridPos>(NUM_PLAYERS);
            for (int i = 0; i < NUM_PLAYERS; i++)
            {
                GridPos pos = RandPos();
                players.Add(pos);
                board.entities.Add(new Entity(board.entities.Count, Block.BlockType.Spawn, pos, true, true));
                board.grid[pos.r, pos.c] = true;
            }

            for (int i = 0; i < NUM_PLAYERS; i++)
            {
                GridPos pos = RandPos();
                board.entities.Add(new Entity(board.entities.Count, Block.BlockType.Finish, pos, true, true));
                board.grid[pos.r, pos.c] = true;
            }
            //board.grid[1, 2] = false;
            //board.grid[2, 2] = false;
            //board.grid[2, 3] = false;
        }

        public Vector2 GetBlockPosition(Block b)
        {
            Vector3 pos = Vector3.one * 0.5f + b.transform.position / (GameSession.gridUnit);
            pos.y = pos.z;
            if (pos.x != Mathf.Floor(pos.x) || pos.z != Mathf.Floor(pos.z))
                Debug.LogWarning("non-snapped position for block " + b);
            return pos;
        }

        public Board GetBoard()
        {
            return board;
        }

        public void ConvertToLevel(LevelRoot root)
        {
            board.ConvertToLevel(root);
            //GameObject groundBlock = Resources.Load("GroundBlock") as GameObject;
            //Transform groundRoot = root.Ground.transform;
            //Transform tableRoot = root.Table.transform;

            //Vector3 offset = new Vector3(-GameSession.gridUnit * board.rows * 0.5f, 0.0f, -GameSession.gridUnit * board.cols * 0.5f);

            //for (int r = 0; r < board.rows; r++)
            //    for (int c = 0; c < board.cols; c++)
            //    {
            //        if (board.grid[r, c])
            //        {
            //            GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(groundBlock) as GameObject;
            //            obj.transform.SetParent(groundRoot);
            //            obj.transform.localScale = Vector3.one;
            //            obj.transform.localPosition = offset + new Vector3(GameSession.gridUnit * (0.5f + (float)r), -5.0f, GameSession.gridUnit * (0.5f + (float)c));
            //        }
            //    }

            //foreach (Entity e in board.entities)
            //{
            //    GameObject obj = TableLoadSave.InstantiatePrefabByType(e.type);
            //    obj.transform.SetParent(tableRoot);
            //    obj.transform.localScale = Vector3.one;
            //    obj.transform.localPosition = offset + new Vector3(GameSession.gridUnit * (0.5f + (float)e.pos.r), 5.0f, GameSession.gridUnit * (0.5f + (float)e.pos.c));
            //}
        }
#endif
    }
}