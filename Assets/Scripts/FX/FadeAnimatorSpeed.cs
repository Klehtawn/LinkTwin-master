using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class FadeAnimatorSpeed : MonoBehaviour {

    Animator animator;

    public float startValue = 0.0f;
    public float endValue = 1.0f;
    public float fadeSpeed = 6.0f;
    public float delay = 0.0f;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        animator.speed = startValue;
        delayTimer = delay;
	}

    float delayTimer = 0.0f;
	// Update is called once per frame
	void Update () {

        if (delayTimer <= 0.0f)
            animator.speed = Mathf.Lerp(animator.speed, endValue, fadeSpeed * Time.deltaTime);
        else
            delayTimer -= Time.deltaTime;
	}

    public void Fade(float startValue, float endValue, float delay = 0.0f)
    {
        this.startValue = startValue;
        this.endValue = endValue;
        this.delayTimer = delay;
    }
}
