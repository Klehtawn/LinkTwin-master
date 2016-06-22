using UnityEngine;
using System.Collections;

[SelectionBase]
[ExecuteInEditMode]
public class Door : Block
{
    public float movingDuration = 0.5f;

/*    Transform doorModel;
    BoxCollider collision;

    Vector3 previousPosition;

    public float autoCloseIn = -1.0f;*/
    
	public override void Awake()
    {
        base.Awake();
        blockType = BlockType.Door;

        /*doorModel = transform.Find("DoorModel");
        collision = GetComponent<BoxCollider>();

        previousPosition = doorModel.position;*/
    }

	public override void Start ()
    {
        base.Start();
	}
	
	// Update is called once per frame
	public override void Update ()
    {
        base.Update();

/*        CopyPositionToCollision();

        Vector3 movingVector = doorModel.position - previousPosition;
        previousPosition = doorModel.position;

        if (movingVector.sqrMagnitude > 0.0f && TheGame.Instance != null)
        {
            movingVector.Normalize();

            Vector3 front = doorModel.position + movingVector * GameSession.gridUnit;

            foreach (Player p in TheGame.Instance.players)
            {
                Vector3 dirToPlayer = p.transform.position - doorModel.position; dirToPlayer.y = 0.0f;
                float d = dirToPlayer.magnitude;
                dirToPlayer /= d;
                if (d < GameSession.gridUnit && Vector3.Dot(dirToPlayer, movingVector) > 0.99f)
                {
                    p.outsidePushVector = movingVector;
                    p.transform.position = front;

                    if (p.CanMoveTo(front + movingVector * GameSession.gridUnit * 0.5f) == false)
                    {
                        p.Die(Player.DieMode.Trap);
                    }
                }
            }
        }

        if(autoCloseTimer > 0.0f)
        {
            autoCloseTimer -= Time.deltaTime;
            if (autoCloseTimer < 0.0f)
                SetState(BlockState.State_On);
        }
 */
    }

    /*void CopyPositionToCollision()
    {
        if (collision != null && doorIsMoving && doorModel != null)
        {
            Vector3 pos = doorModel.localPosition;
            pos.y = collision.center.y;
            collision.center = pos;
        }
    }*/

    //bool doorIsMoving = false;
    //float autoCloseTimer = -1.0f;
    protected override void StateChanged(BlockState prev, BlockState current)
    {
        base.StateChanged(prev, current);

        navigable = current == BlockState.State_Off;

/*        if(doorModel != null)
        {
            Vector3 target = current == BlockState.State_On ? Vector3.zero : Vector3.right * GameSession.gridUnit;
            target.y = doorModel.localPosition.y;
            if (target != doorModel.localPosition)
            {
                doorIsMoving = true;
                LeanTween.moveLocal(doorModel.gameObject, target, movingDuration).setEase(LeanTweenType.easeInBack).setOnComplete(() => 
                {
                    CopyPositionToCollision();
                    doorIsMoving = false;
                });
            }
        }

        if(current == BlockState.State_Off && autoCloseIn > 0.0f)
        {
            autoCloseTimer = autoCloseIn;
        }*/

    }

    public override bool IsValidBlock()
    {
        return true;
    }

    public override void WriteSignificantInfo(ref System.IO.BinaryWriter bw)
    {
        base.WriteSignificantInfo(ref bw);

        //WriteSignificantValue(ref bw, "autocls", autoCloseIn);
        WriteSignificantValue(ref bw, "duration", movingDuration);
        WriteSignificantValue(ref bw, "roty", transform.rotation.eulerAngles.y);
    }

    public override void ReadSignificantInfo(ref System.IO.BinaryReader br)
    {
        base.ReadSignificantInfo(ref br);

        //autoCloseIn = ReadSignificantValueFloat(ref br, "autocls");
        movingDuration = ReadSignificantValueFloat(ref br, "duration");

        float roty = ReadSignificantValueFloat(ref br, "roty");
        transform.rotation = Quaternion.Euler(0.0f, roty, 0.0f);
    }
}
