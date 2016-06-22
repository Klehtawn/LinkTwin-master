using UnityEngine;
using System.Collections;

public class SpriteChangeOnState : MonoBehaviour {

    public Renderer rendererToChange;

    public Sprite onSprite;
    public Sprite offSprite;

	void Start ()
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
        if(rendererToChange != null)
        {
            if(current == Block.BlockState.State_On && onSprite != null)
            {
                rendererToChange.material.SetTexture("_MainTex", onSprite.texture);
            }

            if (current == Block.BlockState.State_Off && offSprite != null)
            {
                rendererToChange.material.SetTexture("_MainTex", offSprite.texture);
            }
        }
    }
}
