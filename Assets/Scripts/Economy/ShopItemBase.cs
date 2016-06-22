using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ShopItemBase : Widget {

    [Serializable]
    public class ItemDesc
    {
        public Sprite icon;
        public string ammount;
    };

    public ItemDesc[] itemsDesc;

    public SimpleIconDef[] itemObjects;

    public Text priceText;

    [NonSerialized]
    public GameEconomy.EconomyItem shopItem;

	// Use this for initialization
	protected override void Start () {

        base.Start();

        if (itemObjects.Length > itemsDesc.Length)
        {
            for (int i = itemsDesc.Length; i < itemObjects.Length; i++)
            {
                itemObjects[i].gameObject.SetActive(false);
            }
        }

        for(int i = 0; i < itemsDesc.Length; i++)
        {
            if (i >= itemObjects.Length) break;
            SimpleIconDef sid = itemObjects[i].GetComponent<SimpleIconDef>();
            sid.icon.sprite = itemsDesc[i].icon;
            sid.caption.text = itemsDesc[i].ammount;
        }
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
	}
}
