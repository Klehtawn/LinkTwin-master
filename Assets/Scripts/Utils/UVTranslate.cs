using UnityEngine;
using System.Collections;

public class UVTranslate : MonoBehaviour
{
    public Vector2 speed = Vector2.right;

    public float loopAt = 60.0f;

    private Vector2 startValue, currentValue;
    private new Renderer renderer;

	// Use this for initialization
	void Start ()
    {
        renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            startValue = renderer.material.GetTextureOffset("_MainTex");
            currentValue = startValue;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(renderer != null && speed.sqrMagnitude > 0.0f)
        {
            currentValue += speed * Time.deltaTime;
            renderer.material.SetTextureOffset("_MainTex", currentValue);

            if (currentValue.x > loopAt)
                currentValue.x -= loopAt;

            if (currentValue.y > loopAt)
                currentValue.y -= loopAt;

            if (currentValue.x < -loopAt)
                currentValue.x += loopAt;

            if (currentValue.y < -loopAt)
                currentValue.y += loopAt;
        }
	}
}
