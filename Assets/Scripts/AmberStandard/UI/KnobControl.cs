using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[ExecuteInEditMode]
public class KnobControl : MonoBehaviour {

    public RectTransform finger;
    public Text valueText;
    public float value = 0.0f;
    public float roundedValue = 0.0f;
    public bool displayRoundedValue = true;
    public float minValue = -1000.0f;
    public float maxValue = 1000.0f;

    public float steps = 20.0f;
    public bool analogue = false;

    private Widget widget;

    public bool supportsScaling = true;
    private bool doScaleAnimation = false;

    private bool isSmall = true;
    public float smallSize = 100.0f;

    private bool isBig = false;
    public float bigSize = 200.0f;

    private float targetSize = 0.0f;

    public RectTransform dotIndicator;

    public Text debugText;

	// Use this for initialization
	void Start () {
        widget = GetComponent<Widget>();
        widget.OnTouchDown += OnTouchDown;
        widget.OnTouchUp += OnTouchUp;
        widget.OnTouchMoved += OnTouchMoved;

        finger.anchoredPosition = new Vector2(widget.width * 0.5f, widget.height * 0.1f);

        //bigSize = widget.width;

        if(supportsScaling)
        {
            isSmall = true;
            widget.size = Vector2.one * smallSize;
            targetSize = smallSize;
        }

        isBig = !isSmall;

        if (isBig)
            targetSize = bigSize;
	}

    bool touchDown = false;

    float prevAngle = 0.0f;

    void OnTouchDown(MonoBehaviour sender, Vector2 pos)
    {
        //if (getFingerDistance(pos) > widget.width * 0.5f * 0.9f) return;

        touchDown = true;

        Color c = Color.white * 0.8f; c.a = 1.0f;
        myTween.Instance.SpriteColorFade(finger.gameObject, c, 0.3f);

        finger.anchoredPosition = pos;
        prevAngle = getFingerAngle();
        if (OnBeginDragging != null)
            OnBeginDragging();

        //if(isSmall)
            ScaleControl(bigSize);

        //HapticFeedback.Execute();

        ShowDebugText("touch down");
    }

    void OnTouchUp(MonoBehaviour sender, Vector2 pos)
    {
        if (touchDown == false) return;

        touchDown = false;

        Color c = Color.white * 0.0f; c.a = 1.0f;
        myTween.Instance.SpriteColorFade(finger.gameObject, c, 0.3f);

        if (isBig)
        {
            if (OnEndDragging != null)
                OnEndDragging();
        }

        //if (isBig)
            ScaleControl(smallSize);

        Desktop.main.sounds.SetMusicPlaybackPitch(1.0f);
        Desktop.main.sounds.SetMusicDefaultVolume();

        ShowDebugText("touch up");
    }

    void ScaleControl(float newSize)
    {
        if(supportsScaling)
        {
            targetSize = newSize;
            isSmall = false;
            isBig = false;
            doScaleAnimation = true;
        }
    }

    void OnTouchMoved(MonoBehaviour sender, Vector2 start, Vector2 current)
    {
        if(touchDown)
        {
            ShowDebugText("touch moving");

            finger.anchoredPosition = current;

            Vector2 center = new Vector2(widget.width * 0.5f, widget.height * 0.5f);

            if (Vector2.Distance(current, center) >= 1.0f && isBig)//widget.width * 0.5f * 0.5f)
            {
                float a = getFingerAngle();

                ShowDebugText("touch moving @" + current.x + "," + current.y + " and angle is " + a);

                float angleDiff = a - prevAngle;

                if (a <= 90.0f && prevAngle >= 270.0f)
                    angleDiff = a + 360.0f - prevAngle;

                if (prevAngle <= 90.0f && a >= 270.0f)
                {
                    angleDiff = a - (360.0f + prevAngle);
                }

                float v = steps * angleDiff / 360.0f;

                AdjustValue(value - v, true);

                prevAngle = a;
            }
        }
    }

    public Action<float> OnValueChanged;
    public Action<float> OnRoundedValueChanged;

    public Action OnBeginDragging;
    public Action OnEndDragging;

    void AdjustValue(float v, bool byInput = false)
    {
        float _v = Mathf.Clamp(v, minValue, maxValue);
        if(_v != value)
        {
            Desktop.main.sounds.SetMusicVolume(0.7f);
            Desktop.main.sounds.SetMusicPlaybackPitch(Mathf.Sign(_v - value) * 3.0f);
            value = _v;
            if (OnValueChanged != null)
                OnValueChanged(value);
        }

        float rv = Mathf.Round(value);
        if(rv != roundedValue)
        {
            roundedValue = rv;
            /*if (analogue == false && byInput)
                //NativeVibration.Vibrate(50);
                HapticFeedback.Execute();*/

            if (analogue == false && byInput)
                GameEvents.Send(GameEvents.EventType.UndoTick);

            if (OnRoundedValueChanged != null)
                OnRoundedValueChanged(rv);
        }
    }
	
	// Update is called once per frame
	void Update () {
        AdjustValue(value);
        SetText();

        if(supportsScaling)
        {
            float ww = widget.width;
            if (doScaleAnimation)
            {
                ww = Mathf.Lerp(ww, targetSize, 8.0f * Time.deltaTime);

                widget.size = Vector2.one * ww;

                if (Mathf.Abs(ww - smallSize) < 0.01f)
                {
                    widget.width = smallSize;
                    isSmall = true;
                    doScaleAnimation = false;
                }

                if (Mathf.Abs(ww - bigSize) < 0.5f)
                {
                    widget.width = bigSize;
                    isBig = true;
                    doScaleAnimation = false;
                    prevAngle = getFingerAngle();
                }
            }
        }

        if(dotIndicator != null)
        {
            float f = (widget.width - smallSize) / bigSize;
            float dotRadius = widget.width * 0.5f - Mathf.Lerp(14.5f, 30.0f, f);
            float angle = -Mathf.PI * 2.0f * value / steps - Mathf.PI * 0.5f;
            Vector2 p = new Vector2(widget.width * 0.5f, widget.height * 0.5f);
            p.x += dotRadius * Mathf.Cos(angle);
            p.y += dotRadius * Mathf.Sin(angle);
            dotIndicator.anchoredPosition = p;
        }
    }

    void SetText()
    {
        if (valueText != null)
        {
            valueText.text = displayRoundedValue ? roundedValue.ToString() : value.ToString();
        }
    }

    float getFingerAngle()
    {
        Vector2 center = new Vector2(widget.width * 0.5f, widget.height * 0.5f);
        Vector2 d = finger.anchoredPosition - center;
        if(d.sqrMagnitude > 0.001f)
        {
            d.Normalize();
            float a = Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(d, Vector2.right));
            if (d.y < 0.0f)
                a = 360.0f - a;

            return a;
        }
        return 0.0f;
    }

    float getFingerDistance()
    {
        return getFingerDistance(finger.anchoredPosition);
    }

    float getFingerDistance(Vector2 pos)
    {
        Vector2 center = new Vector2(widget.width * 0.5f, widget.height * 0.5f);
        Vector2 d = pos - center;
        return d.magnitude;
    }

    void ShowDebugText(string txt)
    {
        if(debugText != null)
        {
            debugText.text = txt;
        }
    }
}

