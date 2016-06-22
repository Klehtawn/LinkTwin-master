using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CopyText : MonoBehaviour {

	// Use this for initialization

    public Transform copyFrom;
    public bool disableRichText;

    string myText;
    
	void Start ()
    {
        myText = getText(transform);
	}

    string getText(Transform t)
    {
        if (t.GetComponent<Text>() != null)
            return t.GetComponent<Text>().text;
        if (GetComponent<TextMesh>() != null)
            return t.GetComponent<TextMesh>().text;

        return "";
    }

    void setText(Transform t, string s)
    {
        if (t.GetComponent<Text>() != null)
            t.GetComponent<Text>().text = s;
        if (GetComponent<TextMesh>() != null)
            t.GetComponent<TextMesh>().text = s;
    }

    int getTextSize(Transform t)
    {
        if (t.GetComponent<Text>() != null)
            return t.GetComponent<Text>().fontSize;
        if (GetComponent<TextMesh>() != null)
            return t.GetComponent<TextMesh>().fontSize;

        return 1;
    }

    void setTextSize(Transform t, int sz)
    {
        if (t.GetComponent<Text>() != null)
            t.GetComponent<Text>().fontSize = sz;
        if (GetComponent<TextMesh>() != null)
            t.GetComponent<TextMesh>().fontSize = sz;
    }

    Color getColor(Transform t)
    {
        if (t.GetComponent<Text>() != null)
            return t.GetComponent<Text>().color;
        if (GetComponent<TextMesh>() != null)
            return t.GetComponent<TextMesh>().color;

        return Color.black;
    }

    void setColor(Transform t, Color c)
    {
        if (t.GetComponent<Text>() != null)
            t.GetComponent<Text>().color = c;
        if (GetComponent<TextMesh>() != null)
            t.GetComponent<TextMesh>().color = c;
    }
	
	// Update is called once per frame
	void Update ()
    {
        string originalText = getText(copyFrom);

        if(myText == null || myText.Equals(originalText) == false)
        {
            myText = originalText;

            setText(transform, removeRichText(myText));
            setTextSize(transform, getTextSize(copyFrom));
        }

        Color c = getColor(transform);
        c.a = getColor(copyFrom).a * 0.6f;
        setColor(transform, c);

        if (transform.parent == copyFrom.transform.parent)
        {
            transform.localScale = copyFrom.localScale;
            transform.localRotation = copyFrom.localRotation;
        }


	}

    string removeRichText(string s)
    {
        if (disableRichText == false)
            return s;
        string ss = s;
        int ii = ss.IndexOf("<color=");
        if(ii >= 0)
        {
            int k = ss.IndexOf('>', ii);
            ss = ss.Remove(ii, k + 1 - ii);
        }
        else
        {
            return ss;
        }

        ii = ss.IndexOf("</color>");
        ss = ss.Remove(ii, 8);

        return ss;
    }
}
