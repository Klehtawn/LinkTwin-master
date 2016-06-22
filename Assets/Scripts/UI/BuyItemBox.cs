using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BuyItemBox : MessageBox {

    public Widget cancelButton;

    public Text titleMessage;

	// Use this for initialization
    protected override void Start()
    {
        base.Start();

        UpdateWindowParameters();

        okButton.OnClick += OnOkButton;
        cancelButton.OnClick += OnCancelButton;

        title = "BUY ITEM";

        EconomyBarModal.Show(this);
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();

        titleMessage.text = title;
	}

    void OnOkButton(MonoBehaviour sender, Vector2 p)
    {
        GameEconomy.Buy(_item);
    }

    void OnCancelButton(MonoBehaviour sender, Vector2 p)
    {
        Close();
    }

    GameEconomy.EconomyItem _item = null;
    public GameEconomy.EconomyItem item
    {
        set
        {
            _item = value;
            Refresh();
        }

        get
        {
            return _item;
        }
    }

    void Refresh()
    {
        string s = Locale.GetString("BUY_ITEM_WITH_RUNES");
        s = s.Replace("{1}", item.price.ToString());

        string itemStr = item.ammount.ToString() + " " + item.displayName; // should be localized
        s = s.Replace("{0}", itemStr);

        message.text = s;
    }
}
