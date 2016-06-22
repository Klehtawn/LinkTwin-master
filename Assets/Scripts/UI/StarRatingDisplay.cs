using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class StarRatingDisplay : MonoBehaviour {

    public RectTransform starsOn;
    public RectTransform starsOff;

    public float rating = 2.5f;
    private float targetRating = 2.5f;

    private RectTransform myRectTransform;

    public float fillDuration = 1.2f;

    Image[] stars;

	// Use this for initialization
	void Awake () {
        myRectTransform = GetComponent<RectTransform>();

        stars = new Image[starsOn.childCount];
        for(int i = 0; i < starsOn.childCount; i++)
        {
            stars[i] = starsOn.GetChild(i).GetComponent<Image>();
            stars[i].fillMethod = Image.FillMethod.Radial360;
            stars[i].fillAmount = 0.0f;
        }

        targetRating = rating;
	}

	void Update () {

        rating += Time.deltaTime * targetRating / fillDuration;
        rating = Mathf.Clamp(rating, 0.0f, targetRating);
        //rating = Mathf.Lerp(rating, targetRating, Time.deltaTime * 3.5f);

        if (Mathf.Abs(rating - targetRating) < 0.001f)
            rating = targetRating;

        rating = Mathf.Min(rating, (float)starsOn.childCount);

        for(int i = 0; i < stars.Length; i++)
        {
            float prev = stars[i].fillAmount;
            stars[i].fillAmount = Mathf.Clamp01(rating - (float)i);

            if (stars[i].fillAmount != prev && prev == 0.0f)
                GameEvents.Send(GameEvents.EventType.RewardOneStar + i);
        }
	}

    public void SetRating(float value, bool animated = false)
    {
        targetRating = value;

        if(animated == false)
            rating = value;
    }
}
