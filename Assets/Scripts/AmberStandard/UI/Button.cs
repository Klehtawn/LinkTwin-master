using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class Button : Widget
{
    public Text caption;
    public Image icon;

    public TextAlignment iconAlign = TextAlignment.Left;
    public TextAlignment textAlign = TextAlignment.Right;

    [Range(-0.2f, 0.2f)]
    public float iconOffsetY = 0.0f;
    [Range(-0.2f, 0.2f)]
    public float captionOffsetY = 0.0f;

    private RectTransform captionTransform = null;
    private RectTransform iconTransform = null;

	// Use this for initialization
	protected override void Awake () {
        base.Awake();

        if (icon != null)
        {
            iconTransform = icon.GetComponent<RectTransform>();
            iconTransform.sizeDelta = Vector2.zero;
            iconTransform.anchoredPosition = Vector2.zero;
        }

        if (caption != null)
        {
            captionTransform = caption.GetComponent<RectTransform>();
            captionTransform.sizeDelta = Vector2.zero;
        }

        UpdateAlignment();
	}
	
	// Update is called once per frame

    int firstFrame = 5;

	protected override void Update () {
        base.Update();

        if(firstFrame > 0)
        {
            firstFrame--;
            UpdateAlignment();
        }
	}

    protected virtual float getButtonBorder()
    {
        if (Desktop.main == null) return 0.0f;
        return 0.1f;
    }

    protected void UpdateAlignment()
    {
        float vertBorder = getButtonBorder();
        float horizBorder = vertBorder / aspect;
        float iconWidthPercents = (1.0f - 2.0f * vertBorder) / aspect;

        if(icon != null && iconTransform == null)
        {
            iconTransform = icon.GetComponent<RectTransform>();
        }
        

        if(icon != null && iconTransform != null && icon.gameObject.activeSelf)
        {
            if(iconAlign == TextAlignment.Left)
                iconTransform.anchorMin = new Vector2(horizBorder, vertBorder);
            if (iconAlign == TextAlignment.Right)
                iconTransform.anchorMin = new Vector2(1.0f - horizBorder - iconWidthPercents, vertBorder);
            if (iconAlign == TextAlignment.Center)
                iconTransform.anchorMin = new Vector2(0.5f - iconWidthPercents * 0.5f, vertBorder);

            iconTransform.anchorMax = new Vector2(iconWidthPercents + iconTransform.anchorMin.x, 1.0f - vertBorder);
            iconTransform.sizeDelta = Vector2.zero;
            iconTransform.anchoredPosition = Vector2.zero;

            iconTransform.anchorMin += new Vector2(0.0f, iconOffsetY);
            iconTransform.anchorMax += new Vector2(0.0f, iconOffsetY);
        }

        if (caption != null && captionTransform != null && caption.gameObject.activeSelf)
        {
            float wid = 1.0f - 2.0f * horizBorder;
            if (icon != null && icon.gameObject.activeSelf)
                wid -= iconWidthPercents;

            if (textAlign == TextAlignment.Left)
                captionTransform.anchorMin = new Vector2(horizBorder, vertBorder);
            if (textAlign == TextAlignment.Right)
                captionTransform.anchorMin = new Vector2(1.0f - horizBorder - wid, vertBorder);
            if (textAlign == TextAlignment.Center)
            {
                captionTransform.anchorMin = new Vector2(0.5f - wid * 0.5f, vertBorder);
                if (icon != null && icon.gameObject.activeSelf)
                    captionTransform.anchorMin = new Vector2(iconWidthPercents + (1.0f - iconWidthPercents - wid) * 0.5f, vertBorder);
            }

            captionTransform.anchorMax = new Vector2(wid + captionTransform.anchorMin.x, 1.0f - vertBorder + captionOffsetY);
            captionTransform.sizeDelta = Vector2.zero;

            captionTransform.anchorMin += new Vector2(0.0f, captionOffsetY);
            captionTransform.anchorMax += new Vector2(0.0f, captionOffsetY);
        }
    }

    protected override void OnValidate()
    {
        UpdateAlignment();
    }
}

