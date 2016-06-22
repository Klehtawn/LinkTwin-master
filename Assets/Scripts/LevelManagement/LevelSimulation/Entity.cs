using System;
using System.Collections.Generic;
using UnityEngine;

namespace LevelSimulation
{
    public class Entity : IComparable<Entity>
    {
        public Block.BlockType type;
        public GridPos pos;
        public bool navigable;
        public bool on;
        public List<int> targets;
        public int index;
        public int delayedToggles;

        public System.Action delayedAction;

        public Entity(int index, Block.BlockType type, GridPos pos, bool navigable, bool on)
        {
            this.index = index;
            this.type = type;
            this.pos = pos;
            this.navigable = navigable;
            this.on = on;
            targets = new List<int>(0);
            delayedToggles = 0;
        }

        public Entity(Entity other)
        {
            index = other.index;
            type = other.type;
            pos = new GridPos(other.pos);
            navigable = other.navigable;
            on = other.on;
            delayedToggles = other.delayedToggles;
            targets = new List<int>(other.targets.Count);
            for (int i = 0; i < other.targets.Count; i++)
            {
                targets.Add(other.targets[i]);
            }
        }

        public bool Equals(Entity other)
        {
            if (type != other.type || pos != other.pos || navigable != other.navigable || on != other.on)
                return false;

            //if (targets.Count != other.targets.Count)
            //    return false;

            //for (int i = 0; i < targets.Count; i++)
            //    if (targets[i] != other.targets[i])
            //        return false;

            return true;
        }

        public int CompareTo(Entity other)
        {
            if (type != other.type)
                return (int)type - (int)other.type;

            if (pos != other.pos)
            {
                if (pos.r != other.pos.r)
                    return pos.r - other.pos.r;
                else
                    return pos.c - other.pos.c;
            }


            if (navigable != other.navigable)
                return navigable ? 1 : -1;

            if (on != other.on)
                return on ? 1 : -1;

            return 0;
        }

    }
}