using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IconWithAmmount : MonoBehaviour {

    public Text ammountText;
    public Image icon;
	// Use this for initialization


    private int _ammount = 0;
    public int ammount
    {
        set
        {
            _ammount = value;
            if (_ammount > 0)
            {
                ammountText.text = _ammount.ToString();
                //icon.GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, 0.75f);
            }
            else
            {
                ammountText.text = "";
                //icon.GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, 1.0f);
            }
        }
        get
        {
            return _ammount;
        }
    }

    public Sprite sprite
    {
        get
        {
            return icon.sprite;
        }
        set
        {
            icon.sprite = value;
        }
    }
}
