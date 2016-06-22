using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class SliderBar : Widget {

    private float prevValue = -1.0f;
    private float interpolatedValue = 0.0f;
    [Range(0.0f, 1.0f)]
    public float value = 0.5f;

    [Range(0.0f, 1.0f)]
    public float fillValue = 0.5f;

    private float interpolatedFillValue = 0.5f;

    public bool fillBar = true;

    [Range(0.1f, 0.9f)]
    public new float height = 0.5f;

    public SliderBarIndicator indicatorRef;

    Widget dot;
    RectTransform dotRect, myRect, filledBarRect, barRect;
	
	protected override void Awake () {
        base.Awake();

        dot = transform.Find("Dot").GetComponent<Widget>();
        dot.OnTouchDown += OnDotPressed;
        dot.OnTouchUp += OnDotReleased;
        dot.OnTouchMoved += OnDotMoved;

        dotRect = dot.GetComponent<RectTransform>();
        myRect = GetComponent<RectTransform>();
        barRect = transform.Find("Bar").GetComponent<RectTransform>();
        filledBarRect = transform.Find("FilledBar").GetComponent<RectTransform>();

        interpolatedValue = value;
        interpolatedValue = fillValue;

        PositionDot(value);
        PositionFillBar(fillValue);

        indicatorRef.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();

        value = Mathf.Clamp01(value);

        if (Mathf.Abs(value - prevValue) > 0.0001f)
        {
            if (OnValueChanged != null)
                OnValueChanged(value, prevValue);

            prevValue = value;
        }

        if (filledBarRect.gameObject.activeSelf != fillBar)
        {
            filledBarRect.gameObject.SetActive(fillBar);
        }

        if(Mathf.Abs(interpolatedValue - value) > 0.005f && valueChangedByInput == false)
        {
            interpolatedValue = Mathf.Lerp(interpolatedValue, value, Time.deltaTime * 7.0f);

            PositionDot(interpolatedValue);
        }
        else
        {
            if (interpolatedValue != value)
            {
                interpolatedValue = value;
                PositionDot(value);
            }
        }

        if(fillBar)
            PositionFillBar(fillValue);

        barRect.anchorMin = new Vector2(0.0f, 0.5f - height * 0.5f);
        barRect.anchorMax = new Vector2(1.0f, 0.5f + height * 0.5f);

        filledBarRect.anchorMin = new Vector2(0.0f, 0.5f - height * 0.5f);
        filledBarRect.anchorMax = new Vector2(0.0f, 0.5f + height * 0.5f);

	}

    void PositionDot(float v)
    {
        float validWidth = myRect.rect.width - myRect.rect.height;

        dotRect.anchoredPosition = new Vector2(v * validWidth, 0.0f);
    }

    void PositionFillBar(float v, bool force = false)
    {
        if(Mathf.Abs(interpolatedFillValue - v) < 0.005f && !force)
        {
            interpolatedFillValue = v;
            return;
        }

        interpolatedFillValue = Mathf.Lerp(interpolatedFillValue, v, Time.deltaTime * 5.0f);
        filledBarRect.sizeDelta = new Vector2(interpolatedFillValue * (myRect.rect.width - myRect.rect.height) + myRect.rect.height * 0.5f, 0.0f);
    }

    [NonSerialized]
    public bool valueChangedByInput = false;
    float dotPressedValue;
    void OnDotPressed(MonoBehaviour sender, Vector2 p)
    {
        valueChangedByInput = true;
        dotPressedValue = value;
        if (OnStartDragging != null)
            OnStartDragging();
    }

    void OnDotReleased(MonoBehaviour sender, Vector2 p)
    {
        valueChangedByInput = false;
        if (OnEndDragging != null)
            OnEndDragging();
    }

    void OnDotMoved(MonoBehaviour sender, Vector2 initialPos, Vector2 currentPos)
    {
        if (valueChangedByInput)
        {
            Vector2 dd = Desktop.main.ScreenToDesktopRelative(currentPos - initialPos);
            value = dotPressedValue + dd.x / myRect.rect.width;
        }
    }

    public Action<float, float> OnValueChanged; // current, previous
    public Action OnStartDragging;
    public Action OnEndDragging;

    public void SetFillValueDirect(float v)
    {
        fillValue = v;
        interpolatedFillValue = v;
        PositionFillBar(v, true);
    }

    List<SliderBarIndicator> indicators = new List<SliderBarIndicator>();

    public SliderBarIndicator AddIndicator(float atValue, float scaling, bool isFloat)
    {
        GameObject obj = GameObject.Instantiate<GameObject>(indicatorRef.gameObject);
        obj.SetActive(true);
        obj.transform.SetParent(indicatorRef.transform.parent);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(rt.offsetMin.x, 0.0f);
        rt.offsetMax = new Vector2(rt.offsetMax.x, 0.0f);
        rt.localScale = Vector3.one * 1.5f;

        SliderBarIndicator sbi = obj.GetComponent<SliderBarIndicator>();
        indicators.Add(sbi);
        sbi.value = atValue;
        sbi.valueIsFloat = isFloat;

        return sbi;
    }

    public void RemoveAllIndicators()
    {
        foreach(SliderBarIndicator sbi in indicators)
        {
            Destroy(sbi.gameObject);
        }

        indicators.Clear();
    }
}
