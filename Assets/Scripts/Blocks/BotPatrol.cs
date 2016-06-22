using UnityEngine;
using System.Collections;

public class BotPatrol : BotBehaviour {

    AIWaypoints pathToFollow = null;

    public enum PatrolType
    {
        OneTime,
        BackAndForth,
        Loop
    };

    public PatrolType type = PatrolType.BackAndForth;

    private int targetPointIndex = 0;
    private int patrolDirection = 1;

    private Player player;

    public float durationBetweenMoves = 0.5f;

    private float timeToMove = 1.0f;

    public bool randomizePath = true;
    public bool avoidOtherPlayers = true;

	void Start () {

        player = GetComponent<Player>();

        Reset();
	}
	
	// Update is called once per frame
	void Update () {

        if (pathToFollow == null || player.isDead) return;

        if(timeToMove > 0.0f)
        {
            timeToMove -= Time.deltaTime;
            if(timeToMove <= 0.0f)
            {
                Vector3 dir = currentPoint - player.position;

                if(avoidOtherPlayers)
                {
                    foreach(Player p in TheGame.Instance.players)
                    {
                        if (p.isBot) continue;
                        if(p.DistanceTo(player) < GameSession.gridUnit * 1.1f)
                        {
                            // move away
                            dir = -(p.position - player.position) * 2.0f; dir.y = 0.0f;
                            break;
                        }
                    }
                }

                int dice = randomizePath ? Random.Range(0, 100) % 2 : 0;

                if(dice == 0)
                {
                    if (Mathf.Abs(dir.x) < 0.5f)
                        dir.x = 0.0f;
                    else
                        dir.z = 0.0f;

                    if (Mathf.Abs(dir.z) < 0.5f)
                        dir.z = 0.0f;
                }
                else
                {
                    if (Mathf.Abs(dir.z) < 0.5f)
                        dir.z = 0.0f;
                    else
                        dir.x = 0.0f;

                    if (Mathf.Abs(dir.x) < 0.5f)
                        dir.x = 0.0f;
                }
                

                if (Mathf.Abs(dir.x) > 0.0f || Mathf.Abs(dir.z) > 0.0f)
                {
                    Vector3 newPos = player.position;
                    if (Mathf.Abs(dir.x) > 0.0f)
                    {
                        newPos.x += Mathf.Sign(dir.x) * GameSession.gridUnit;
                    }

                    if (Mathf.Abs(dir.z) > 0.0f)
                    {
                        newPos.z += Mathf.Sign(dir.z) * GameSession.gridUnit;
                    }

                    if (player.CanMoveTo(newPos) && TheGame.Instance.blocks.ItemAt(newPos) != null && !closeToPlayer(newPos))
                    {
                        player.MoveTo(newPos, OnMovementCompleted);
                    }
                    else
                    {
                        timeToMove = 0.1f;
                    }
                }
            }
        }
	}

    bool closeToPlayer(Vector3 pos)
    {
        foreach (Player p in TheGame.Instance.players)
        {
            if (p.isBot) continue;
            if (p.DistanceTo(pos) < GameSession.gridUnit * 1.1f)
            {
                // move away
                return true;
            }
        }

        return false;
    }

    Vector3 currentPoint
    {
        get
        {
            Vector3 p = pathToFollow.points[targetPointIndex];
            p.y = player.transform.position.y;
            return p;
        }
    }

    void IncreaseIndexEventually()
    {
        if (pathToFollow == null) return;

        Vector3 d = player.position - currentPoint;
        if (d.sqrMagnitude < 0.1f)
        {
            targetPointIndex += patrolDirection;

            if (targetPointIndex < 0)
            {
                if (type == PatrolType.BackAndForth)
                {
                    targetPointIndex = 1;
                    patrolDirection *= -1;
                }
                else
                    if (type == PatrolType.Loop)
                    {
                        targetPointIndex = pathToFollow.points.Length - 1;
                    }
                    else
                    {
                        targetPointIndex = 0;
                    }
            }
            else
                if (targetPointIndex == pathToFollow.points.Length)
                {
                    if (type == PatrolType.BackAndForth)
                    {
                        targetPointIndex = pathToFollow.points.Length - 2;
                        patrolDirection *= -1;
                    }
                    else
                        if (type == PatrolType.Loop)
                        {
                            targetPointIndex = 0;
                        }
                        else
                        {
                            targetPointIndex = pathToFollow.points.Length - 1;
                        }
                }
        }
    }

    void OnMovementCompleted(Player p)
    {
        int t = targetPointIndex;
        IncreaseIndexEventually();

        timeToMove = durationBetweenMoves;

        if(targetPointIndex != t) // new control point
        {
            timeToMove += pathToFollow.delays[t];
        }

        Debug.Log("Follow point" + targetPointIndex.ToString());
    }

    AIWaypoints getClosestPath()
    {
        if(TheGame.Instance == null)
        {
            return null;
        }

        AIWaypoints[] aiws = TheGame.Instance.blocks.GetBlocksOfType<AIWaypoints>();

        float minDist = GameSession.gridUnit * 1000.0f;
        AIWaypoints closest = null;

        foreach(AIWaypoints aiw in aiws)
        {
            aiw.UpdatePoints();

            Vector3 d = transform.position - aiw.points[0]; d.y = 0.0f;
            if(d.sqrMagnitude < minDist)
            {
                closest = aiw;
                minDist = d.sqrMagnitude;
                patrolDirection = 1;
            }

            d = transform.position - aiw.points[aiw.points.Length - 1]; d.y = 0.0f;
            if (d.sqrMagnitude < minDist)
            {
                closest = aiw;
                minDist = d.sqrMagnitude;
                patrolDirection = -1;
            }
        }

        float limit = GameSession.gridUnit * GameSession.gridUnit * 5.0f * 5.0f;

        closest = minDist < limit ? closest : null;

        if(closest != null)
        {
            if (patrolDirection == 1)
                targetPointIndex = 0;
            else
                targetPointIndex = closest.points.Length - 1;
        }

        return closest;
    }

    public override void Reset()
    {
        base.Reset();

        pathToFollow = getClosestPath();

        IncreaseIndexEventually();
    }
}
