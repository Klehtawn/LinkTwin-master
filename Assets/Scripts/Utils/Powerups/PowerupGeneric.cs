using UnityEngine;
using System.Collections;
using System;

public class PowerupGeneric : MonoBehaviour {

    protected Player player;
    public GameEconomy.EconomyItemType type = GameEconomy.EconomyItemType.Invalid;

	// Use this for initialization
	protected virtual void Start ()
    {
        player = GetComponentInParent<Player>();
	}
	
	// Update is called once per frame
    protected virtual void Update()
    {
	
	}

    public virtual bool PlayerCanMove()
    {
        return true;
    }

    public virtual void OnPlayerAction(Player.PlayerAction pa)
    {

    }

    public Action<PowerupGeneric> OnPowerupDestroyed;

    void OnDestroy()
    {
        player.RemovePowerup(type);
    }
}
