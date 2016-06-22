using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class WorkshopItems : Widget {

    public Widget items;
    public GameObject buttonSource;

    public WorkshopBlocksList blocksToLoad;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();
	}


    bool itemsFilled = false;
	protected override void Update () {

        base.Update();

        if(itemsFilled == false)
        {
            Fill();
            itemsFilled = true;
        }
	
	}

    void Fill()
    {
        Widget.DeleteAllChildren(items.transform);

        for(int i = 0; i < blocksToLoad.blocks.Length; i++)
        {
            if (blocksToLoad.blocks[i] == null) continue;

            GameObject but = GameObject.Instantiate<GameObject>(buttonSource);
            but.transform.SetParent(items.transform);
            but.transform.localScale = Vector3.one;
            but.name = blocksToLoad.blocks[i].name;

            WorkshopItemButton wib = but.GetComponent<WorkshopItemButton>();
            wib.pos = new Vector2(i * wib.width + 5.0f, 0.0f);
            wib.source = blocksToLoad.blocks[i].gameObject;
            wib.OnTouchUp += OnWorkshopItemTouchUp;
            wib.OnTouchDown += OnWorkshopItemTouchDown;
            wib.OnItemSelected += OnWorkshopItemSelected;
        }

        items.FitToChildren(new Vector2(5.0f, 0.0f));
    }

    WorkshopItemButton _selected = null;
    public WorkshopItemButton selected
    {
        get
        {
            return _selected;
        }
    }

    Vector2 pressedPosition;
    void OnWorkshopItemTouchDown(MonoBehaviour sender, Vector2 pos)
    {
        pressedPosition = pos;
    }

    public Action<WorkshopItemButton> OnItemSelected;

    void OnWorkshopItemTouchUp(MonoBehaviour sender, Vector2 pos)
    {
        if(Vector2.Distance(pos, pressedPosition) > 4.0f)
        {
            if (selected != null)
            {
                _selected.selected = false;
                _selected = null;

                if (OnItemSelected != null)
                    OnItemSelected(selected);
            }
            return;
        }

        WorkshopItemButton wib = sender.GetComponent<WorkshopItemButton>();
        Debug.Log("PRESSED: " + wib.text.text);

        wib.selected = !wib.selected;

        if (wib.selected)
            _selected = wib;
        else
            _selected = null;

        //if (OnItemSelected != null)
        //    OnItemSelected(selected);
    }

    void OnWorkshopItemSelected(WorkshopItemButton sender)
    {
        if (OnItemSelected != null)
            OnItemSelected(sender);
    }

    public void ClearSelection()
    {
        if (selected != null)
            selected.selected = false;
        _selected = null;
    }
}
