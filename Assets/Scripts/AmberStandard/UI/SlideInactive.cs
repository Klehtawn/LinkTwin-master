using UnityEngine;
using System.Collections;

public class SlideInactive : MonoBehaviour {

    public Vector2 direction = Vector2.down;
    public LeanTweenType tween = LeanTweenType.linear;
    public float slideDuration = 0.35f;

    Widget widget;
    Vector3 initialPosition = Vector3.one * -100000.0f;

	void Start ()
    {
        widget = GetComponent<Widget>();

        widget.OnWidgetActivated += OnWidgetActivated;

        OnWidgetActivated(widget);
	}

    void OnWidgetActivated(Widget sender)
    {
        if (initialPosition.x <= -100000.0f)
            initialPosition = transform.localPosition;
        LeanTween.cancel(gameObject);
        if(widget.active)
        {
            LeanTween.moveLocal(gameObject, initialPosition, slideDuration).setEase(tween);
        }
        else
        {
            Vector3 newPos = Desktop.main.DesktopToScreen(new Vector2(direction.x * widget.width, direction.y * widget.height)) - Desktop.main.DesktopToScreen(Vector2.zero);
            newPos += initialPosition;
            LeanTween.moveLocal(gameObject, newPos, slideDuration).setEase(tween);
        }
    }
}
