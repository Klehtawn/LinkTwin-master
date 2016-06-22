using UnityEngine;
using System.Collections;

public class ColorMe : MonoBehaviour
{
    public Gradient rainbow = new Gradient();

    public float duration = 0.5f;

    private float timer = 0.0f;

    Renderer myRenderer;

    public Color modulate = Color.white;

	void Start()
	{
        myRenderer = GetComponent<Renderer>();
        if(myRenderer != null)
            myRenderer.material.color = rainbow.Evaluate(0.0f);
	}
	
	void Update()
	{
        if (myRenderer == null)
        {
            myRenderer = GetComponent<Renderer>();
        }

        if (myRenderer == null)
            return;

        timer += Time.deltaTime;
        if (timer >= duration)
            timer -= duration;

        myRenderer.material.color = rainbow.Evaluate(Mathf.Clamp01(timer / duration)) * modulate;
	}
}
