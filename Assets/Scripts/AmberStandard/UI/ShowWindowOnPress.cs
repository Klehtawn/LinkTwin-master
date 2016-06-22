using UnityEngine;
using System.Collections;

public class ShowWindowOnPress : MonoBehaviour {

    public WindowA windowToShow;
    public bool showModal = false;

	// Use this for WindowA
	void Start () {

        Widget w = GetComponent<Widget>();
        if (w != null)
            w.OnClick += OnButtonPress;
	
	}

    void OnButtonPress(MonoBehaviour sender, Vector2 p)
    {
        if(windowToShow != null)
        {
            if (showModal == false)
            {
                WindowA par = GetComponentInParent<WindowA>();
                par.Close();
                if(par.isModal)
                {
                    Desktop.main.topmostWindow.Close();
                }
            }

            if(showModal)
                WindowA.Create(windowToShow).ShowModal();
            else
                WindowA.Create(windowToShow).Show();
        }
        else
        {
            DesktopUtils.ShowNotAvailable();
        }
    }
}
