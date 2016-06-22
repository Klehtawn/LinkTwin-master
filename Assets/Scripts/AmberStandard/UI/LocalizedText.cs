using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour {

    private string prevText = "";
    public string text = "[Localized Text]";

    Text myText;

	// Use this for initialization
	void Start () {
        myText = GetComponent<Text>();
        myText.text = Locale.GetString(text);
	}
	
	// Update is called once per frame
	void Update () {

        if(text != prevText)
        {
            myText.text = Locale.GetString(text);
            prevText = text;
        }
	
	}
}
