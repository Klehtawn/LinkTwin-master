using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameWonBoxEx : MessageBox {

    public Widget retryButton;
    public Widget nextButton;
    public Widget exitButton;

    public float rating = 2.0f;
    public int reward;

    private Widget titleWidget;
    private Widget rewardWidget;
    private Widget starsWidget;
    private Widget buttonsWidget;

    public Text rewardsText;

    public int[] movesPerStar;

    public enum ReturnValues
    {
        PressedRetry,
        PressedNext,
        PressedExit,
    }

    float initialHeight;

    private WindowA congratsBox;
    private WindowA videoAdBox;

    protected override void Start()
    {
        base.Start();

        UpdateWindowParameters();

        retryButton.OnClick += OnRetryButtonPressed;
        nextButton.OnTouchUp += OnNextButtonPressed;
        exitButton.OnClick += OnExitButtonPressed;

        OnWindowStartClosing += _OnWindowStartClosing;

        EconomyBarModal.Show(this);

        titleWidget = transform.Find("Title").GetComponent<Widget>();
        rewardWidget = transform.Find("Reward").GetComponent<Widget>();
        starsWidget = transform.Find("Stars").GetComponent<Widget>();
        buttonsWidget = transform.Find("Buttons").GetComponent<Widget>();

        RectTransform starsOff = starsWidget.GetComponent<StarRatingDisplay>().starsOff;
        for (int i = 0; i < starsOff.childCount; i++)
        {
            Transform c = starsOff.GetChild(i);
            Text tc = c.GetComponentInChildren<Text>();

            //if (tc != null)
            //    tc.text = "+" + coinsPerStar.ToString();
        }

        starsWidget.GetComponent<StarRatingDisplay>().SetRating(0.0f, false);

        rewardsText.text = "+" + reward.ToString();


        rewardWidget.active = false;
        starsWidget.active = false ;
        buttonsWidget.active = false;

        rewardWidget.GetComponent<CanvasGroup>().alpha = 0.0f;
        starsWidget.GetComponent<CanvasGroup>().alpha = 0.0f;
        buttonsWidget.GetComponent<CanvasGroup>().alpha = 0.0f;

        StartCoroutine(ShowStars(0.8f));
        StartCoroutine(ShowReward(0.8f));
        StartCoroutine(ShowButtons(1.3f));

        Text[] starsMoves = starsOff.GetComponentsInChildren<Text>();
        if(starsMoves.Length == 3 && movesPerStar != null && movesPerStar.Length == 3)
        {
            for (int i = 1; i < starsMoves.Length; i++)
            {
                starsMoves[i].text = movesPerStar[i].ToString() + " moves";
            }
        }
        else
        {
            for (int i = 0; i < starsMoves.Length; i++)
                starsMoves[i].gameObject.SetActive(false);
        }

        if (GameSession.HasFinishedChapter() == true)
        {
            CongratulationsSmallBox csb = WindowA.Create("UI/CongratulationsSmallBox").GetComponent<CongratulationsSmallBox>();
            csb.defaultButton.OnClick += OnUnlockNextChapter;
            congratsBox = csb.GetComponent<WindowA>();
            StartCoroutine(ShowCongratsBox(2.3f));
        }
        else if (GameEconomy.CanShowVideoAd())
        {
            WatchVideoAdBox wvab = WindowA.Create("UI/WatchVideoAdBox").GetComponent<WatchVideoAdBox>();
            videoAdBox = wvab.GetComponent<WindowA>();
            StartCoroutine(ShowVideoAdBox(2.3f));
        }
    }

    void OnUnlockNextChapter(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedNext);
    }

    bool rewardGiven = false;
    void GiveReward()
    {
        if (rewardGiven == false)
        {
            GameEconomy.currency += reward;
            rewardGiven = true;
            GameEconomy.SaveLocal();
        }
    }
    IEnumerator ShowStars(float delay)
    {
        yield return new WaitForSeconds(delay);

        starsWidget.active = true;
        starsWidget.GetComponent<StarRatingDisplay>().SetRating(0.0f, false);
        starsWidget.GetComponent<StarRatingDisplay>().SetRating(rating, true);

        GiveReward();
    }

    IEnumerator ShowReward(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (reward > 0)
        {
            titleWidget.active = false;
            rewardWidget.active = true;
        }
    }

    IEnumerator ShowButtons(float delay)
    {
        yield return new WaitForSeconds(delay);
        buttonsWidget.active = true;
    }

    IEnumerator ShowCongratsBox(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (congratsBox != null)
        {
            congratsBox.ShowModal();
            congratsBox.transform.SetParent(transform);
        }
    }

    IEnumerator ShowVideoAdBox(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (videoAdBox != null)
        {
            videoAdBox.ShowModal();
            videoAdBox.transform.SetParent(transform);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        initialHeight = height;
    }

    void _OnWindowStartClosing(WindowA w, int ret)
    {
        if (congratsBox != null)
            congratsBox.Close();

        if (videoAdBox != null)
            videoAdBox.Close();

        GiveReward();
    }
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightArrow))
            Close((int)ReturnValues.PressedNext);
#endif

        if(rewardWidget.active)
        {
            float totalCoins = (float)reward * starsWidget.GetComponent<StarRatingDisplay>().rating / rating;
            rewardsText.text = "+" + Mathf.RoundToInt(totalCoins).ToString();
        }
	}

    void OnRetryButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedRetry);
    }

    void OnNextButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedNext);
    }

    void OnExitButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedExit);
    }
}
