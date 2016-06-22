using UnityEngine;
using System.Collections;

public class MovingPlatform2 : Block {

    public float range = 0.0f;
    public float movementDuration = 1.0f; // per cell
    
    private bool stickyOnY = false;

    Vector3 initialPosition;
    Vector3 previousPosition;

    Vector3 startPosition, endPosition;


    bool wasStarted = false;
	// Use this for initialization
	public override void Start ()
    {
        base.Start();

        GetComponent<Collider>().isTrigger = true;

        SnapToGrid.SnapToGround(this);

        initialPosition = position;
        previousPosition = initialPosition;

        startPosition = initialPosition;
        endPosition = initialPosition + transform.localRotation * Vector3.forward * range * GameSession.gridUnit;

        Move();

        stickyOnY = navigable;

        OnPlayerHit = _OnPlayerHit;
        OnPlayerLeft = _OnPlayerLeft;

        wasStarted = true;
	}
	
	// Update is called once per frame
    int movingStep = 0; // forward, backward

    float climbUpDuration = 0.7f;
    float climbUpTimer = 0.7f;

    Player playerOnTop = null;

	public override void Update ()
    {
        base.Update();

        if (behaviourActive == false)
            return;

        startPosition = initialPosition;
        endPosition = initialPosition + transform.localRotation * Vector3.forward * range * GameSession.gridUnit;

        playerOnTop = null;

        if (TheGame.Instance != null)
        {
            if (range != 0.0f || stickyOnY)
            {
                Vector3 movingVector = position - previousPosition;
                previousPosition = position;

                if (movingVector.sqrMagnitude > 0.0f)
                {
                    movingVector.Normalize();

                    Vector3 front = transform.position + movingVector * GameSession.gridUnit;


                    // check for obstacles
                    Block blockInFront = root.blocks.ItemAt(front);
                    if (blockInFront != null && (blockInFront.navigable == false || blockInFront.blockType == BlockType.MovableBox))
                    {
                        LeanTween.cancel(gameObject, true);
                        moveTimer = getRangeDuration();
                        Vector3 p = blockInFront.position - movingVector * GameSession.gridUnit;
                        p.y = position.y;
                        position = p;
                        if (movingStep % 2 == 0)
                            endPosition = position;
                        else
                            startPosition = position;
                    }
                    else
                    {
                        foreach (Player p in TheGame.Instance.players)
                        {
                            float d = p.DistanceTo(this);
                            if (d < GameSession.gridUnit)
                            {
                                if (stickyOnY && d < (GameSession.gridUnit * 0.9f))
                                {
                                    climbUpTimer -= Time.deltaTime;
                                    climbUpTimer = Mathf.Max(0.0f, climbUpTimer);
                                    Vector3 pos = position;
                                    pos.y = p.position.y;

                                    if(p.IsMoving() == false)
                                    {
                                        playerOnTop = p;
                                    }
                                        
                                    break;
                                }
                                else if(!stickyOnY)
                                {
                                    Vector3 dirToPlayer = p.position - position; dirToPlayer.y = 0.0f; dirToPlayer.Normalize();

                                    //if(p.DistanceTo(front) < GameSession.gridUnit * 0.5f)// && Vector3.Dot(dirToPlayer, movingVector) > 0.99f)
                                    {
                                        p.outsidePushVector = movingVector;
                                        p.transform.position = front;
                                        p.FinishedMoving(front);

                                        Block b = root.blocks.ItemAt(front + movingVector * GameSession.gridUnit * 0.5f);

                                        if (p.CanMoveTo(front + movingVector * GameSession.gridUnit * 0.5f) == false || (b != null && b.blockType == BlockType.MovableBox))
                                        {
                                            p.Die(Player.DieMode.Trap);
                                        }
                                    }
                                }
                            }
                        }

                        if (playerOnTop == null)
                            climbUpTimer = climbUpDuration;
                    }
                }
            }
        }

        float t = moveTimer / getRangeDuration();
        if (t >= 1.0f)
        {
            movingStep++;
            Move();
        }

        transform.position = Vector3.Lerp(movingStart, movingEnd, Mathf.Clamp01(moveTimer / getRangeDuration()));
        moveTimer += Time.deltaTime;
	}

    void LateUpdate()
    {
        if(playerOnTop != null)
        {
            Vector3 pos = position;
            pos.y = playerOnTop.position.y;
            playerOnTop.position = Vector3.Lerp(pos, playerOnTop.position, climbUpTimer / climbUpDuration);
        }
    }

    float moveTimer = 0.0f;
    Vector3 movingStart, movingEnd;
    void Move()
    {
        if (GetComponent<SnapToGrid>() != null)
            GetComponent<SnapToGrid>().enabled = false;

        if (movingStep % 2 == 1)
        {
            movingStart = endPosition;
            movingEnd = startPosition;
        }
        else
        {
            movingStart = startPosition;
            movingEnd = endPosition;
        }

        moveTimer = 0.0f;
    }

    
    void _OnPlayerHit(Player p)
    {
        if(navigable == false)
            p.CancelMoving();
    }

    void _OnPlayerLeft(Player p)
    {

    }

    public override void WriteSignificantInfo(ref System.IO.BinaryWriter bw)
    {
        base.WriteSignificantInfo(ref bw);

        WriteSignificantValue(ref bw, "range", range);
        WriteSignificantValue(ref bw, "duration", movementDuration);
    }

    public override void ReadSignificantInfo(ref System.IO.BinaryReader br)
    {
        base.ReadSignificantInfo(ref br);

        range = ReadSignificantValueFloat(ref br, "range");
        movementDuration = ReadSignificantValueFloat(ref br, "duration");
    }

    public void Reset(bool resetPos = false)
    {
        if (wasStarted == false)
            return;

        if(resetPos)
        {
            SnapToGrid.SnapToGround(this);
            initialPosition = transform.position;
        }

        previousPosition = initialPosition;

        startPosition = initialPosition;
        endPosition = initialPosition + transform.localRotation * Vector3.forward * range * GameSession.gridUnit;
        moveTimer = 0.0f;
        movingStep = 0;

        position = initialPosition;

        Move();
    }

    public override Vector3 origin
    {
        get
        {
            return initialPosition;
        }
    }

    protected override void OnBehaviourEnabled(bool enable)
    {
        base.OnBehaviourEnabled(enable);

        Reset();
    }

    public bool PointInRange(Vector3 p)
    {
        Vector3 d = p - origin; d.y = 0.0f;
        Vector3 fwd = transform.localRotation * Vector3.forward;
        d.Normalize();
        if (Vector3.Dot(d, fwd) < 0.995f) return false;

        return d.sqrMagnitude < range * range;
    }

    public bool BlockInRange(Block b)
    {
        return PointInRange(b.position);
    }

    float getRangeDuration()
    {
        return Mathf.Max(1.0f, range) * movementDuration;
    }
}
