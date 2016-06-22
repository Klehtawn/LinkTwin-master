using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class WorkshopItemButton : Widget {

    public Image image;
    public Text text;
    public Image select;

    public bool manageChildrenSelect = true;

    private GameObject _source;
    public GameObject source
    {
        set
        {
            _source = value;
            image.sprite = _source.GetComponent<IconRepresentation>().icon;
            text.text = _source.name;
        }

        get
        {
            return _source;
        }
    }

    private bool _selected = false;
    private bool firstTime = true;

    public Action<WorkshopItemButton> OnItemSelected;
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
                firstTime = false;

                select.gameObject.SetActive(_selected);

                if (transform.parent != null && _selected && manageChildrenSelect)
                {
                    for (int i = 0; i < transform.parent.childCount; i++)
                    {
                        WorkshopItemButton wib = transform.parent.GetChild(i).GetComponent<WorkshopItemButton>();
                        if (wib == null) continue;
                        if (wib != this)
                            wib.selected = false;
                    }
                }

                if (OnItemSelected != null)
                    OnItemSelected(this);
            }
        }
    }

	// Use this for initialization
    protected override void Start()
    {
        base.Start();
        selected = false;
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
	}
}
