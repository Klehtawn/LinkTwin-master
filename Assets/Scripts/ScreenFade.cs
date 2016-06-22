using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

public class ScreenFade : MonoBehaviour {

    public float initialAlpha = 0.0f;
    public float finalAlpha = 1.0f;

    public float delay = 0.0f;

    public Text message;
    // Use this for initialization

    CanvasGroup canvasGroup;

    public bool dontDestroy = false;

    public Action OnCompleted;
    
    void Start () {

        canvasGroup = GetComponentInChildren<CanvasGroup>();
        canvasGroup.alpha = initialAlpha;
	}

    // Update is called once per frame
    bool finalized = false;
	void Update () {

        delay -= Time.deltaTime;
        if (delay > 0.0f)
            return;

        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, finalAlpha, Time.deltaTime * 4.5f);
        if(Mathf.Abs(canvasGroup.alpha - finalAlpha) < 0.01f && finalized == false)
        {
            if (OnCompleted != null)
                OnCompleted();

            if(dontDestroy == false)
                Destroy(gameObject);

            finalized = true;
        }
	}

    static public ScreenFade StartFade(GameObject fader, float initialAlpha, float targetAlpha, Action OnCompleted)
    {
        GameObject obj = GameObject.Instantiate<GameObject>(fader);
        ScreenFade fade = obj.GetComponent<ScreenFade>();
        fade.initialAlpha = initialAlpha;
        fade.finalAlpha = targetAlpha;
        fade.OnCompleted = OnCompleted;
        return fade;
    }

    static public void StartFade(GameObject fader, float initialAlpha, float targetAlpha, string message, float delay, Action OnCompleted)
    {
        GameObject obj = GameObject.Instantiate<GameObject>(fader);
        ScreenFade fade = obj.GetComponent<ScreenFade>();
        fade.initialAlpha = initialAlpha;
        fade.finalAlpha = targetAlpha;
        fade.OnCompleted = OnCompleted;
        fade.message.text = message;
        fade.delay = delay;
    }
}
