using UnityEngine;
using System.Collections;

public class Highlight : MonoBehaviour {

	// Use this for initialization
	void Start () {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if(sr != null)
        {
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0.0f);
            LeanTween.alpha(sr.gameObject, c.a, 0.75f).setFrom(0.0f).setEase(LeanTweenType.easeInBack);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Destroy()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            LeanTween.alpha(sr.gameObject, 0.0f, 0.5f).setFrom(c.a).setEase(LeanTweenType.easeOutBack).setOnComplete(() => GameObject.Destroy(gameObject));
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }


}
