using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

[SelectionBase]
public class NarrationBlock : Block {

    [Serializable]
    public class EventDef
    {
        public GameEvents.Event Event;
        public int hitCount = -1;
        public TextAsset narration;

        public EventDef()
        {
            hitCount = -1;
        }
    }

    class DialogueLine
    {
        public float moment;
        public float duration;
        public string text;
        public Player.Kind actor;
    };

    List<DialogueLine> dialogueLines = new List<DialogueLine>();
    List<TextBubble> textBubbles = new List<TextBubble>();

    float dialogueTimer = 0.0f;

    public GameObject narrationBox;

    public EventDef[] events = null;

    public override void Awake()
    {
        base.Awake();
        blockType = BlockType.Narration;
        ignoreOnGamePlay = true;
    }

	public override void Start () {
        base.Start();

        GameEvents.OnGameEventTriggered += OnGameEvent;

        //ShowMessage(2.0f, 3.0f, "boy: Hello !");
        //ShowMessage(3.5f, 3.0f, "girl: Is it me you looking for ?");
	}

    protected override void OnDestroy()
    {
        base.OnDestroy();

        GameEvents.OnGameEventTriggered -= OnGameEvent;
    }
	
	// Update is called once per frame
	public override void Update () {
        base.Update();

        dialogueTimer += Time.deltaTime;

        if (dialogueLines.Count > 0)
        {
            if(dialogueTimer >= dialogueLines[0].moment)
            {
                DialogueLine dl = dialogueLines[0];
                ShowTextBubble(dl.duration, dl.actor, dl.text);
                dialogueLines.RemoveAt(0);
            }
        }

        for (int i = 0; i < textBubbles.Count; i++)
        {
            if (textBubbles[i] == null)
            {
                textBubbles.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < textBubbles.Count; i++)
        {
            TextBubble tb = textBubbles[i];
            WorldPositionToDesktopPosition(tb.objectFollowed, tb.GetComponent<RectTransform>());
        }
	}

    public override void WriteSignificantInfo(ref System.IO.BinaryWriter bw)
    {
        base.WriteSignificantInfo(ref bw);

        WriteSignificantValue(ref bw, "eventsCount", (byte)events.Length);

        for(int i = 0; i < events.Length; i++)
        {
            GameEvents.Event ev = events[i].Event;
            string istr = i.ToString();
            WriteSignificantValue(ref bw, "sender" + istr, (byte)ev.sender);
            WriteSignificantValue(ref bw, "type" + istr, (byte)ev.type);
            WriteSignificantValue(ref bw, "args" + istr, ev.args);
            WriteSignificantValue(ref bw, "hitcount" + istr, events[i].hitCount);

            string path = events[i].narration.name;
#if UNITY_EDITOR
            path = AssetDatabase.GetAssetPath(events[i].narration);
            if (path.StartsWith("Assets/"))
                path = path.Remove(0, 7);

            if (path.StartsWith("Resources/"))
                path = path.Remove(0, 10);

            if (path.EndsWith(".txt"))
                path = path.Remove(path.IndexOf(".txt"), 4);
#endif


            WriteSignificantValue(ref bw, "text" + istr, path);
        }
    }
    
    public override void ReadSignificantInfo(ref System.IO.BinaryReader br)
    {
        base.ReadSignificantInfo(ref br);

        int ec = ReadSignificantValueByte(ref br, "eventsCount");
        events = new EventDef[ec];

        for(int i = 0; i < ec; i++)
        {
            events[i] = new EventDef();

            string istr = i.ToString();

            GameEvents.Sender sender = (GameEvents.Sender)ReadSignificantValueByte(ref br, "sender" + istr);
            GameEvents.EventType type = (GameEvents.EventType)ReadSignificantValueByte(ref br, "type" + istr);
            string args = ReadSignificantValueString(ref br, "args" + istr);
            events[i].Event = new GameEvents.Event(sender, type, args);

            string fn = ReadSignificantValueString(ref br, "text" + istr);
            events[i].narration = Resources.Load<TextAsset>(fn);

            events[i].hitCount = ReadSignificantValueInt32(ref br, "hitcount" + istr);
        }
    }

    public override void LinkSignificantValues(Block[] blocks)
    {
    }

    public override bool IsValidBlock()
    {
        return false;
    }

    void OnGameEvent(GameEvents.Event ev)
    {
        if(events == null) return;

        foreach(EventDef ed in events)
        {
            if(ed.Event.Equals(ev) && (ed.hitCount >= ev.hitCount || ed.hitCount == -1))
            {
                PushNarration(ev.sender, ed);
            }
        }
    }

    GameEvents.Sender playerKindToEventSender(Player.Kind k)
    {
        if (k == Player.Kind.Boy) return GameEvents.Sender.Boy;
        if (k == Player.Kind.Girl) return GameEvents.Sender.Girl;
        if (k == Player.Kind.Bot) return GameEvents.Sender.Bot;

        return GameEvents.Sender.Boy;
    }

    Player.Kind eventSenderToPlayerKind(GameEvents.Sender s)
    {
        if (s == GameEvents.Sender.Boy) return Player.Kind.Boy;
        if (s == GameEvents.Sender.Girl) return Player.Kind.Girl;
        if (s == GameEvents.Sender.Bot) return Player.Kind.Bot;

        return Player.Kind.Boy;
    }

    public void Show(Player.Kind who)
    {
        foreach(EventDef ed in events)
        {
            PushNarration(playerKindToEventSender(who), ed);
            break;
        }
    }

    List<EventDef> narrationsToPlay = new List<EventDef>();

    Player.Kind currentSender = Player.Kind.Boy;

    void PushNarration(GameEvents.Sender sender, EventDef ta)
    {
        if (ta == null) return;
        narrationsToPlay.Add(ta);

        currentSender = eventSenderToPlayerKind(sender);

        string[] lines = Regex.Split(ta.narration.text, "\n|\r|\r\n|\t");

        float currentMoment = 0.0f;
        float currentDuration = 1.0f;
        foreach(string l in lines)
        {
            if (l.Length == 0) continue;
            if(l.StartsWith("@") || l.StartsWith("+")) // is time
            {
                bool isOfs = l.StartsWith("+");
                string ll = l.Remove(0, 1);
                ll = ll.Replace(" ", string.Empty);
                string[] values = ll.Split(',');

                if(isOfs)
                    currentMoment += float.Parse(values[0]);
                else
                    currentMoment = float.Parse(values[0]);
                currentDuration = float.Parse(values[1]);
            }
            else
            if(l.StartsWith("[") && l.EndsWith("]")) // parameters
            {
                if(l.Equals("[override]"))
                {
                    dialogueLines.Clear();
                    RemoveAllTextBubbles();
                }
            }
            else
            {
                AddDialogueLine(currentMoment, currentDuration, l);
            }
        }
    }

    Player boy, girl, bot;

    public override void OnGamePreStart()
    {
        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null)
            r.enabled = false;
    }

