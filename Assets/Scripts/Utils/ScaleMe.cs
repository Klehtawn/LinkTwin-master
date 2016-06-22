using UnityEngine;
using System.Collections;

public class ScaleMe : MonoBehaviour
{
    public Vector3 axis = new Vector3(1.0f, 1.0f, 1.0f);

    public float initialScale = 1.0f;
    public float finalScale = 0.0001f;

    public float duration = 0.5f;

    private float timer = 0.0f;
	void Start()
	{
	}
	
	void Update()
	{
        timer += Time.deltaTime;
        if (timer >= duration)
            timer -= duration;

        transform.localScale = Mathf.Lerp(initialScale, finalScale, Mathf.Clamp01(timer / duration)) * axis;
	}
}
