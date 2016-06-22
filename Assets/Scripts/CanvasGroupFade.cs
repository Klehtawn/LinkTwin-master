using UnityEngine;
using System;
using System.Collections;

public class CanvasGroupFade : MonoBehaviour {

    CanvasGroup canvasGroup;
    public Action OnCompleted;

    public float initialAlpha = 0.0f;
    public float finalAlpha = 1.0f;

    private float currentAlpha = 0.0f;

    public float delay = 0.0f;

    // Use this for initialization
    void Start () {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            canvasGroup.alpha = initialAlpha;
            currentAlpha = initialAlpha;
        }
    }

    public void SetInitialAlpha(float a)
    {
        if(canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        initialAlpha = a;
        canvasGroup.alpha = a;
        currentAlpha = a;
        enabled = true;
        delay = 0.0f;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (delay > 0.0f)
        {
            delay -= Time.deltaTime;
            return;
        }

        currentAlpha = Mathf.Lerp(currentAlpha, finalAlpha, Mathf.Clamp01(Time.deltaTime * 4.5f));
        canvasGroup.alpha = currentAlpha;
        if (Mathf.Abs(canvasGroup.alpha - finalAlpha) < 0.01f)
        {
            if (OnCompleted != null)
                OnCompleted();
            enabled = false;
        }
    }
}
