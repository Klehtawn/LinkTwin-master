using UnityEngine;
using System.Collections;

public class FadeScreenToBlackOnClose : CloseEffect {

	void Awake ()
    {
        WindowA w = GetComponent<WindowA>();
        w.OnWindowClosing += _OnWindowClosing;
	}

    void _OnWindowClosing(WindowA w, float factor)
    {
        Desktop.main.postProcess.brightness = factor;
    }
}
