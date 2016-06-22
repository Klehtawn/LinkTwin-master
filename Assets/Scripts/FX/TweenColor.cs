using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class TweenColor : MonoBehaviour {

    public Color startColor = Color.white;
    public Color endColor = Color.white;
    public float duration = 1.0f;
    public float delay = 0.5f;

    public bool setColorAtStart = true;

    private Image myImage;
    private Text myText;
	
	public void Start () {

        myImage = GetComponent<Image>();
        myText = GetComponent<Text>();

        if(setColorAtStart)
            SetColor(startColor);
        
        StartCoroutine(DoFading());
	}

    public Action OnCompleted;
	
	IEnumerator DoFading()
    {
        yield return new WaitForSeconds(delay);

        float steps = 30.0f;
        for (int i = 0; i < (int)steps; i++)
        {
            Color c = Color.Lerp(startColor, endColor, (float)i / (steps - 1.0f));
            SetColor(c);
            yield return new WaitForSeconds(duration / steps);
        }

        if (OnCompleted != null)
            OnCompleted();
    }

    void SetColor(Color c)
    {
        if (myImage != null)
            myImage.color = c;

        if (myText != null)
            myText.color = c;
    }
}
