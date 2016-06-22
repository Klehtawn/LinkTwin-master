using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class OnOffButton : Button {

    public bool isOn = true;
	// Use this for initialization

    public Transform dotOn;
    public Transform dotOff;

    public Transform showItemOnState;
    public Transform showItemOffState;

    protected override void Start()
    {
        base.Start();

        UpdateState();

        OnTouchUp += OnPressed;
        OnTouchMoved += OnCursorMoved;
	}

    public Action OnStateChanged;

    public void UpdateState()
    {
        if (OnStateChanged != null)
            OnStateChanged();

        if (icon != null && caption != null)
        {
            if (isOn)
            {
                iconAlign = TextAlignment.Right;
                textAlign = TextAlignment.Left;
                caption.text = Locale.GetString("ON");
            }
            else
            {
                iconAlign = TextAlignment.Left;
                textAlign = TextAlignment.Right;
                caption.text = Locale.GetString("OFF");
            }

            UpdateAlignment();
        }

        if (dotOn != null)
        {
            dotOn.gameObject.SetActive(isOn);
        }

        if(dotOff != null)
        {
            dotOff.gameObject.SetActive(!isOn);
        }

        if(showItemOnState != null)
        {
            showItemOnState.gameObject.SetActive(isOn);
        }

        if (showItemOffState != null)
        {
            showItemOffState.gameObject.SetActive(!isOn);
        }
    }

    protected override void OnValidate()
    {
        UpdateState();
    }

    protected override float getButtonBorder()
    {
        return 0.05f;
    }

    void OnPressed(MonoBehaviour sender, Vector2 p)
    {
     /*   if (!isOn && p.x < height) return;
        if (isOn && p.x > height) return;
        isOn = !isOn;
        UpdateState();*/
    }

    void OnCursorMoved(MonoBehaviour sender, Vector2 startPos, Vector2 currentPos)
    {
        float dx = currentPos.x - startPos.x;
        if (Mathf.Abs(dx) < height) return;

        if (dx > height && !isOn)
        {
            isOn = true;
            UpdateState();
            GameEvents.Send(GameEvents.EventType.ButtonPress);
        }

        if (dx < -height && isOn)
        {
            isOn = false;
            UpdateState();
            GameEvents.Send(GameEvents.EventType.ButtonPress);
        }
    }
}
