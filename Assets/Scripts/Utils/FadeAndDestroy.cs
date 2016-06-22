using UnityEngine;
using System.Collections;

public class FadeAndDestroy : MonoBehaviour {

    public float duration = 0.0f;
    public float delay = 0.0f;
    private float timer = 0.0f;
    private CanvasGroup canvasGroup;
    private float initialAlpha = 1.0f;
	void Start () {

        timer = duration + delay;

        if (duration < 0.001f)
            Destroy(gameObject);
        else
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            initialAlpha = canvasGroup.alpha;
        }
	}
	
	// Update is called once per frame
	void Update () {

        if (duration <= 0.0f) return;

        timer -= Time.deltaTime;
        if (timer <= 0.0f)
            Destroy(gameObject);
        else
        {
            if(timer <= duration)
                canvasGroup.alpha = initialAlpha * Mathf.Pow(Mathf.Clamp01(timer / duration), 2.0f);
        }
	}

    public static void Destroy(GameObject o, float delay)
    {
        if (o == null) return;

        FadeAndDestroy fad = o.GetComponent<FadeAndDestroy>();
        if (fad == null)
            fad = o.AddComponent<FadeAndDestroy>();
        fad.duration = delay;
    }
}
