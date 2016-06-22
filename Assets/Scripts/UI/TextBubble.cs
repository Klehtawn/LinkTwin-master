using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TextBubble : Widget {

    public RectTransform textRectangle;
    public RectTransform arrowRectangle;

    public Text message;

    float currentWidth = 0.2f;

    CanvasGroup canvasGroup;

    float destroyTimer = -1.0f;
    FadeAndDestroy fadeAndDestroy = null;

    RectTransform rectTransform;

    [NonSerialized]
    public Transform objectFollowed;

    float myWidth;

    protected override void Awake()
    {
        base.Awake();

        rectTransform = GetComponent<RectTransform>();
        myWidth = rectTransform.rect.width;

        SetTextRectWidth(currentWidth);

        SetAnchor(0.1f);
    }

    int waitFrames = 2;
	
	protected override void Start () {
        base.Start();

        myTween.Instance.SpriteAlphaFade(message.gameObject, 0.0f, 0.0f);
        myTween.Instance.SpriteAlphaFade(message.gameObject, 1.0f, 0.5f, 0.3f);

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();

        if (waitFrames > 0)
        {
            waitFrames--;
            if(waitFrames == 0)
            {
                RectTransform tr = message.GetComponent<RectTransform>();
                float tw = tr.rect.width;
                float limit = 40.0f;
                if (tw < myWidth - limit)
                {
                    rectTransform.sizeDelta = new Vector2(tw + limit, rectTransform.sizeDelta.y);
                }

                if(tw > myWidth - limit)
                {
                    ContentSizeFitter csf = message.GetComponent<ContentSizeFitter>();
                    csf.enabled = false;
                    tr.sizeDelta = Vector2.zero;
                }

                SetAnchor(anchoring);
            }
            return;
        }

        if(destroyTimer >= 0.0f)
        {
            destroyTimer -= Time.deltaTime;
            if (destroyTimer < 0.0f)
            {
                fadeAndDestroy = gameObject.AddComponent<FadeAndDestroy>();
                fadeAndDestroy.duration = 0.15f;
            }
        }

        if (fadeAndDestroy != null) return;
        
        if (Mathf.Abs(currentWidth - 1.0f) > 0.001f)
        {
            currentWidth = Mathf.Lerp(currentWidth, 1.0f, Time.deltaTime * 9.0f);
            SetTextRectWidth(currentWidth);
        }
        else
        {
            if (currentWidth != 1.0f)
            {
                SetTextRectWidth(currentWidth);
                currentWidth = 1.0f;
            }
        }

        if (Mathf.Abs(1.0f - canvasGroup.alpha) > 0.01f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1.0f, Time.deltaTime * 6.0f);
        }
        else
        {
            if(canvasGroup.alpha != 1.0f)
                canvasGroup.alpha = 1.0f;
        }
	}

    void SetTextRectWidth(float percents)
    {
        if (rectTransform.pivot.x < 0.5f)
            textRectangle.anchorMax = new Vector2(percents, textRectangle.anchorMax.y);
        else
        {
            textRectangle.anchorMin = new Vector2(1.0f - percents, textRectangle.anchorMin.y);
            textRectangle.anchorMax = new Vector2(1.0f, textRectangle.anchorMax.y);
        }
    }

    public void DestroyAfter(float d)
    {
        destroyTimer = d;
    }

    float anchoring = 0.1f;

    public void SetAnchor(float anchorX)
    {
        float arrowOfs = 20.0f;

        anchoring = anchorX;

        if (anchorX > 0.5f)
        {
            GetComponentInChildren<Text>().alignment = TextAnchor.MiddleRight;

            arrowRectangle.anchorMin = new Vector2(1.0f, arrowRectangle.anchorMin.y);
            arrowRectangle.anchorMax = new Vector2(1.0f, arrowRectangle.anchorMax.y);
            arrowRectangle.anchoredPosition = new Vector2(-arrowOfs, 0.0f);

            rectTransform.pivot = new Vector2(1.0f - arrowOfs / rectTransform.rect.width, 0.0f);
        }
        else
        {
            GetComponentInChildren<Text>().alignment = TextAnchor.MiddleLeft;

            arrowRectangle.anchorMin = new Vector2(0.0f, arrowRectangle.anchorMin.y);
            arrowRectangle.anchorMax = new Vector2(0.0f, arrowRectangle.anchorMax.y);
            arrowRectangle.anchoredPosition = new Vector2(arrowOfs, 0.0f);

            rectTransform.pivot = new Vector2(arrowOfs / rectTransform.rect.width, 0.0f);
        }
    }
}
