using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaterialAlphaFromCanvasAlpha : MonoBehaviour {

    CanvasGroup canvasGroup;
    int colorUniform;
    new Renderer renderer;
    Color materialColor;
	void Awake () {
        canvasGroup = GetComponentInParent<CanvasGroup>();
        renderer = GetComponent<Renderer>();
        colorUniform = Shader.PropertyToID("_Color");

        if (renderer != null)
            materialColor = renderer.material.GetColor(colorUniform);

        CopyAlpha();
	}
	
	// Update is called once per frame
	void Update () {
        CopyAlpha();
	}

    void CopyAlpha()
    {
        if (renderer != null && canvasGroup != null)
        {
            if (materialColor.a != canvasGroup.alpha)
            {
                materialColor.a = canvasGroup.alpha;
                renderer.material.SetColor(colorUniform, materialColor);
            }
        }
    }
}
