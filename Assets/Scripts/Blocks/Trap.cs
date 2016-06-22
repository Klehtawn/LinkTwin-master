using UnityEngine;
using System.Collections;

public class Trap : Block {
	// Use this for initialization

    FallingTriangles fallingTriangles;

	public override void Start ()
    {
        fallingTriangles = GetComponentInChildren<FallingTriangles>();

        OnPlayerHit += OnPlayerEntered;
	}

    void OnPlayerEntered(Player p)
    {
        if (fallingTriangles.isEffectStarted == false)
        {
            fallingTriangles.StartEffect(true);
        }
    }

    public override void Update ()
    {
        base.Update();

	    if(playersOnSite.Count > 0 && state == BlockState.State_On)
        {
            foreach (Player p in playersOnSite)
                p.Die(Player.DieMode.Trap);
        }
	}

    protected override void StateChanged(BlockState prev, BlockState current)
    {
        if(current == BlockState.State_On)
        {
            if (playersOnSite.Count > 0)
            {
                foreach (Player p in playersOnSite)
                    p.Die(Player.DieMode.Trap);
            }

            getRoot();
            if (root.blocks.ground.Count == 0)
                root.RefreshStructure();


            //fallingTriangles.Resume();
            fallingTriangles.StartEffect(false);

            Destroy(gameObject, 1.5f);
        }
    }

    public override void OnGamePreStart()
    {
        base.OnGamePreStart();
        getRoot();
        Block ground = root.blocks.GroundAt(position);
        if (ground != null)
            Destroy(ground.gameObject);
    }
}
