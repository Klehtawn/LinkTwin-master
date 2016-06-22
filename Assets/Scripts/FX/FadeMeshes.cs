using UnityEngine;
using System.Collections;

public class FadeMeshes : MonoBehaviour {

    public MeshRenderer mesh1;
    public MeshRenderer mesh2;

    [Range(0.0f, 1.0f)]
    public float targetFactor = 0.0f;

    private Color color1, color2;

    int colorUniform;

    float factor = 0.0f;

	void Start () {

        color1 = mesh1.material.color;
        color2 = mesh2.material.color;

        colorUniform = Shader.PropertyToID("_Color");

        factor = targetFactor;

        UpdateFromFactor();
	}
	
    public void SetFactor(float f)
    {
        targetFactor = f;
    }

    void Update()
    {
        if(Mathf.Abs(factor - targetFactor) > 0.001f)
        {
            factor = Mathf.Clamp01(Mathf.Lerp(factor, targetFactor, 10.0f * Time.deltaTime));
            UpdateFromFactor();
        }
        else
        {
            if(factor != targetFactor)
            {
                factor = targetFactor;
                UpdateFromFactor();
            }
        }
    }

    void UpdateFromFactor()
    {
        Color c1 = color1;
        c1.a = 1.0f - factor;

        Color c2 = color2;
        c2.a = factor;

        mesh1.material.SetColor(colorUniform, c1);
        mesh2.material.SetColor(colorUniform, c2);
    }
}
