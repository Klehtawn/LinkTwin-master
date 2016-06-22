using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PowerupButton : Widget {

    public Text caption;
    public Image icon;
    public Image selectedBackground;

    public bool manageChildrenSelect = true;

    private bool _selected = false;
    private bool firstTime = true;

    public GameEconomy.EconomyItemType powerupType = GameEconomy.EconomyItemType.PowerupFreezeCharacter;

    public Action<PowerupButton> OnItemSelected;
    public bool selected
    {
        get
        {
            return _selected;
        }

        set
        {
            if (_selected != value || firstTime)
            {
                _selected = value;

                bool wasFirstTime = firstTime;
                firstTime = false;

                selectedBackground.gameObject.SetActive(_selected);

                if (transform.parent != null && _selected && manageChildrenSelect)
                {
                    for (int i = 0; i < transform.parent.childCount; i++)
                    {
                        PowerupButton pb = transform.parent.GetChild(i).GetComponent<PowerupButton>();
                        if (pb == null) continue;
                        if (pb != this)
                            pb.selected = false;
                    }
                }

                if (OnItemSelected != null && wasFirstTime == false)
                    OnItemSelected(this);
            }
        }
    }

	// Use this for initialization
    protected override void Start()
    {
        base.Start();
        selected = false;
        ammount = 0;

        OnClick += OnButtonClicked;
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
	}

    private int _ammount = -1;
    public int ammount
    {
        set
        {
            if (_ammount != value)
            {
                _ammount = value;
                caption.text = _ammount.ToString();
            }
        }
        get
        {
            return _ammount;
        }
    }

    void OnButtonClicked(MonoBehaviour sender, Vector2 p)
    {
        if (selected == false)
        {
            //if (_ammount > 0)
            {
                selected = true;
            }
        }
        else
        {
            selected = false;
        }
    }
}
