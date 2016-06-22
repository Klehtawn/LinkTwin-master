using UnityEngine;
using System.Collections;

public class FadeInactive : MonoBehaviour {

    CanvasGroup canvasGroup;
    Widget widget;

    public float inactiveAlpha = 0.35f;
    public bool disableWhenInactive = false;
    public float fadeSpeed = 7.0f;
	void Start ()
    {
        widget = GetComponent<Widget>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = widget.active ? 1.0f : inactiveAlpha;
        targetAlpha = canvasGroup.alpha;

        widget.OnWidgetActivated += OnWidgetActivated;
	}

    void Update()
    {
        if (canvasGroup.alpha != targetAlpha)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

            if (Mathf.Abs(canvasGroup.alpha - targetAlpha) < 0.01f)
                canvasGroup.alpha = targetAlpha;

            if(canvasGroup.alpha == 0.0f && disableWhenInactive)
            {
                gameObject.SetActive(false);
            }
        }
    }

    float targetAlpha = 0.0f;
    void OnWidgetActivated(Widget sender)
    {
        if(widget.active && disableWhenInactive)
        {
            widget.gameObject.SetActive(true);
        }
        targetAlpha = widget.active ? 1.0f : inactiveAlpha;
    }
}
