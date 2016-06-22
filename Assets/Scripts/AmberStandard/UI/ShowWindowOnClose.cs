using UnityEngine;
using System.Collections;

public class ShowWindowOnClose : MonoBehaviour {

    public WindowA windowToShowPrefab;
    public bool showFromBegining = false;
    void Start()
    {
        WindowA w = GetComponent<WindowA>();
        if(showFromBegining)
            w.OnWindowStartClosing += OnWindowClosed;
        else
            w.OnWindowClosed += OnWindowClosed;
    }
	
	void OnWindowClosed(WindowA sender, int returnValue)
    {
        if(windowToShowPrefab != null)
        {
            WindowA.Create(windowToShowPrefab).Show();
        }
    }
}
