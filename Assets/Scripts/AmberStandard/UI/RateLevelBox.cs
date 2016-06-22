using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class RateLevelBox : MessageBox {

    Transform stars;

	protected override void Start ()
    {
        base.Start();

        stars = transform.Find("Stars");
        for (int i = 0; i < stars.childCount; i++)
        {
            Transform c = stars.GetChild(i);
            c.GetComponent<Widget>().OnTouchUp += OnStarPressed;
            //c.GetComponent<Widget>().OnMouseEnter += OnStarHover;
            //c.GetComponent<Widget>().OnMouseExit += OnStarHover;
            SetStarAlpha(c, 0.3f);
        }
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
	}

    bool aStarWasPressed = false;
    void OnStarPressed(MonoBehaviour sender, Vector2 p)
    {
        UpdateStarsAlpha(sender);
        aStarWasPressed = true;
        for (int i = 0; i < stars.childCount; i++)
        {
            if (sender == stars.GetChild(i).GetComponent<Widget>())
            {
                GameSession.RateCurrentLevel(i + 1);
                StartCoroutine(CallClose(0.7f));
                EnableInput(false);
                return;
            }
        }
    }

    IEnumerator CallClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }

    void OnStarHover(MonoBehaviour sender, Vector2 p)
    {
        if(aStarWasPressed == false)
            UpdateStarsAlpha(sender);
    }

    void UpdateStarsAlpha(MonoBehaviour last)
    {
        float alpha = 1.0f;
        for (int i = 0; i < stars.childCount; i++)
        {
            Transform c = stars.GetChild(i);
            SetStarAlpha(c, alpha);
            if (last == c.GetComponent<Widget>())
                alpha = 0.3f;
        }
    }

    void SetStarAlpha(Transform t, float alpha)
    {
        Image img = t.GetComponentInChildren<Image>();
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
