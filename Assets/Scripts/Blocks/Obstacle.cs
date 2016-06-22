using UnityEngine;
using System.Collections;

[SelectionBase]
public class Obstacle : Block {

	// Use this for initialization
	public override void Start () {

        base.Start();
	
	}
	
	// Update is called once per frame
	public  override void Update () {

        base.Update();
	
	}

    protected override void StateChanged(BlockState prev, BlockState current)
    {
        //EnableRendering(current == BlockState.State_On);
    }
}
