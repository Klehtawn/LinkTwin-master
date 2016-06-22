using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelSimulation
{
    public class Board : IComparable<Board>
    {
        public int rows;
        public int cols;
        public bool[,] grid;
        public List<Entity> entities;
        public List<Entity> players;
        public List<Entity> exits;

        public bool[,] usedGround;
        public bool[] usedEntities;
        public bool recordUsage = false;

        private Board() { }

        public Board(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            grid = new bool[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = false;
                }
            entities = new List<Entity>();
            players = new List<Entity>();
            exits = new List<Entity>();
        }

        public Board(Board other)
        {
            this.rows = other.rows;
            this.cols = other.cols;
            grid = new bool[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = other.grid[r, c];
                }

            entities = new List<Entity>(other.entities.Count);
            players = new List<Entity>(other.players.Count);
            exits = new List<Entity>(other.exits.Count);

            for (int i = 0; i < other.entities.Count; i++)
                entities.Add(new Entity(other.entities[i]));
            for (int i = 0; i < other.players.Count; i++)
                players.Add(new Entity(other.players[i]));
            foreach (Entity e in entities)
            {
                if (e.type == Block.BlockType.Finish)
                    exits.Add(e);
            }
            //SearchSpecialEntities();
        }

        public Board(LevelRoot root)
        {
            InitFromLevel(root);
        }

        public void Initialize(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            grid = new bool[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    grid[r, c] = false;
                }
            entities = new List<Entity>();
            players = new List<Entity>();
            exits = new List<Entity>();
        }

        public List<Entity> EntitiesAt(GridPos pos)
        {
            List<Entity> results = new List<Entity>();
            foreach (Entity e in entities)
                if (e.pos == pos)
                    results.Add(e);

            foreach (Entity p in players)
                if (p.pos == pos)
                    results.Add(p);

            return results;
        }

        public bool Equals(Board other)
        {
            //if (rows != other.rows || cols != other.cols)
            //    return false;

            //for (int r = 0; r < rows; r++)
            //    for (int c = 0; c < cols; c++)
            //        if (grid[r, c] != other.grid[r, c])
            //            return false;

            //if (players.Count != other.players.Count && exits.Count != other.exits.Count)
            //    return false;

            for (int i = 0; i < players.Count; i++)
                if (players[i].pos != other.players[i].pos)
                    return false;

            if (entities.Count != other.entities.Count)
                return false;

            for (int i = 0; i < entities.Count; i++)
            {
                if (!entities[i].Equals(other.entities[i]))
                    return false;
            }
            //for (int r = 0; r < rows; r++)
            //    for (int c = 0; c < cols; c++)
            //    {
            //        List<Entity> myentities = EntitiesAt(new GridPos(r, c));
            //        List<Entity> otherentities = other.EntitiesAt(new GridPos(r, c));
            //        if (myentities.Count != otherentities.Count)
            //            return false;

            //        for (int i = 0; i < myentities.Count; i++)
            //        {
            //            Entity e = myentities[i];
            //            bool found = false;
            //            for (int j = 0; j < otherentities.Count; j++)
            //            {
            //                if (otherentities[j].Equals(e))
            //                {
            //                    found = true;
            //                    break;
            //                }
            //            }

            //            if (!found)
            //                return false;
            //        }
            //    }

            return true;
        }

        public void SearchSpecialEntities()
        {
            players.Clear();
            exits.Clear();
            foreach (Entity e in entities)
            {
                if (e.type == Block.BlockType.Spawn)
                    players.Add(new Entity(-1, Block.BlockType.Player, e.pos, e.navigable, e.on));
                if (e.type == Block.BlockType.Finish)
                    exits.Add(e);
            }
        }

        public enum MoveResult
        {
            Moved,
            Blocked,
            Dead
        }

        public MoveResult CanEntityMove(Entity player, int dr, int dc, bool markUsage = false)
        {
            int r = player.pos.r + dr;
            int c = player.pos.c + dc;

            if (r >= rows || c >= cols || r < 0 || c < 0)
                return MoveResult.Dead;

            GridPos pos = new GridPos(r, c);

            List<Entity> elist = EntitiesAt(pos);
            foreach (Entity e in elist)
            {
                if ((e.navigable == false && e.on == true) ||
                    (e.type == Block.BlockType.Player && CanEntityMove(e, dr, dc, markUsage) == MoveResult.Blocked) ||
                    (e.type == Block.BlockType.MovableBox && CanEntityMove(e, dr, dc, markUsage) == MoveResult.Blocked))
                {
                    if (markUsage)
                    {
                        usedEntities[e.index] = true;
                        usedGround[r, c] = true;
                    }
                    return MoveResult.Blocked;
                }

                //if (e.type == Block.BlockType.MovableBox && CanEntityMove(e, dr, dc) == MoveResult.Dead)
                //    return MoveResult.Dead;

                if (e.type == Block.BlockType.Portal)
                {
                    Entity target = entities[e.targets[0]];
                    int rp = target.pos.r + dr;
                    int cp = target.pos.c + dc;

                    if (rp >= rows || cp >= cols || rp < 0 || cp < 0)
                        return MoveResult.Dead;

                    if (grid[rp, cp] == false)
                        return MoveResult.Dead;
                }
            }

            if (grid[r, c] == false)
                return MoveResult.Dead;

            return MoveResult.Moved;
        }

        public bool CanMove(int dr, int dc)
        {
            bool atLeastOneMoved = false;
            foreach (Entity p in players)
            {
                MoveResult result = CanEntityMove(p, dr, dc);
                if (result == MoveResult.Dead)
                    return false;
                if (result == MoveResult.Moved)
                    atLeastOneMoved = true;
            }

            return atLeastOneMoved;
        }

        public void MoveEntity(Entity p, int dr, int dc)
        {
            MoveResult result = CanEntityMove(p, dr, dc, recordUsage);

            if (p.type == Block.BlockType.Player && result != MoveResult.Moved)
                return;

            if (p.type == Block.BlockType.MovableBox && result == MoveResult.Blocked)
                return;

            // --- update entities in old position
            List<Entity> localEntities = EntitiesAt(p.pos);
            foreach (Entity e in localEntities)
            {
                if (e == p)
                    continue;
                if (e.type == Block.BlockType.PushButton)
                {
                    //OnButtonLeft(e);
                    Entity pushbutton = e;
                    e.delayedAction += () => OnButtonLeft(pushbutton);
                }
            }

            // --- update position
            p.pos = new GridPos(p.pos.r + dr, p.pos.c + dc);

            // --- mark this entity and ground tile as used
            if (recordUsage)
            {
                if (p.index != -1)
                    usedEntities[p.index] = true;
                if (p.pos.r >= 0 && p.pos.r < rows && p.pos.c >=0 && p.pos.c < cols)
                    usedGround[p.pos.r, p.pos.c] = true;
            }

            // --- check for dead, stop here if so
            if (p.type == Block.BlockType.MovableBox && result == MoveResult.Dead)
                return;

            // --- update entities in new position
            localEntities = EntitiesAt(p.pos);
            foreach (Entity e in localEntities)
            {
                if (e == p)
                    continue;
                if (e.type == Block.BlockType.Button || e.type == Block.BlockType.PushButton)
                {
                    //OnButtonEntered(e);
                    Entity button = e;
                    e.delayedAction += () => OnButtonEntered(button);
                }
                if (e.type == Block.BlockType.Portal)
                {
                    Entity _p = p;
                    Entity _e = e;
                    p.delayedAction += () => OnEntityEnteredPortal(_p, _e, dr, dc);
                    //OnEntityEnteredPortal(p, e, dr, dc);
                }
                if (e.type == Block.BlockType.MovableBox)
                {
                    MoveEntity(e, dr, dc);
                }
            }
        }

        private bool CanButtonChangeState(Entity button)
        {
            for (int i = 0; i < button.targets.Count; i++)
            {
                Entity target = entities[button.targets[i]];
                List<Entity> tentities = EntitiesAt(target.pos);
                if (target.type != Block.BlockType.Finish)
                {
                    foreach (Entity t in tentities)
                        if (t.type == Block.BlockType.MovableBox || t.type == Block.BlockType.Player)
                            return false;
                }
            }
            return true;
        }

        private void OnButtonEntered(Entity button)
        {
            if (CanButtonChangeState(button))
            {
                ToggleButtonState(button);
            }
            else if (button.type == Block.BlockType.PushButton)
            {
                button.delayedToggles++;
            }
        }

        private void ToggleButtonState(Entity button)
        {
            button.on = !button.on;
            //Debug.Log("new button state for " + button.type + ": " + button.on);
            foreach (int i in button.targets)
            {
                Entity target = entities[i];
                target.on = button.on;
            }
        }

        private void OnButtonLeft(Entity button)
        {
            if (CanButtonChangeState(button))
            {
                button.on = !button.on;
                foreach (int i in button.targets)
                {
                    Entity target = entities[i];
                    target.on = button.on;
                }
            }
            else
            {
                button.delayedToggles++;
            }
        }

        private void OnEntityEnteredPortal(Entity p, Entity portal, int dr, int dc)
        {
            Entity target = entities[portal.targets[0]];

            if (recordUsage)
            {
                usedEntities[portal.index] = true;
                usedEntities[target.index] = true;
            }

            p.pos.r = target.pos.r + dr;
            p.pos.c = target.pos.c + dc;

            List<Entity> afterPortalEntities = EntitiesAt(p.pos);
            //Debug.Log("entered portal, dir " + dr + "," + dc + ", entity count: " + afterPortalEntities.Count);
            bool blocked = false;
            foreach (Entity ape in afterPortalEntities)
            {
                if (ape == p)
                    continue;

                if (ape.navigable == false && ape.on == true)
                    blocked = true;

                if (ape.type == Block.BlockType.MovableBox || ape.type == Block.BlockType.Player)
                {
                    //Debug.Log("pushing box");
                    MoveEntity(ape, dr, dc);
                }

                if (ape.type == Block.BlockType.Button || ape.type == Block.BlockType.PushButton)
                {
                    ape.on = !ape.on;
                    //Debug.Log("new button state: " + ape.on);
                    foreach (int i in ape.targets)
                    {
                        Entity apetarget = entities[i];
                        apetarget.on = ape.on;
                    }
                }
                if (ape.type == Block.BlockType.Portal)
                {
                    OnEntityEnteredPortal(p, ape, dr, dc);
                }
            }

            if (blocked)
            {
                //Debug.Log("blocked by portal, reflect back");
                p.delayedAction = () => MoveEntity(p, -dr, -dc);
                //MoveEntity(p, -dr, -dc);
            }
        }

        public void Move(int dr, int dc)
        {
            // --- move all player objects
            foreach (Entity p in players)
                MoveEntity(p, dr, dc);

            // --- search for stuff that needs to die
            //List<Entity> toRemove = new List<Entity>();
            foreach (Entity e in entities)
                if (e.type == Block.BlockType.MovableBox &&
                    (e.pos.r < 0 || e.pos.c < 0 || e.pos.r >= rows || e.pos.c >= cols || grid[e.pos.r, e.pos.c] == false))
                {
                    e.pos.r = 0;
                    e.pos.c = 0;
                    e.type = Block.BlockType.Undefined;
                    e.on = false;
                    e.navigable = true;
                }

            //foreach (Entity e in toRemove)
            //    entities.Remove(e);

            bool delayedActionFound = false;

            do
            {
                delayedActionFound = false;

                // --- execute delayed player actions
                foreach (Entity p in players)
                    if (p.delayedAction != null)
                    {
                        //Debug.Log("after move delayed player");
                        Action a = p.delayedAction;
                        //p.delayedAction();
                        p.delayedAction = null;
                        a();
                        delayedActionFound = true;
                    }

                // --- execute delayed actions on entities
                foreach (Entity e in entities)
                    if (e.delayedAction != null)
                    {
                        //Debug.Log("after move delayed other: " + e.type + ", " + e.delayedAction);
                        Action a = e.delayedAction;
                        //e.delayedAction();
                        e.delayedAction = null;
                        a();
                        //e.delayedToggle = false;
                        delayedActionFound = true;
                    }
            }
            while (delayedActionFound);

            // --- button delayed toggles execute after everything else
            foreach (Entity e in entities)
            {
                if ((e.type == Block.BlockType.Button || e.type == Block.BlockType.PushButton) && e.delayedToggles > 0)
                {
                    if (CanButtonChangeState(e))
                    {
                        e.delayedToggles = e.delayedToggles % 2;
                        while (e.delayedToggles > 0)
                        {
                            ToggleButtonState(e);
                            e.delayedToggles--;
                        }
                    }
                }

            }
        }

        public bool Won()
        {
            foreach (Entity exit in exits)
            {
                if (!exit.on)
                    return false;
                bool found = false;
                List<Entity> stuff = EntitiesAt(exit.pos);
                foreach (Entity e in stuff)
                    if (e.type == Block.BlockType.Player)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    return false;
            }
            return true;
        }

        public string PlayerPos()
        {
            string res = "(";
            foreach (Entity p in players)
                res += p.pos.r + "," + p.pos.c + " ";
            res += ")";
            return res;
        }

        public int CompareTo(Board other)
        {
            for (int i = 0; i < players.Count; i++)
                if (players[i].pos != other.players[i].pos)
                {
                    if (players[i].pos.r != other.players[i].pos.r)
                        return players[i].pos.r - other.players[i].pos.r;
                    else
                        return players[i].pos.c - other.players[i].pos.c;
                }


            if (entities.Count != other.entities.Count)
                return entities.Count - other.entities.Count;

            for (int i = 0; i < entities.Count; i++)
            {
                int result = entities[i].CompareTo(other.entities[i]);
                if (result != 0)
                    return result;
            }
            return 0;
        }

        public void InitUnusedTables()
        {
            usedEntities = new bool[entities.Count];
            usedGround = new bool[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    usedGround[r, c] = false;
            for (int i = 0; i < entities.Count; i++)
            {
                Entity e = entities[i];
                // --- spawns and exits are always used, ground below them too
                if (e.type == Block.BlockType.Spawn || e.type == Block.BlockType.Finish)
                {
                    usedEntities[i] = true;
                    usedGround[e.pos.r, e.pos.c] = true;
                }
                else
                {
                    usedEntities[i] = false;
                }
            }
            recordUsage = true;
        }

        public Vector2 GetBlockPosition(Block b)
        {
            Vector3 pos = Vector3.one * 0.5f + b.transform.position / (GameSession.gridUnit);
            pos.y = pos.z;
            return pos;
        }

        public void InitFromLevel(LevelRoot root)
        {
            //Debug.Log("Creating LevelSimulation...");
            TableDescription td = null;
            TableLoadSave.ConvertSceneToMapDescription(ref td, root.transform);

            root.blocks.Fill(root.gameObject);

            //List<Entity> blockList = new List<Entity>(root.blocks.items.Count);
            //Debug.Log("processing " + root.blocks.items.Count + " blocks");
            int minx = int.MaxValue;
            int miny = int.MaxValue;
            int maxx = int.MinValue;
            int maxy = int.MinValue;
            foreach (Block b in root.blocks.ground)
            {
                Vector2 pos = GetBlockPosition(b);
                minx = Mathf.Min(minx, (int)pos.x);
                miny = Mathf.Min(miny, (int)pos.y);
                maxx = Mathf.Max(maxx, (int)pos.x);
                maxy = Mathf.Max(maxy, (int)pos.y);
            }
            //Debug.Log("ground bounding rect at " + minx + ", " + miny + ", " + maxx + ", " + maxy);
            foreach (Block b in root.blocks.items)
            {
                Vector2 pos = GetBlockPosition(b);
                minx = Mathf.Min(minx, (int)pos.x);
                miny = Mathf.Min(miny, (int)pos.y);
                maxx = Mathf.Max(maxx, (int)pos.x);
                maxy = Mathf.Max(maxy, (int)pos.y);
            }
            //Debug.Log("full bounding rect at " + minx + ", " + miny + ", " + maxx + ", " + maxy);

            int ox = -minx;
            int oy = -miny;
            int rows = maxx - minx + 1;
            int cols = maxy - miny + 1;

            //Debug.Log("board is " + rows + " x " + cols + " at offset " + ox + ", " + oy);
            Initialize(rows, cols);
            foreach (Block b in root.blocks.ground)
            {
                Vector2 pos = GetBlockPosition(b);
                int r = ox + (int)pos.x;
                int c = oy + (int)pos.y;
                grid[r, c] = true;
            }
            for (int i = 0; i < root.blocks.items.Count; i++)
            {
                Block b = root.blocks.items[i];
                Vector2 pos = GetBlockPosition(b);
                GridPos gp = new GridPos(ox + (int)pos.x, oy + (int)pos.y);
                //Debug.Log("block " + i + " type: " + b.blockType + " nav: " + b.navigable + " on: " + b.IsValidBlock() + " at: " + gp.r + ", " + gp.c);
                entities.Add(new Entity(i, b.blockType, gp, b.navigable, b.IsValidBlock()));
            }
            for (int i = 0; i < root.blocks.items.Count; i++)
            {
                Entity e = entities[i];
                Block b = root.blocks.items[i];
                if (b.blockType == Block.BlockType.Button || b.blockType == Block.BlockType.PushButton)
                {
                    TriggerBlock button = b as TriggerBlock;
                    foreach (Block t in button.targets)
                    {
                        int tindex = root.blocks.items.IndexOf(t);
                        //Debug.Log("button(" + i + ") target " + tindex);
                        e.targets.Add(tindex);
                        Entity target = entities[tindex];
                        target.on = e.on;
                        if (target.type == Block.BlockType.Door)
                            target.navigable = !target.on;
                    }
                }
                if (b.blockType == Block.BlockType.Portal)
                {
                    Portal portal = b as Portal;
                    int tindex = root.blocks.items.IndexOf(portal.target);
                    //Debug.Log("portal(" + i + ") target " + tindex);
                    e.targets.Add(tindex);
                }
            }

            SearchSpecialEntities();
            //Debug.Log("Simulation loaded, " + startingBoard.players.Count + " spawns, " + startingBoard.exits.Count + " exits");
        }

        public void ConvertToLevel(LevelRoot root)
        {
            GameObject groundBlock = Resources.Load("GroundBlock") as GameObject;
            Transform groundRoot = root.Ground.transform;
            Transform tableRoot = root.Table.transform;

            float ox = -GameSession.gridUnit * (rows / 2);
            float oy = -GameSession.gridUnit * (cols / 2);
            Vector3 offset = new Vector3(ox, 0.0f, oy);

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    if (grid[r, c])
                    {
#if UNITY_EDITOR
                        GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(groundBlock) as GameObject;
#else
                        GameObject obj = GameObject.Instantiate(groundBlock);
#endif
                        obj.transform.SetParent(groundRoot);
                        obj.transform.localScale = Vector3.one;
                        obj.transform.localPosition = offset + new Vector3(GameSession.gridUnit * (0.5f + (float)r), -5.0f, GameSession.gridUnit * (0.5f + (float)c));
                    }
                }

            List<GameObject> objects = new List<GameObject>(entities.Count);

            foreach (Entity e in entities)
            {
                GameObject obj = TableLoadSave.InstantiatePrefabByType(e.type);
                obj.transform.SetParent(tableRoot);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = offset + new Vector3(GameSession.gridUnit * (0.5f + (float)e.pos.r), 5.0f, GameSession.gridUnit * (0.5f + (float)e.pos.c));
                objects.Add(obj);
            }

            foreach (Entity e in entities)
            {
                GameObject obj = objects[e.index];
                switch (e.type)
                {
                    case Block.BlockType.Portal:
                        Portal portal = obj.GetComponent<Portal>();
                        //Debug.Log("portal " + e.index + " with target index " + e.targets[0] + " of " + entities.Count);
                        GameObject t = objects[e.targets[0]];
                        Portal target = t.GetComponent<Portal>();
                        portal.target = target;
                        break;
                }
            }
        }
    }
}