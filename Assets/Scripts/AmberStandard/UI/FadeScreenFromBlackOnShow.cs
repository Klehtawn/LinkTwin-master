using UnityEngine;
using System.Collections;
using System;

public class FadeScreenFromBlackOnShow : ShowEffect
{
    WindowA myWindow = null;
	void Awake ()
    {
        myWindow = GetComponent<WindowA>();
	}

    float timer = 0.0f;

    public override void StartShowing()
    {
        Desktop.main.postProcess.brightness = 0.0f;
        timer = 0.0f;
    }
	void Update ()
    {
	    if(timer < duration && duration > 0.0f)
        {
            timer += Time.deltaTime;
            Desktop.main.postProcess.brightness = Mathf.Clamp01(timer / duration);
        }
	}
}
