using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Widget))]
public class SendEventOnClick : MonoBehaviour {

    public GameEvents.EventType Event = GameEvents.EventType.ButtonPress;

	// Use this for initialization
	void Start () {

        Widget w = GetComponent<Widget>();
        w.OnClick += OnWidgetClick;
	}

    void OnWidgetClick(MonoBehaviour mb, Vector2 p)
    {
        GameEvents.Send(Event);
    }
	
}
