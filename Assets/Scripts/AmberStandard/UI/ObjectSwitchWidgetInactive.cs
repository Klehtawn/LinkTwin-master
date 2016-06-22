using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ObjectSwitchWidgetInactive : MonoBehaviour
{
    public GameObject[] active;
    public GameObject[] inactive;

    Widget widget;

	// Use this for initialization
	void Awake () {

        widget = GetComponent<Widget>();
        widget.OnWidgetActivated += OnWidgetActivated;

        OnWidgetActivated(widget);
	}

    void OnWidgetActivated(Widget sender)
    {
        foreach (GameObject r in active)
        {
            r.SetActive(widget.active);
        }

        foreach (GameObject r in inactive)
        {
            r.SetActive(!widget.active);
        }
    }
}
