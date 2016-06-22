using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Player : MonoBehaviour {

    public enum DieMode
    {
        Falling,
        Trap,
        Overlap,
    };

    public enum Kind
    {
        Boy,
        Girl,
        Bot
    };

    public Kind kind = Kind.Boy;

    public float life = 100.0f;

    //bool isJumping = false;

    [HideInInspector]
    public Vector2 lastMove = Vector2.zero;

    [HideInInspector]
    public bool isTeleporting = false;

    Vector3 futurePosition = Vector3.zero;

    [HideInInspector]
    public Vector3 outsidePushVector = Vector3.zero;

    private bool hasBotBehaviour = false;
    private bool checkedBotBehaviour = false;

    [NonSerialized]
    public bool isDead = false;

    [NonSerialized]
    public List<Block> pushedBoxes = new List<Block>();

    [NonSerialized]
    public Vector3 nextPosition;

    private BlendTextures blendTextures;

    private float idleTimer = 0.0f;
    private float idleEventTime = 4.6f;

    public enum PlayerAction
    {
        PlayerFinishedMoving
    }

	void Start ()
    {
        /*if (!isBall)
        {
            transform.localScale = Vector3.one * 0.001f;
            LeanTween.scale(gameObject, Vector3.one, UnityEngine.Random.Range(0.3f, 0.5f)).setDelay(UnityEngine.Random.Range(0.1f, 0.4f)).setEase(LeanTweenType.easeOutElastic);
        }
        else
        {
            rotatingBall = GetComponentInChildren<RotatingBall>();
            rotatingBall.EnableRotation(false);
        }*/

        gameObject.transform.localScale = Vector3.one;

        DissolveTexture[] dts = GetComponentsInChildren<DissolveTexture>();
        if(dts != null)
        {
            foreach(DissolveTexture dt in dts)
                dt.SetFrom(0.0f, 1.0f);
        }

        MarkGroundBlock();

        getBotBehaviour();

        isDead = false;

        nextPosition = transform.position;

        blendTextures = GetComponent<BlendTextures>();

        idleTimer = 0.0f;
        idleEventTime = UnityEngine.Random.Range(3.0f, 9.0f);
	}

    void getBotBehaviour()
    {
        if(checkedBotBehaviour == false)
        {
            hasBotBehaviour = GetComponent<BotBehaviour>() != null;
            checkedBotBehaviour = true;
        }
    }

    bool isOnTable(Vector3 pos)
    {
        return TheGame.Instance.blocks.ItemAt(pos) != null;
    }

    public bool CanMoveTo(Vector3 pos)
    {
        /*if (TheGame.Instance.blocks.Intersects(transform.position, pos))
        {
            return false;
        }*/

        Vector3 directionOfMove = DirectionTo(pos);
        Vector3 movement = directionOfMove * GameSession.gridUnit;
        if (movement.sqrMagnitude < 0.0001f) return false;

        Block b = TheGame.Instance.blocks.ItemAt(pos);
        if (b == null) return true;

        Player pp = TheGame.Instance.PlayerAt(pos);
        if (pp != null && pp.CanMoveTo(pp.position + movement) == false)
            return false;

        MovableBox mb = TheGame.Instance.blocks.ItemAt<MovableBox>(pos);
        if(mb != null)
            return mb.CanBeWalkedOn(directionOfMove);

        return b.CanBeWalkedOn(directionOfMove);
    }
    
	bool finishedMoving = true;

    public LeanTweenType movementInterpolation = LeanTweenType.easeOutBack;
    public float movementDuration = 0.26f;

    MovableBox pushedBox = null;

    public void MoveTo(Vector3 newPosition, Action<Player> onCompleted = null)
    {
        ResetLocalMovement();

		Vector3 dir = newPosition - transform.position;

        //float threshold = GameSession.gridUnit * 0.1f;

        if(CanMoveTo(newPosition))
        {
            float movementDurationFactor = 1.0f;           
            finishedMoving = false;
            float totalDuration = movementDuration * movementDurationFactor;
            MovableBox mb = TheGame.Instance.blocks.ItemAt<MovableBox>(newPosition);
            if(mb != null)
            {
                pushedBox = mb;
                mb.PushedByPlayer(this);

            }

            foreach (PowerupGeneric pg in powerups.Values)
            {
                if (pg.PlayerCanMove() == false)
                {
                    newPosition = position;
                    break;
                }
            }

            if (blendTextures != null)
                blendTextures.SetFactorInterpolated(1.0f);

            idleTimer = 0.0f;

            GameEvents.Send(kind, GameEvents.EventType.Moving);

            LeanTween.move(gameObject, newPosition, totalDuration).setEase(movementInterpolation).setOnComplete(() =>
                {
                    if (onCompleted != null)
                        onCompleted(this);
                    FinishedMoving(newPosition);
                });

            /*LeanTween.scale(gameObject, Vector3.one * 1.15f, totalDuration * 0.5f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
                {
                    LeanTween.scale(gameObject, Vector3.one, totalDuration * 0.5f).setEase(LeanTweenType.easeOutBack);
                });*/

            lastMove = NormalizeDirection(new Vector2(dir.x, dir.z));

            if (Mathf.Abs(dir.x) > 0.0f)
            {
                LookAtDir(dir);
            }

        }
        else
        {
            if (onCompleted != null)
                onCompleted(this);

            GameEvents.Send(kind, GameEvents.EventType.HitBox);
        }

        nextPosition = newPosition;
    }

    public void MoveToFast(Vector3 newPosition, Action<Player> onCompleted = null)
    {
        ResetLocalMovement();

        Vector3 dir = newPosition - transform.position;

        //float threshold = GameSession.gridUnit * 0.1f;

        if (CanMoveTo(newPosition))
        {
            finishedMoving = true;
            gameObject.transform.position = newPosition;
            if (onCompleted != null)
                onCompleted(this);
            FinishedMoving(newPosition);

            lastMove = NormalizeDirection(new Vector2(dir.x, dir.z));
        }
        else
        {
            if (onCompleted != null)
                onCompleted(this);
        }
    }

    public static Vector2 NormalizeDirection(Vector2 direction)
    {
        Vector2 diff = direction;
        if (diff.x != 0.0f)
            diff.x = Mathf.Sign(diff.x);
        if (diff.y != 0.0f)
            diff.y = Mathf.Sign(diff.y);

        return diff;
    }

    public void Move(Vector3 vector)
    {
		if (finishedMoving == false)
			return;

        if (isTeleporting)
            return;
        
		Vector3 newPosition = SnapToGrid.Snap(transform.position + vector * GameSession.gridUnit);
        
        MoveTo(newPosition);
    }

    bool aMovementWasTried = false;
    Vector3 movementTried = Vector3.zero;

    public bool CanMove(Vector3 vector)
    {
        if (finishedMoving == false)
            return false;

        if (isTeleporting)
            return false;

        Vector3 newPosition = SnapToGrid.Snap(transform.position + vector * GameSession.gridUnit);

        return CanMoveTo(newPosition);
    }

    public void TryToMove(Vector3 vector)
    {
        if (finishedMoving == false)
            return;

        if (isTeleporting)
            return;

        Vector3 newPosition = SnapToGrid.Snap(transform.position + vector * GameSession.gridUnit);
        Vector3 realNewPosition = newPosition;

        if (CanMoveTo(newPosition))
            futurePosition = realNewPosition;

        aMovementWasTried = true;
        movementTried = vector;
    }

    public bool CompleteMovement(bool fast = false)
    {
        bool ret = aMovementWasTried;
        if (aMovementWasTried)
        {
            bool canMove = true;
            foreach(Player p in TheGame.Instance.players)
            {
                if (p == this) continue;
                Vector3 d = p.futurePosition - futurePosition; d.y = 0.0f;
                if (d.magnitude < GameSession.gridUnit * 0.2f)
                    canMove = false;                
            }

            // pushed by piston
            if (Vector3.Dot(outsidePushVector, movementTried) < -0.99f) // opposed vectors
                canMove = false;

            if (canMove)
            {
                if (fast)
                    MoveToFast(futurePosition);
                else
                    MoveTo(futurePosition);
            }
            
            if(canMove == false || Vector3.Distance(futurePosition, transform.position) < 0.001f) // didn't move
            {
                DoMovementFailed(movementTried);
            }

            ret = canMove;

        }
        aMovementWasTried = false;

        outsidePushVector = Vector3.zero;

        return ret;
    }

    public void StartMovement()
    {
        futurePosition = transform.position;
        aMovementWasTried = false;
    }

	public void FinishedMoving(Vector3 newPosition)
	{
        if (blendTextures != null)
            blendTextures.SetFactorInterpolated(0.0f, 0.2f);

        idleTimer = 0.0f;

        if (pushedBox != null)
        {
            pushedBox.PushedByPlayer(null);
            pushedBox = null;
        }

		finishedMoving = true;

        if (isOnTable(newPosition) == false)
        {
            Die(DieMode.Falling);
        }

        MarkGroundBlock();

        foreach(PowerupGeneric pg in powerups.Values)
        {
            pg.OnPlayerAction(PlayerAction.PlayerFinishedMoving);
        }
	}

    void MarkGroundBlock()
    {
#if UNITY_EDITOR
        Block b = blockUnderneath;
        if (b != null)
        {
            GroundBlock gb = b as GroundBlock;
            if (gb != null)
            {
                //gb.SetBrightness(-0.4f);
                gb.SetColor(new Color(0.84f, 1.0f, 1.0f, 1.0f));
                gb.IncrementUsage();
            }
        }
#endif
    }

    public void Update()
    {
        //NavMeshAgent agent = GetComponent<NavMeshAgent>();
        //if (agent.isOnNavMesh == false && isJumping == false)
        //    TheGame.Instance.TriggerGameLost();
      
        idleTimer += Time.deltaTime;

        if(idleTimer > idleEventTime)
        {
            idleTimer = 0.0f;
            idleEventTime = UnityEngine.Random.Range(3.0f, 9.0f);
            DoIdleEvent();
        }
    }

    void DoIdleEvent()
    {
        GameEvents.Send(kind, GameEvents.EventType.CharacterIdle);
        int dice = UnityEngine.Random.Range(1, 10);

        if (dice % 2 == 0)
            FlipLookAt();
        else
            DoALittleJump();
    }

    bool isDieing = false;

    public void Die(DieMode mode)
    {
        if (isDieing) return;

#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8
        //if(GameSession.GetSettingForVibrations())
        ///    Handheld.Vibrate();
#endif

        isDieing = true;
        //canRotate = false;

        DissolveTexture[] dts = GetComponentsInChildren<DissolveTexture>();
        if (dts != null)
        {
            foreach(DissolveTexture dt in dts)
                dt.SetFrom(1.0f, 0.0f);
        }

        if(mode == DieMode.Trap)
        {
            LeanTween.scale(gameObject, Vector3.one * 0.7f, 0.35f).setEase(LeanTweenType.easeOutBack);
        }

        if(mode == DieMode.Falling || mode == DieMode.Trap)
        {
            //LeanTween.scale(gameObject, Vector3.one * 0.001f, 0.5f).setEase(LeanTweenType.easeOutBack);

            // play fx
        }

        /*if (mode == DieMode.Trap)
        {
            float startAngle = transform.eulerAngles.y;
            float delay = 0.0f;
            for(int i = 0; i < 5; i++)
            {
                float angle = 30.0f;
                if (i % 2 == 0)
                    angle *= -1.0f;
                float duration = Random.Range(0.3f, 0.5f) * 0.25f;
                LeanTween.rotateY(gameObject, startAngle + angle, duration).setEase(LeanTweenType.linear).setDelay(delay);
                delay += duration;
            }

            LeanTween.scale(gameObject, Vector3.one * 0.001f, 0.5f).setEase(LeanTweenType.easeInBack).setDelay(delay);
        }*/

        if(mode == DieMode.Overlap)
        {
        }

        if(isControlledByPlayer)
            TheGame.Instance.TriggerGameLost();

        GameEvents.Send(kind, GameEvents.EventType.Fall);

        isDead = true;
    }
    public float DistanceTo(Vector3 pos2)
    {
        Vector3 pos1 = transform.position; pos1.y = 0.0f;
        pos2.y = 0.0f;
        return Vector3.Distance(pos1, pos2);
    }

    public float DistanceTo(MonoBehaviour mb)
    {
        return DistanceTo(mb.transform.position);
    }
    public float DistanceTo3D(MonoBehaviour mb)
    {
        Vector3 pos1 = transform.position;
        Vector3 pos2 = mb.transform.position;
        return Vector3.Distance(pos1, pos2);
    }

    public Vector3 VectorFrom(MonoBehaviour mb)
    {
        Vector3 pos1 = transform.position; pos1.y = 0.0f;
        Vector3 pos2 = mb.transform.position; pos2.y = 0.0f;
        pos1 -= pos2;
        pos1.Normalize();
        pos1.x = Mathf.Sign(pos1.x);
        pos1.z = Mathf.Sign(pos1.z);
        return pos1;
    }

    public Vector3 DirectionTo(Transform t)
    {
        return DirectionTo(t.position);
    }

    public Vector3 DirectionTo(Vector3 pos)
    {
        Vector3 d = pos - position;
        d.y = 0.0f;
        d.Normalize();
        return d;
    }

    public void MoveLocal(Vector3 pos, bool fast = false)
    {
        localMovementReset = false;
    }

    bool localMovementReset = true;
    public void ResetLocalMovement()
    {
        if (localMovementReset) return;
        MoveLocal(Vector3.zero);
        localMovementReset = true;
    }

    public bool IsMoving()
    {
        return finishedMoving == false || isTeleporting;
    }

    void DoMovementFailed(Vector3 dir)
    {
        
    }

    public void CancelMoving()
    {
        LeanTween.cancel(gameObject, true);
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

    public Block blockUnderneath
    {
        get
        {
            return TheGame.Instance.blocks.GroundAt(position);
        }
    }

    public bool CanPushPlayersFromPos(Vector3 pos)
    {
        Player p = TheGame.Instance.PlayerAt(pos);
        if (p == null) return true;

        Vector3 newPos = pos + new Vector3(lastMove.x, 0.0f, lastMove.y) * GameSession.gridUnit;

        while (true)
        {
            if (p.CanMoveTo(newPos) == false)
                return false;

            p = TheGame.Instance.PlayerAt(newPos);
            if (p == null)
                break;

            newPos += new Vector3(lastMove.x, 0.0f, lastMove.y) * GameSession.gridUnit;
        }

        return true;
    }

    public void PushPlayersFromPos(Vector3 pos)
    {
        Player p = TheGame.Instance.PlayerAt(pos);
        if (p == null) return;

        Vector3 newPos = pos + new Vector3(lastMove.x, 0.0f, lastMove.y) * GameSession.gridUnit;

        while (true)
        {
            p.MoveTo(newPos);

            p = TheGame.Instance.PlayerAt(newPos);
            if (p == null)
                break;

            newPos += new Vector3(lastMove.x, 0.0f, lastMove.y) * GameSession.gridUnit;
        }
    }

    public bool isControlledByPlayer
    {
        get
        {
            getBotBehaviour();
            return !hasBotBehaviour;
        }
    }

    public bool isBot
    {
        get
        {
            getBotBehaviour();
            return hasBotBehaviour;
        }
    }

    Dictionary<GameEconomy.EconomyItemType, PowerupGeneric> powerups = new Dictionary<GameEconomy.EconomyItemType, PowerupGeneric>();

    public void ApplyPowerup(GameEconomy.EconomyItemType pu)
    {
        PowerupGeneric pg = null;
        if(pu == GameEconomy.EconomyItemType.PowerupFreezeCharacter)
        {
            GameObject o = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("PowerupFreeze"));
            o.transform.SetParent(transform);
            o.transform.localPosition = new Vector3(-4.0f, 1.0f, 4.0f);
            Widget.SetLayer(o, gameObject.layer);

            pg = o.GetComponent<PowerupGeneric>();


        }

        powerups.Add(pu, pg);
    }

    public void RemovePowerup(GameEconomy.EconomyItemType pu)
    {
        if(powerups.ContainsKey(pu))
        {
            PowerupGeneric pg = powerups[pu];
            if (pg != null)
                Destroy(pg.gameObject);
            powerups.Remove(pu);
        }
    }

    public bool HasPowerup(GameEconomy.EconomyItemType pu)
    {
        return powerups.ContainsKey(pu);
    }


    void LookAtDir(Vector3 dir)
    {
        Transform c = transform.GetChild(0);
        float scaling = Mathf.Abs(c.transform.localScale.z);
        LeanTween.scale(c.gameObject, new Vector3(Mathf.Sign(dir.x), 1.0f, 1.0f) * scaling, 0.35f).setEase(LeanTweenType.easeOutExpo);
        //LeanTween.rotateZ(c.gameObject, -Mathf.Sign(dir.x) * 90.0f + 90.0f, 0.25f).setEase(LeanTweenType.easeOutExpo);
    }
    void FlipLookAt()
    {
        Transform c = transform.GetChild(0);
        float scaling = Mathf.Abs(c.transform.localScale.z);
        LeanTween.scale(c.gameObject, new Vector3(Mathf.Sign(c.transform.localScale.x) * -1.0f, 1.0f, 1.0f) * scaling, 0.5f).setEase(LeanTweenType.easeOutExpo);
    }

    void DoALittleJump()
    {
        Transform c = transform.GetChild(0);

        float jumpDuration = 0.4f;

        LeanTween.moveLocalZ(c.gameObject, GameSession.gridUnit * 0.07f, jumpDuration * 0.5f).setEase(LeanTweenType.easeOutSine).setOnComplete(() =>
            {
                LeanTween.moveLocalZ(c.gameObject, 0.0f, jumpDuration * 0.5f).setEase(LeanTweenType.easeInSine);
            });
    }
}
