using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MaterialByTransparency : MonoBehaviour {

    public Material opaqueMaterial;
    public Material transparentMaterial;

    private CanvasGroup canvasGroup;
    private Image image;
    private RawImage rawImage;
	void Start () {
        canvasGroup = GetComponentInParent<CanvasGroup>();
        image = GetComponent<Image>();
        rawImage = GetComponent<RawImage>();
	}
	
	// Update is called once per frame
	void Update () {
        if (canvasGroup != null && (image != null || rawImage != null))
        {
            Material m = opaqueMaterial;
            if (canvasGroup.alpha < 0.99f)
            {
                m = transparentMaterial;
            }

            if (image != null)
                image.material = m;
            else
                rawImage.material = m;
        }
	}
}
