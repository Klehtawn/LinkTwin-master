using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class SimpleWidget : MonoBehaviour,  IPointerDownHandler, IPointerUpHandler
{
    public Action<MonoBehaviour> OnTouchDown;
    public Action<MonoBehaviour> OnTouchUp;

    public bool active = true;
    private bool prevActive = false;

    public float onPressScale = 1.0f;

    CanvasGroup canvasGroup;

    protected virtual void Start () {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = active ? 1.0f : 0.3f;
        prevActive = active;
    }
	
	// Update is called once per frame
    protected virtual void Update()
    {
	    if(canvasGroup != null && active != prevActive)
        {
            canvasGroup.alpha = active ? 1.0f : 0.3f;
            prevActive = active;
        }
	}

    public void OnPointerDown(PointerEventData ped)
    {
        if (active == false) return;
        if (OnTouchDown != null)
            OnTouchDown(this);

        LeanTween.scale(gameObject, Vector3.one * onPressScale, 0.5f).setEase(LeanTweenType.easeOutBack);
    }

    public void OnPointerUp(PointerEventData ped)
    {
        if (active == false) return;
        if (OnTouchUp != null)
            OnTouchUp(this);

        LeanTween.scale(gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutElastic);
    }
}
