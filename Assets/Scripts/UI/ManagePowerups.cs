using UnityEngine;
using System.Collections;
using System;

public class ManagePowerups : MonoBehaviour {

	// Use this for initialization
	void Start () {

        for (int i = 0; i < transform.childCount; i++)
        {
            PowerupButton pb = transform.GetChild(i).GetComponent<PowerupButton>();
            pb.OnItemSelected += OnPowerupButtonPressed;
        }

        Refresh();
	}
	
	// Update is called once per frame
	void Update () {
        for (int i = 0; i < transform.childCount; i++)
        {
            PowerupButton pb = transform.GetChild(i).GetComponent<PowerupButton>();
            pb.selected = TheGame.Instance.powerupActive == pb.powerupType;
        }
	}

    public void Refresh()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            PowerupButton pb = transform.GetChild(i).GetComponent<PowerupButton>();
            pb.ammount = GameEconomy.boughtAmmount(pb.powerupType);
        }
    }

    public Action<GameEconomy.EconomyItemType, bool> OnPowerupSelected;
    void OnPowerupButtonPressed(PowerupButton sender)
    {
        if(GameEconomy.boughtAmmount(sender.powerupType) == 0)
        {
            // no powerups
            // go to shop

            if(sender.selected)
                OnPowerupSelected(GameEconomy.EconomyItemType.Invalid, false);
        }
        else
        if (OnPowerupSelected != null)
            OnPowerupSelected(sender.powerupType, sender.selected);
    }
}
