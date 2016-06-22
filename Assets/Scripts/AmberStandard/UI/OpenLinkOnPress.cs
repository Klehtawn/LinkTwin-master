using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class OpenLinkOnPress : MonoBehaviour {

    public string url;

	// Use this for WindowA
	void Start () {

        if(url == null || url.Length == 0)
        {
            Text t = GetComponent<Text>();
            if (t != null)
                url = t.text;
        }

        Widget w = GetComponent<Widget>();
        if (w != null)
            w.OnClick += OnButtonPress;
	
	}

    void OnButtonPress(MonoBehaviour sender, Vector2 p)
    {
        Application.OpenURL(url);
    }
}
