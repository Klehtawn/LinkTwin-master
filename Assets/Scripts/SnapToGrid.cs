using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class SnapToGrid : MonoBehaviour {

    public bool snapToGrid      = true;

    public bool snapOnGround    = false;

//	private Vector3 curPosition;
    private bool hasChanged = false;

    public bool useLocal = false;

    public bool snapToUnityGrid = false;

    Collider myCollider = null;

    void Update()
    {
        if (snapToGrid)
        {
            hasChanged = true; //(curPosition - transform.position).sqrMagnitude > 0.1f;

            if (hasChanged)
            {
                if (useLocal)
                {
                    transform.localPosition = GetGridPosition(transform.localPosition);
//                    curPosition = transform.localPosition;
                }
                else
                {
                    transform.position = GetGridPosition(transform.position);
//                    curPosition = transform.position;
                }

                hasChanged = false;
            }
        }
    }

    public static Vector3 Snap(Vector3 to, bool local = false)
	{
		Vector3 gridPos = to;

        float sx = Mathf.Sign(to.x);
		float sz = Mathf.Sign(to.z);

        float halfGrid = GameSession.gridUnit * 0.5f;

        if (local == false)
        {
            gridPos.x = Mathf.Round(Mathf.Abs(to.x) / GameSession.gridUnit - 0.5f) * GameSession.gridUnit + halfGrid;
            gridPos.z = Mathf.Round(Mathf.Abs(to.z) / GameSession.gridUnit - 0.5f) * GameSession.gridUnit + halfGrid;
        }
        else
        {
            gridPos.x = Mathf.Round(Mathf.Abs(to.x) / GameSession.gridUnit) * GameSession.gridUnit;
            gridPos.z = Mathf.Round(Mathf.Abs(to.z) / GameSession.gridUnit) * GameSession.gridUnit;
        }

        gridPos.x *= sx;
        gridPos.z *= sz;

		return gridPos;
	}

    public static void SnapToGround(Block block)
    {
        BoxCollider myCollider = block.GetComponent<BoxCollider>();
        Vector3 pos = block.transform.position;

        Block b = null;
        
        if(TheGame.Instance != null)
            b = TheGame.Instance.blocks.GroundAt(block.transform.position);
        else
        {
            LevelRoot lr = block.GetComponentInParent<LevelRoot>();
            if(lr != null)
            {
                b = lr.blocks.GroundAt(block.transform.position);
            }
        }
        if (b != null)
        {
            if (b.GetComponent<Collider>() != null && myCollider != null)
            {
                float yDiff = myCollider.bounds.extents.y + b.GetComponent<Collider>().bounds.extents.y;
                pos.y = b.transform.position.y + yDiff;
            }
        }

        block.transform.position = pos;
    }

	public Vector3 GetGridPosition(Vector3 to)
	{
		Vector3 gridPos = Snap (to, useLocal);
		gridPos.y = to.y;

        if(snapOnGround)
        {
#if UNITY_EDITOR
            Block b = null;
            if(Application.isPlaying && TheGame.Instance != null)
                b = TheGame.Instance.blocks.GroundAt(to);
            if(b != null)
            {
                if(b.GetComponent<Collider>() != null && myCollider != null)
                {
                    float yDiff = myCollider.bounds.extents.y + b.GetComponent<Collider>().bounds.extents.y;
                    gridPos.y = b.transform.position.y + yDiff;
                }
            }
            else
#endif
            {
                if(myCollider != null)
                    gridPos.y = myCollider.bounds.extents.y;
            }
        }

        return gridPos;
    }

    void Start()
    {
        myCollider = GetComponentInChildren<Collider>();
    }
 }