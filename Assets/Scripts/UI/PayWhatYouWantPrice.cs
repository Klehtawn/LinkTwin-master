using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PayWhatYouWantPrice : MonoBehaviour {

    public Text price;
    public Text message;

    private GameEconomy.EconomyItem _shopItem;
    public GameEconomy.EconomyItem shopItem
    {
        get
        {
            return _shopItem;
        }
        set
        {
            _shopItem = value;
            price.text = _shopItem.priceStr;
            if(message.GetComponent<LocalizedText>() != null)
                message.GetComponent<LocalizedText>().text = _shopItem.displayName;
            else
                message.text = _shopItem.displayName;
        }
    }
}
