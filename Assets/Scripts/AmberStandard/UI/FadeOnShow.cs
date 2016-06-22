using UnityEngine;
using System.Collections;
using System;

public class FadeOnShow : ShowEffect
{
    float initialTransparency = -1.0f;

    WindowA myWindow = null;
	void Awake ()
    {
        myWindow = GetComponent<WindowA>();
        initialTransparency = myWindow.transparency;
        myWindow.SetTransparency(0.0f);
	}

    public override void StartShowing()
    {
        base.StartShowing();
        myWindow.SetTransparency(0.0f);
    }

    float timer = 0.0f;
	void Update ()
    {
	    if(timer < duration && duration > 0.0f)
        {
            timer += Time.deltaTime;
            myWindow.transparency = Mathf.Lerp(0.0f, initialTransparency, Mathf.Clamp01(timer / duration));
        }
	}

    public override void Restore()
    {
        if(timer == 0.0f && myWindow != null)
        {
            myWindow.SetTransparency(initialTransparency);
        }
    }
}
