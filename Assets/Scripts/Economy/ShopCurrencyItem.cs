using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShopCurrencyItem : Widget {

    public Text text;
    public Text ammountText;

	protected override void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
	}

    GameEconomy.CurrencyItem _item = null;
    public GameEconomy.CurrencyItem item
    {
        get
        {
            return _item;
        }
        set
        {
            _item = value;
            Refresh();
        }
    }

    void Refresh()
    {
        if(_item == null)
        {
            text.text = "NA";
            ammountText.text = "NA";
        }
        else
        {
            text.text = _item.sProductFormattedPrice.ToString();
            ammountText.text = _item.sProductName;
        }
    }
}
