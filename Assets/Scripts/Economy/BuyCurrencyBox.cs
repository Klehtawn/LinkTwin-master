using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BuyCurrencyBox : MessageBox {

    public Text titleText;
    public GameObject itemPrefab;

    RectTransform itemsRoot;

	// Use this for initialization
	protected override void Start () {
        base.Start();

        itemsRoot = transform.Find("Items").GetComponent<RectTransform>();

        GameEconomy.Initialize();

        Fill();

        EconomyBarModal.Show(this);
	}

    bool repositioned = false;
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();
        titleText.text = title;

        if(message.text.Length == 0 && repositioned == false)
        {
            //itemsRoot.anchorMin = new Vector2(0.0f, 0.4f);
            //itemsRoot.anchorMax = new Vector2(1.0f, 0.7f);
            repositioned = true;
        }
	}

    void Fill()
    {
        List<GameEconomy.CurrencyItem> list = GameEconomy.GetCurrencyItems();
        
        for(int i = 0; i < list.Count; i++)
        {
            GameObject o = GameObject.Instantiate<GameObject>(itemPrefab);
            o.transform.SetParent(itemsRoot);
            o.transform.localScale = Vector3.one;
            o.GetComponent<ShopCurrencyItem>().item = list[i];

            Widget w = o.GetComponent<Widget>();
            w.OnClick += _OnItemClicked;
        }
    }

    void _OnItemClicked(MonoBehaviour sender, Vector2 pos)
    {
        ShopCurrencyItem sci = sender.GetComponent<ShopCurrencyItem>();
        GameEconomy.BuyCurrency(sci.item);
        Close();
    }
}
