using UnityEngine;
using System.Collections;

public class FadeMaterial : MonoBehaviour {

    public Renderer rendererToChange;

    private int colorUniform;

    private Color value, targetValue;

	void Start () {
        colorUniform = Shader.PropertyToID("_Color");

        value = rendererToChange.material.GetColor(colorUniform);
        targetValue = value;
	}
	
	// Update is called once per frame
	void Update ()
    {
        Color diff = targetValue - value;
        if (Mathf.Abs(diff.r) > 0.001f || Mathf.Abs(diff.g) > 0.001f || Mathf.Abs(diff.b) > 0.001f || Mathf.Abs(diff.a) > 0.001f)
        {
            value = Color.Lerp(value, targetValue, Time.deltaTime * 4.0f);
            SetValue();
        }
        else
        {
            if (value != targetValue)
            {
                SetValue();
                value = targetValue;
            }
        }
	}

    void SetValue()
    {
        rendererToChange.material.SetColor(colorUniform, value);
    }

    public void FadeTo(Color from, Color to)
    {
        value = from;
        targetValue = to;
        SetValue();
    }
}
