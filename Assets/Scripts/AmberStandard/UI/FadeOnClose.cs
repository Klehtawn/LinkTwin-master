using UnityEngine;
using System.Collections;

public class FadeOnClose : CloseEffect {

    private float initialTransparency = -1.0f;
	void Awake ()
    {
        WindowA w = GetComponent<WindowA>();
        w.OnWindowClosing += _OnWindowClosing;
	}

    void _OnWindowClosing(WindowA w, float factor)
    {
        if (initialTransparency < 0.0f)
            initialTransparency = w.transparency;
        w.transparency = Mathf.Clamp01(factor * initialTransparency);
    }
}
