using UnityEngine;
using System.Collections;

public class ShopIcon : MonoBehaviour {

    public GameEconomy.EconomyItemType type;
	
    public Sprite sprite
    {
        get
        {
            return GetComponent<SpriteRenderer>().sprite;
        }
    }

    public Sprite variant(string name)
    {
        SpriteRenderer[] variants = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer v in variants)
            if (v.gameObject.name == name)
                return v.sprite;

        return null;
    }
}
