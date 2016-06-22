using UnityEngine;
using System.Collections;

public class TweenAnimation : MonoBehaviour {

    public Vector3 initialOffset = Vector3.zero;
    public Vector3 translateOffset = Vector3.zero;
    public float duration = 0.5f;
    public float delay = 0.0f;
    public LeanTweenType interpolation = LeanTweenType.linear;

    private Vector3 initialPosition;

	void Start ()
    {
        initialPosition = transform.position;
        DoAnimation();
	}

    void OnValidate()
    {
        //LeanTween.cancel(gameObject);
        //DoAnimation();
    }

    void DoAnimation()
    {
        transform.position = initialPosition + initialOffset;
        LeanTween.move(gameObject, initialPosition + translateOffset, duration).setEase(interpolation).setFrom(initialPosition + initialOffset).setDelay(delay);
    }
}
