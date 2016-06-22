using UnityEngine;
using System.Collections;

public class FadeOnCreate : MonoBehaviour {

    public float duration = 0.0f;
    public float alpha = 1.0f;

    private float timer = 0.0f;
    private CanvasGroup canvasGroup;

	void Start () {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0.0f;

        timer = duration;
	}
	
	// Update is called once per frame
	void Update () {

        if (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            float t = 1.0f - Mathf.Pow(Mathf.Clamp01(timer / duration), 2.0f);
            canvasGroup.alpha = alpha * t;
        }
	}

    public static void Fade(GameObject o, float delay)
    {
        if (o == null) return;

        if (delay > 0.001f)
        {
            FadeOnCreate foc = o.GetComponent<FadeOnCreate>();
            if (foc == null)
                foc = o.AddComponent<FadeOnCreate>();
            foc.duration = delay;
            foc.Start();
        }
    }
}
