using UnityEngine;
using System.Collections;

public class VisibilityChangeOnState : MonoBehaviour
{
    public Transform onNode;
    public Transform offNode;

    void Start()
    {
        Block b = GetComponent<Block>();
        if (b != null)
        {
            b.OnStateChanged += OnBlockStateChanged;
            OnBlockStateChanged(Block.BlockState.State_Undefined, b.state);
        }
    }

    void OnBlockStateChanged(Block.BlockState prev, Block.BlockState current)
    {
        onNode.gameObject.SetActive(current == Block.BlockState.State_On);
        offNode.gameObject.SetActive(!onNode.gameObject.activeSelf);
    }
}
