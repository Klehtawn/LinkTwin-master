using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class SliderBarIndicator : MonoBehaviour {

    public Text caption;

    RectTransform parentRect, myRect;

    [Range(0.0f, 1.0f)]
    public float value = 1.0f;
    public float valueScaling = 1.0f;
    public bool valueIsFloat = true;

	// Use this for initialization
	void Start () {
        parentRect = transform.parent.GetComponent<RectTransform>();
        myRect = transform.GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 p = myRect.anchoredPosition;
        float newX = parentRect.rect.width * value;

        if (Mathf.Abs(newX - p.x) > 0.005f)
        {
            p.x = Mathf.Lerp(p.x, newX, Time.deltaTime * 5.0f);
        }
        else
        {
            p.x = newX;
        }

        myRect.anchoredPosition = p;

        float v = value * valueScaling;
        if (valueIsFloat)
            caption.text = v.ToString();
        else
            caption.text = Mathf.RoundToInt(v).ToString();
	}
}
