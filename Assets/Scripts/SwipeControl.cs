using UnityEngine;
using System.Collections;
using System;

public class SwipeControl
{
    public Action<Vector2> OnTouchEnded;
    public Action<Vector2> OnTouchStart;
    public Action<Vector2> OnSwipe;
    public Action<Vector2> OnTouchMove;

    bool hasTouch = false;
    Vector2 touchPos;
    Vector2 touchStart;
    float touchTime;

    bool swipeWasCalled = false;
    
    // Update is called once per frame
    public void Update()
    {
        if (Input.touchCount > 0 || Input.GetMouseButton(0))
        {
            Vector2 cursorPos = Vector2.zero;
            if (Input.touchCount > 0)
                cursorPos = Input.GetTouch(0).position;
            else
                cursorPos = Input.mousePosition;

            if (hasTouch == false)
            {
                touchStart = cursorPos;
                touchTime = Time.time;
                CallTouchStart(touchStart);
            }

            CallTouchMove(cursorPos);

            hasTouch = true;
            touchPos = cursorPos;
        }
        else if (hasTouch == true)
        {
            CallTouchEnded(touchPos);
            hasTouch = false;
        }
    }

    float centimetersToPixels(float cms)
    {
        float dpi = Screen.dpi;
        if(dpi == 0.0f)
            dpi = 90.0f;

        float inches = cms / 2.54f;
        return inches * dpi;
    }

    public float maxSwipeDuration = 0.5f;

    void CallTouchEnded(Vector2 pos)
    {
        /*if (false)
        {
            if (Time.time - touchTime < maxSwipeDuration)
            {
                // swipe
                Vector2 diff = pos - touchStart;
                //float threshold = Screen.width * 0.05f;
                if (Mathf.Abs(diff.x) < Mathf.Abs(diff.y))
                    diff.x = 0.0f;
                else
                    if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                        diff.y = 0.0f;
                    else
                    {
                        diff = Vector2.zero;
                    }

                if (diff.magnitude > centimetersToPixels(GameSession.minSwipeLength))
                {
                    if (diff.x != 0.0f)
                        diff.x = Mathf.Sign(diff.x);
                    if (diff.y != 0.0f)
                        diff.y = Mathf.Sign(diff.y);

                    CallSwipe(diff);
                }
            }
        }*/

        if (OnTouchEnded != null)
            OnTouchEnded(pos);
    }

    Vector2 NormalizeDirection(Vector2 direction)
    {
        Vector2 diff = direction;
        if (diff.x != 0.0f)
            diff.x = Mathf.Sign(diff.x);
        if (diff.y != 0.0f)
            diff.y = Mathf.Sign(diff.y);

        return diff;
    }

    void CallTouchMove(Vector2 pos)
    {
        Vector2 diff = pos - touchStart;
        if (OnTouchMove != null)
        {
            OnTouchMove(pos);
        }

        if (swipeWasCalled == false && (Time.time - touchTime < maxSwipeDuration))
        {
            // swipe
            //float threshold = Screen.width * 0.05f;
            if (Mathf.Abs(diff.x) < Mathf.Abs(diff.y))
                diff.x = 0.0f;
            else
                if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                    diff.y = 0.0f;

            if (diff.magnitude > centimetersToPixels(GameSession.minSwipeLength))
            {
                diff = NormalizeDirection(diff);

                CallSwipe(diff);
            }
        }
    }

    void CallTouchStart(Vector2 pos)
    {
        if (OnTouchStart != null)
            OnTouchStart(pos);

        swipeWasCalled = false;
    }

    void CallSwipe(Vector2 dir)
    {
        if (OnSwipe != null)
            OnSwipe(dir);

        swipeWasCalled = true;
    }
}
