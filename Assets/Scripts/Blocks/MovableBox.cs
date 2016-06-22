using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MovableBox : Block {

    SnapToGrid snap;
	public override void Start () {
        base.Start();

        snap = GetComponent<SnapToGrid>();
	}

    //Vector3 lastMove = Vector3.zero;
	
	// Update is called once per frame

    Vector3 pushingDirection;
    float pushingDistance;

    public override void Update()
    {
        base.Update();

        if (behaviourActive == false) return;

        if(pushingPlayer)
        {
            Vector3 pushDir = pushingPlayer.DirectionTo(position);

            Vector3 p = pushingPlayer.position + pushDir * GameSession.gridUnit;
            p.y = position.y;
            position = p;

            Portal portal = root.blocks.ItemAt<Portal>(p);
            if (portal == null)
                PushNextBox(pushDir);
        }

        if(isCustomMove)
        {
            PushNextBox(customMovingDir);
        }

        Block underneath = root.blocks.GroundAt(position);
        if (underneath == null)
            Die();
	}

    void Die()
    {
        LeanTween.scale(gameObject, Vector3.one * 0.001f, 0.4f).setEase(LeanTweenType.easeOutBack).setDelay(0.2f).setOnComplete(() => gameObject.SetActive(false));
    }

    public override bool CanBeWalkedOn(Vector3 fromDir)
    {
        if(fromDir.sqrMagnitude < 0.001f)
        {
            return true;
        }

        Vector3 newPos = position + fromDir * GameSession.gridUnit;

        Player p = TheGame.Instance.PlayerAt(newPos);
        if(p != null)
        {
            if (p.CanMoveTo(newPos + fromDir * GameSession.gridUnit) == false)
                return false;
        }

        Block b = root.blocks.ItemAt(newPos);
        if (b == null) return true;
        
        return b.CanBeWalkedOn(fromDir);
    }

    Player pushingPlayer = null;
    public void PushedByPlayer(Player p)
    {
        if( p != null)
        {
            GameEvents.Send(p.kind, GameEvents.EventType.PushingBox);
        }

        if(p != null)
        {
            p.pushedBoxes.Clear();
            snap.enabled = false;
        }
        else
        {
            if(pushingPlayer != null)
            {
                foreach(Block b in pushingPlayer.pushedBoxes)
                {
                    if (b != null)
                    {
                        b.position = SnapToGrid.Snap(b.position);
                        b.GetComponent<SnapToGrid>().enabled = true;
                    }
                }
            }
        }

        pushingPlayer = p;
        snap.enabled = p == null;

        if (p != null)
        {
            pushingDirection = position - p.position; pushingDirection.y = 0.0f;
            pushingDistance = pushingDirection.magnitude;
            pushingDirection.Normalize();
        }
    }

    void PushNextBox(Vector3 fromDir)
    {
        Vector3 newPos = position + fromDir * GameSession.gridUnit;

        MovableBox nextBox = root.blocks.ItemAt<MovableBox>(newPos);
        if(nextBox != null)
        {
            if(pushingPlayer)
            {
                if (pushingPlayer.pushedBoxes.Contains(nextBox) == false)
                {
                    pushingPlayer.pushedBoxes.Add(nextBox);
                }
            }

            PushPlayer(newPos + fromDir * GameSession.gridUnit);
         
            nextBox.snap.enabled = false;
            nextBox.ModifyPosition(newPos);

            Portal portal = root.blocks.ItemAt<Portal>(newPos);
            if(portal == null)
                nextBox.PushNextBox(fromDir);
        }
    }

    bool isCustomMove = false;
    Vector3 customMovingDir;
    Player pushedPlayer = null;

    public override bool ModifyPosition(Vector3 p, bool animated = false)
    {
        if (animated)
        {
            Vector3 newPos = p;
            newPos.y = position.y;
            isCustomMove = true;
            customMovingDir = p - position; customMovingDir.Normalize();

            if (PushPlayer(newPos) == false)
                return false;
            
            LeanTween.move(gameObject, p, 0.4f).setEase(LeanTweenType.easeInOutBack).setOnComplete(() =>
               {
                   isCustomMove = false;
                   pushedPlayer = null;
               });
        }
        else
        {
            base.ModifyPosition(p);
        }

        return true;
    }

    bool PushPlayer(Vector3 newPos)
    {
        if (pushedPlayer != null) return true;

        pushedPlayer = TheGame.Instance.PlayerAt(newPos);
        if (pushedPlayer)
        {
            Vector3 dir = pushedPlayer.DirectionTo(position);
            if (pushedPlayer.CanMoveTo(newPos - dir * GameSession.gridUnit))
                pushedPlayer.MoveTo(newPos - dir * GameSession.gridUnit);
            else
            {
                pushedPlayer = null;
                return false;
            }
            Debug.Log("Pushing player!");
        }

        return true;
    }

    public Vector3 futurePosition
    {
        get
        {
            if (pushingPlayer == null) return position;
            return pushingPlayer.nextPosition + pushingDirection * pushingDistance;
        }
    }
}

