using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[SelectionBase]
public class Block : BlockModifier {

    public bool navigable = true;
    public List<Player> playersOnSite = new List<Player>();

    public Action<Player> OnPlayerInTheMiddle;
    public Action<Player> OnPlayerHit;
    public Action<Block, Vector3> OnBlockHit;
    public Action<Player> OnPlayerLeft;

    public string sourcePrefab;

    private bool _hasMoved = false;
    private Vector3 prevPosition;

    [NonSerialized]
    public bool isTeleporting = false;

    [NonSerialized]
    public bool isEnteringTeleport = false;

    [NonSerialized]
    public bool isLeavingTeleport = false;

    public bool ignoreOnGamePlay = false;


    public enum BlockState
    {
        State_Undefined,
        State_On,
        State_Off
    };

    public BlockState state = BlockState.State_On;
    private BlockState prevState = BlockState.State_Undefined;

    public enum BlockType
    {
        Undefined,
        Ground,
        Spawn,
        Finish,
        Trap,
        Portal,
        Button,
        Piston,
        Default,
        Door,
        Wall,
        MovableBox,
        Crumbling,
        Waypoints,
        Tutorial,
        Player,
        PushButton,
        Narration
    };

    public BlockType blockType = BlockType.Default;

    [HideInInspector]
    public byte blockIdentifier = 0;

    // Use this for initialization

    protected LevelRoot root;
    public virtual void Awake()
    {
#if UNITY_EDITOR
        UnityEngine.Object src = UnityEditor.PrefabUtility.GetPrefabParent(gameObject);
        if(src != null)
            sourcePrefab = src.name;
#endif
    }

    public virtual void Start () {

        prevPosition = position;

        if (GetComponent<SnapToGrid>() == null)
            gameObject.AddComponent<SnapToGrid>();

        getRoot();
	}

    List<Player> playersToAdd = new List<Player>();

    public virtual void Update()
    {
        UpdateState();

        if(playersToAdd.Count > 0)
        {
            for(int i = 0; i < playersToAdd.Count; i++)
            {
                Player p = playersToAdd[i];
                Vector3 d = p.transform.position - transform.position;
                d.y = 0.0f;
                if(d.sqrMagnitude < 0.01f)
                {
                    playersOnSite.Add(p);
                    playersToAdd.RemoveAt(i);
                    if (OnPlayerInTheMiddle != null)
                        OnPlayerInTheMiddle(p);
                    break;
                }
            }
        }

        Vector3 dist = position - prevPosition;
        _hasMoved = dist.sqrMagnitude > 1.0f;
    }

