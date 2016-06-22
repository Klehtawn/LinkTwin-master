using UnityEngine;
using System.Collections;

public class PowerupFreeze : PowerupGeneric {

    protected override void Start()
    {
        base.Start();
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
	}

    public override bool PlayerCanMove()
    {
        return false;
    }

    public override void OnPlayerAction(Player.PlayerAction pa)
    {
        if (pa == Player.PlayerAction.PlayerFinishedMoving)
            Destroy(gameObject);
    }
}