    public override void OnGameStarted()
    {
        base.OnGameStarted();

        foreach(Player p in TheGame.Instance.players)
        {
            if (p.kind == Player.Kind.Boy)
                boy = p;

            if (p.kind == Player.Kind.Girl)
                girl = p;

            if (p.kind == Player.Kind.Bot)
                bot = p;
        }

        dialogueLines.Clear();
        dialogueTimer = 0.0f;
    }

    void AddDialogueLine(float moment, float duration, string msg)
    {
        Player.Kind who = currentSender;

        if (msg.StartsWith("boy: ", true, System.Globalization.CultureInfo.InvariantCulture))
        {
            who = Player.Kind.Boy;
            msg = msg.Remove(0, 5);
        }
        else
            if (msg.StartsWith("girl: ", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                who = Player.Kind.Girl;
                msg = msg.Remove(0, 6);
            }
            else
                if (msg.StartsWith("bot: ", true, System.Globalization.CultureInfo.InvariantCulture))
                {
                    who = Player.Kind.Bot;
                    msg = msg.Remove(0, 5);
                }
                else
                    if (msg.StartsWith("other: ", true, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        who = currentSender == Player.Kind.Boy ? Player.Kind.Girl : Player.Kind.Boy;
                        msg = msg.Remove(0, 7);
                    }

        AddDialogueLine(moment, duration, who, msg);
    }

    void AddDialogueLine(float moment, float duration, Player.Kind who, string msg)
    {
        DialogueLine dl = new DialogueLine();
        dl.actor = who;
        dl.moment = moment + dialogueTimer;
        dl.duration = duration;
        dl.text = msg;

        dialogueLines.Add(dl);
    }

    void ShowTextBubble(float duration, Player.Kind who, string msg)
    {
        if (Desktop.main == null)
            return;

        GameObject obj = GameObject.Instantiate<GameObject>(narrationBox);
        obj.transform.SetParent(Desktop.main.overlays);
        TextBubble tb = obj.GetComponent<TextBubble>();


        Player p = boy;
        if (who == Player.Kind.Girl) p = girl;
        if (who == Player.Kind.Bot) p = bot;

        Vector3 pos = WorldPositionToDesktopPosition(p.transform, tb.GetComponent<RectTransform>());
        tb.message.text = msg;
        tb.DestroyAfter(duration);
        tb.objectFollowed = p.transform;

        if (pos.x > Desktop.main.width * 0.5f)
            tb.SetAnchor(0.8f);

        textBubbles.Add(tb);
    }

    Vector3 WorldPositionToDesktopPosition(Transform wt, RectTransform dp)
    {
        Vector3 pos = TheGame.Instance.gameCamera.WorldToViewportPoint(wt.position + Vector3.forward * GameSession.gridUnit * 0.5f);
        pos = Desktop.main.ViewportToDesktop(pos);
        dp.anchoredPosition = pos;
        return pos;
    }

    void RemoveAllTextBubbles()
    {
        for (int i = 0; i < textBubbles.Count; i++)
            if (textBubbles[i] != null)
                textBubbles[i].DestroyAfter(0.5f);
    }
}