    public bool hasMoved
    {
        get
        {
            return _hasMoved;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Player otherPlayer = other.gameObject.GetComponent<Player>();
        if (otherPlayer != null)
        {
            if (playersToAdd.Contains(otherPlayer) == false)
            {
                playersToAdd.Add(otherPlayer);
                if (OnPlayerHit != null)
                    OnPlayerHit(otherPlayer);
            }
        }
        else
        {
            Block b = other.gameObject.GetComponent<Block>();
            if(b != null)
            {
                if (OnBlockHit != null)
                {
                    Vector3 d = position - b.position; d.y = 0.0f; d.Normalize();
                    OnBlockHit(b, d);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Player otherPlayer = other.gameObject.GetComponent<Player>();
        if (otherPlayer == null) return;
        playersOnSite.Remove(otherPlayer);
        playersToAdd.Remove(otherPlayer);
        if (OnPlayerLeft != null)
            OnPlayerLeft(otherPlayer);
    }

    void UpdateState()
    {
        if(prevState != state)
        {
            SetState(state, true);
        }
    }

    public void SetState(BlockState newState, bool force = false)
    {
        if (newState == state && !force) return;

        state = newState;

        if (OnStateChanged != null)
        {
            OnStateChanged(prevState, state);
        }

        StateChanged(prevState, state);

        prevState = state;
    }

    public void ToggleState()
    {
        if (state == BlockState.State_On)
            SetState(BlockState.State_Off);
        else
            SetState(BlockState.State_On);
    }

    public Action<BlockState, BlockState> OnStateChanged;
    protected virtual void StateChanged(BlockState prev, BlockState current)
    {

    }

    public override void WriteSignificantInfo(ref System.IO.BinaryWriter bw)
    {
        base.WriteSignificantInfo(ref bw);

        Vector3 pos = transform.position;

        WriteSignificantValue(ref bw, "px", pos.x);
        WriteSignificantValue(ref bw, "py", pos.y);
        WriteSignificantValue(ref bw, "pz", pos.z);
        WriteSignificantValue(ref bw, "ry", transform.rotation.eulerAngles.y);

        WriteSignificantValue(ref bw, "bid", blockIdentifier);

        WriteSignificantValue(ref bw, "st", (byte)state);

        WriteSignificantValue(ref bw, "navi", navigable);

        WriteSignificantValue(ref bw, "name", gameObject.name);
    }

    public override void ReadSignificantInfo(ref System.IO.BinaryReader br)
    {
        base.ReadSignificantInfo(ref br);

        Vector3 pos = Vector3.zero;
        pos.x = ReadSignificantValueFloat(ref br, "px");
        pos.y = ReadSignificantValueFloat(ref br, "py");
        pos.z = ReadSignificantValueFloat(ref br, "pz");

        float angleY = ReadSignificantValueFloat(ref br, "ry");
        transform.rotation = Quaternion.Euler(0.0f, angleY, 0.0f);

        blockIdentifier = ReadSignificantValueByte(ref br, "bid");

        state = (BlockState)ReadSignificantValueByte(ref br, "st");

        gameObject.name = ReadSignificantValueString(ref br, "name");

        navigable = ReadSignificantValueBool(ref br, "navi");

        transform.position = pos;
    }

    public virtual bool IsValidBlock()
    {
        return state == BlockState.State_On;
    }

    public virtual bool CanBeWalkedOn(Vector3 fromDir)
    {
        return navigable || IsValidBlock() == false;
    }

    public void EnableRendering(bool enable = true)
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
            r.enabled = enable;
    }

    public void EnableRendering(float factor)
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
            r.enabled = factor > 0.4f;
    }

    public void Enable(bool enable = true)
    {
        BlockModifier[] bms = GetComponents<BlockModifier>();
        foreach (BlockModifier bm in bms)
        {
            bm.enabled = enable;
        }
    }

    public void Disable()
    {
        Enable(false);
    }

    public override void EnableBehaviour(bool enable = true)
    {
        base.EnableBehaviour(enable);

        BlockModifier[] bms = GetComponents<BlockModifier>();

        foreach(BlockModifier bm in bms)
        {
            if (bm == this) continue;
            bm.EnableBehaviour(enable);
        }

        OnBehaviourEnabled(enable);
    }

    protected virtual void OnBehaviourEnabled(bool enable)
    {

    }

    public Vector3 position
    {
        get
        {
            return transform.position;
        }
        set
        {
            transform.position = value;
        }
    }

    public virtual Vector3 origin
    {
        get
        {
            return transform.position;
        }
    }

    public Action<Block> OnBlockDeleted;

    protected virtual void OnDestroy()
    {
        if (OnBlockDeleted != null)
            OnBlockDeleted(this);
    }

    protected void getRoot()
    {
        if(root == null)
            root = GetComponentInParent<LevelRoot>();

        if (root != null && root.blocks.ground.Count == 0)
            root.RefreshStructure();
    }

    public bool IsNeighbourOf(Block b)
    {
        Vector3 d = position - b.position;
        d.y = 0.0f;
        if (Mathf.Abs(d.x) > 0.1f && Mathf.Abs(d.z) > 0.1f) return false;
        if (Mathf.Abs(d.x) < 0.1f) d.x = 0.0f;
        if (Mathf.Abs(d.z) < 0.1f) d.z = 0.0f;
        return Mathf.Abs(d.magnitude - GameSession.gridUnit) < 0.05f;
    }

    public Block[] GetNeighbours(bool onlyNSEW = true, bool ignoreWalls = true)
    {
        getRoot();

        List<Block> n = new List<Block>();

        if(root != null)
        {
            Block b = GetNeighbour(Vector3.left);
            if (b != null) n.Add(b);

            b = GetNeighbour(Vector3.right);
            if (b != null) n.Add(b);

            b = GetNeighbour(Vector3.forward);
            if (b != null) n.Add(b);

            b = GetNeighbour(Vector3.back);
            if (b != null) n.Add(b);

            if(!onlyNSEW)
            {
                b = GetNeighbour(new Vector3(-1.0f, 0.0f, 1.0f));
                if (b != null) n.Add(b);

                b = GetNeighbour(new Vector3(1.0f, 0.0f, 1.0f));
                if (b != null) n.Add(b);

                b = GetNeighbour(new Vector3(1.0f, 0.0f, -1.0f));
                if (b != null) n.Add(b);

                b = GetNeighbour(new Vector3(-1.0f, 0.0f, -1.0f));
                if (b != null) n.Add(b);
            }
        }

        if(ignoreWalls)
        {
            for(int i = 0; i < n.Count; i++)
            {
                if(n[i].blockType == BlockType.Wall)
                {
                    n.RemoveAt(i);
                    i--;
                }
            }
        }

        return n.ToArray();
    }

    public Block[] GetGroundNeighbours()
    {
        getRoot();

        List<Block> n = new List<Block>();

        if (root != null)
        {
            Block b = GetGroundNeighbour(Vector3.left);
            if (b != null) n.Add(b);

            b = GetGroundNeighbour(Vector3.right);
            if (b != null) n.Add(b);

            b = GetGroundNeighbour(Vector3.forward);
            if (b != null) n.Add(b);

            b = GetGroundNeighbour(Vector3.back);
            if (b != null) n.Add(b);
        }

        return n.ToArray();
    }

    public Block GetNeighbour(Vector3 side)
    {
        getRoot();

        if (root == null) return null;

        return root.blocks.ItemAt(position + side * GameSession.gridUnit);
    }

    public Block GetGroundNeighbour(Vector3 side)
    {
        getRoot();

        if (root == null) return null;

        return root.blocks.GroundAt(position + side * GameSession.gridUnit);
    }

    public virtual void Refresh()
    {

    }

    public Player GetPlayerAtFutureLocation()
    {
        if(TheGame.Instance == null) return null;
        foreach(Player p in TheGame.Instance.players)
        {
            Vector3 d = p.nextPosition - position; d.y = 0.0f;
            if (d.magnitude < GameSession.gridUnit * 0.1f)
                return p;
        }

        return null;
    }

    public Player GetPlayerAtLocation()
    {
        if (TheGame.Instance == null) return null;
        foreach (Player p in TheGame.Instance.players)
        {
            Vector3 d = p.position - position; d.y = 0.0f;
            if (d.magnitude < GameSession.gridUnit * 0.1f)
                return p;
        }

        return null;
    }

    public virtual void OnGameStarted()
    {
        BlockShadow[] allShadows = GetComponentsInChildren<BlockShadow>();

        foreach (BlockShadow bs in allShadows)
            bs.IncrementOffset();
    }

    public virtual void OnGamePreStart()
    {
        BlockShadow.Reset();
    }

    public virtual void OnGamePostStart()
    {

    }

    public virtual bool ModifyPosition(Vector3 p, bool animated = false)
    {
        p.y = position.y;
        position = p;
        return true;
    }
}
