using UnityEngine;
using System.Collections;

public class ButtonPressEffect : MonoBehaviour {

    bool assigned = false;
    void Update()
    {
        if(assigned == false)
        {
            assigned = true;
            Widget w = GetComponent<Widget>();
            if (w != null)
            {
                w.OnTouchDown += OnButtonPress;
                w.OnTouchUp += OnButtonReleased;
            }
        }
    }

    void OnDestroy()
    {
        Widget w = GetComponent<Widget>();
        if (w != null)
        {
            w.OnTouchUp -= OnButtonPress;
            w.OnTouchDown -= OnButtonReleased;
        }
    }
	
	void OnButtonPress(MonoBehaviour sender, Vector2 p)
    {
        OnButtonPress();
    }
    void OnButtonReleased(MonoBehaviour sender, Vector2 p)
    {
        OnButtonReleased();
    }

    protected virtual void OnButtonPress()
    {

    }

    protected virtual void OnButtonReleased()
    {

    }

    public virtual void CopyFrom(ButtonPressEffect other)
    {

    }
}
