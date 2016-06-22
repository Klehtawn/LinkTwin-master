using UnityEngine;
using System.Collections;

public class TranslateOnChangeState : MonoBehaviour {

    public Transform target;

    public Vector3 onStatePosition;
    public Vector3 offStatePosition;

    public float duration = 0.3f;
	
	void Start () {
        Block b = GetComponent<Block>();
        b.OnStateChanged += OnBlockStateChanged;
	}

    void OnBlockStateChanged(Block.BlockState prev, Block.BlockState current)
    {
        Vector3 t = current == Block.BlockState.State_On ? onStatePosition : offStatePosition;
        LeanTween.moveLocal(target.gameObject, t, duration).setEase(LeanTweenType.linear);
    }
}
