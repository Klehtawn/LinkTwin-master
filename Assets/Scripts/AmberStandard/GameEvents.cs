using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameEvents
{
    public enum Sender
    {
        None,
        Boy,
        Girl,
        Bot,
        BoyAndGirl,
        BoyOrGirl,
        System
    }
    public enum EventType
    {
        None,
        Moving,
        Fall,
        Spawned,
        ReachedFinish,
        HitBox,
        PushingBox,
        ButtonPress,
        EnterPortal,
        BoardButtonPressed,
        MainMenuEntered,
        GameplayEntered,
        ChapterSelectEntered,
        UndoTick,
        CharacterIdle,
        LevelCompleted,
        RewardOneStar,
        RewardTwoStars,
        RewardThreeStars,
        PowerupUsed,
        ChapterUnlock,
        ConfirmationPress
    }

    [Serializable]
    public class Event
    {
        public Sender sender = Sender.None;
        public EventType type = EventType.None;
        public string args;

        [NonSerialized]
        public int hitCount = 0;

        public Event(Sender sender, EventType type, string args = null)
        {
            this.sender = sender;
            this.type = type;
            this.args = args;
        }

        public bool Equals(Event other)
        {
            int argsLen = args != null ? args.Length : 0;
            int otherArgsLen = other.args != null ? other.args.Length : 0;
            if (argsLen != otherArgsLen) return false;

            if (type != other.type) return false;

            if(argsLen > 0)
            {
                if (args != other.args)
                    return false;
            }

            if (other.sender == Sender.BoyOrGirl && (sender == Sender.Boy || sender == Sender.Girl))
                return true;

            if (sender == Sender.BoyOrGirl && (other.sender == Sender.Boy || other.sender == Sender.Girl))
                return true;
           

            return sender == other.sender;
        }
    }

    public static List<Event> events = new List<Event>();

    public static Action<Event> OnGameEventTriggered;

    public static void Send(EventType type)
    {
        Send(Sender.System, type);
    }

    public static void Send(Sender who, EventType type, string args = null)
    {
        Debug.Log("Event [" + type.ToString() + "] sent by [" + who.ToString() + "]");
        Event e = new Event(who, type, args);

        bool added = false;
        foreach(Event ee in events)
        {
            if(ee.Equals(e))
            {
                ee.hitCount++;
                e = ee;
                added = true;
                break;
            }
        }

        if (!added)
        {
            e.hitCount = 1;
            events.Add(e);
        }

        if (OnGameEventTriggered != null)
            OnGameEventTriggered(e);
    }

    public static void Send(Player.Kind who, EventType type, string args = null)
    {
        Send(who == Player.Kind.Boy ? Sender.Boy : Sender.Girl, type, args);
    }

    public static void ClearEvents()
    {
        events.Clear();
    }

    public static void Decorate(Sender who, EventType type, string args, Action action)
    {

    }
}
