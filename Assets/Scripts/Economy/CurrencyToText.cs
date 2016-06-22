using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class CurrencyToText : MonoBehaviour {

    public bool animated = true;
    public bool poppingAnimation = true;
    public float interpolateSpeed = 7.0f;

    float prevCurrency;
    float currency;
    float targetCurrency;

    Text myText;
	void Start () {
        myText = GetComponent<Text>();

        currency = (float)GameEconomy.currency;
        prevCurrency = currency;
        targetCurrency = currency;

        UpdateText();
	}

    void UpdateText()
    {
        int c = (int)currency;
        myText.text = c.ToString();
    }

    bool poppingAnimCompleted = true;
	// Update is called once per frame
	void Update ()
    {
        if ((int)targetCurrency != GameEconomy.currency)
        {
            targetCurrency = (float)GameEconomy.currency;
            if (animated == false)
                currency = targetCurrency;
        }

        if(Mathf.Abs(targetCurrency - currency) < 1.0f)
        {
            currency = targetCurrency;
        }
        else
        {
            currency = Mathf.Lerp(currency, targetCurrency, interpolateSpeed * Time.deltaTime);
        }

        if (Mathf.Abs(prevCurrency - currency) > 1.0f)
        {
            prevCurrency = currency;
            UpdateText();
            if (poppingAnimation == true && poppingAnimCompleted)
            {
                float dur = 0.4f;
                LeanTween.scale(gameObject, Vector3.one * 1.2f, dur * 0.5f).setEase(LeanTweenType.easeOutBack).setOnComplete(() =>
                    {
                        LeanTween.scale(gameObject, Vector3.one, dur * 0.5f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
                            {
                                poppingAnimCompleted = true;
                            });
                    });
            }
        }
	}
}
