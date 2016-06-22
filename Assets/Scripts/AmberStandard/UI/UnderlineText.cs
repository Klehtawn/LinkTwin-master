using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class UnderlineText : MonoBehaviour {

    public Text text;

    Image image;

    RectTransform textRectTransform = null;
    RectTransform imageRectTransform = null;

    ThemedElement themedElement = null;

    public float verticalOffset = 0.4f;
    public float horizontalMargins = 1.0f;
    public float weight = 0.1f;
	
	void Start () {

        image = gameObject.GetComponent<Image>();
        if(image == null)
            image = gameObject.AddComponent<Image>();

        themedElement = GetComponent<ThemedElement>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (image == null || text == null) return;

        if(themedElement == null && Application.isPlaying == false)
            themedElement = GetComponent<ThemedElement>();

        if (themedElement == null)
        {
            Color c = text.color;
            c.a *= 0.7f;
            image.color = c;
        }

        if (textRectTransform == null)
            textRectTransform = text.GetComponent<RectTransform>();

        if (imageRectTransform == null)
            imageRectTransform = image.GetComponent<RectTransform>();

        imageRectTransform.sizeDelta = new Vector2(text.preferredWidth + (float)text.font.fontSize * 0.5f * horizontalMargins, (float)text.font.fontSize * weight);
        imageRectTransform.anchoredPosition = new Vector2(0.0f, -text.preferredHeight * verticalOffset);
	}
}
