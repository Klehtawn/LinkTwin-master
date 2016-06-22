using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class UserLevelButton : Button
{
    public enum ButtonEvent
    {
        Play = 0,
        Share = 1,
        Edit = 2,
        Delete = 3,
        SlideIn = 4
    }

    public Text textLevelId;
    public Text textAuthor;
    public Text textNumVotes;
    public Text textRating;
    public Image imageStarsDim;
    public Image imageStarsBright;
    //public Animator slideAnim;
    public RectTransform panelSecondary;
    public GameObject contentRoot;
    public bool isLoading = true;

    public RawImage thumbnail;

    public Widget buttonShare;
    public Widget buttonEdit;
    public Widget buttonDelete;

    public int levelIndex;
    public string levelId = "";
    public string author = "";
    public int numVotes = 0;
    public float rating = 0f;

    public float sliderOutX = 360.6f;
    public float sliderInX = 96.3f;
    private float sliderTargetX;
    //private bool isSliding = false;
    public bool secondaryPanel = true;

    protected enum SliderState
    {
        Out,
        SlideIn,
        In,
        SlideOut
    }

    protected SliderState sliderState;

    public Action<MonoBehaviour, ButtonEvent> OnUserLevelButtonEvent;

    protected override void Start()
    {
        base.Start();

        contentRoot.SetActive(false);

        if (!secondaryPanel)
            panelSecondary.gameObject.SetActive(false);
    }

    private void UpdateContent()
    {
        textLevelId.text = levelId.ToUpper();
        textAuthor.text = author.ToUpper();
        textNumVotes.text = numVotes.ToString() + " VOTES";

        if (numVotes > 0)
        {
            textRating.text = (Mathf.Round(rating * 10) / 10).ToString();
            imageStarsDim.fillAmount = 1f;
        }
        else
        {
            textRating.text = "";
            imageStarsDim.fillAmount = 1.0f;
        }

        imageStarsBright.fillAmount = rating / 5.0f;

        OnClick += Tap;
        OnSwipe += Swipe;

        sliderState = SliderState.Out;
        sliderTargetX = sliderOutX;
    }

    protected override void Update()
    {
        base.Update();

        if (!Application.isPlaying)
            return;

        if (Mathf.Abs(panelSecondary.anchoredPosition.x - sliderTargetX) > Mathf.Epsilon)
        {
            panelSecondary.anchoredPosition = new Vector2(Mathf.Lerp(panelSecondary.anchoredPosition.x, sliderTargetX, 0.2f), panelSecondary.anchoredPosition.y);
            if (Mathf.Abs(panelSecondary.anchoredPosition.x - sliderTargetX) < 1f)
            {
                if (sliderState == SliderState.SlideIn)
                    sliderState = SliderState.In;
                if (sliderState == SliderState.SlideOut)
                    sliderState = SliderState.Out;
                panelSecondary.anchoredPosition = new Vector2(sliderTargetX, panelSecondary.anchoredPosition.y);
            }
        }
    }

    public void FinishLoading()
    {
        isLoading = false;
        contentRoot.SetActive(true);
        UpdateContent();
    }

    protected void Swipe(MonoBehaviour sender, Vector2 dir, float duration)
    {
        return;

        if (isLoading) return;
        if (!secondaryPanel) return;

        if (Mathf.Abs(dir.x) < Mathf.Abs(dir.y))
            return;

        if (dir.x > 0)
        {
            SlideOut();
        }

        if (dir.x < 0)
        {
            SlideIn();
            CallAction(ButtonEvent.SlideIn);
        }
    }

    protected static bool ButtonHit(MonoBehaviour button, Vector2 pos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(button.GetComponent<RectTransform>(), pos);
    }

    protected void Tap(MonoBehaviour sender, Vector2 pos)
    {
        if (isLoading) return;

        if (sliderState == SliderState.Out)
        {
            CallAction(ButtonEvent.Play);
        }
        else if (sliderState == SliderState.In)
        {
            if (ButtonHit(buttonShare, pos))
                CallAction(ButtonEvent.Share);
            if (ButtonHit(buttonEdit, pos))
                CallAction(ButtonEvent.Edit);
            if (ButtonHit(buttonDelete, pos))
                CallAction(ButtonEvent.Delete);
        }
    }

    public void SlideOut()
    {
        sliderState = SliderState.SlideOut;
        sliderTargetX = sliderOutX;
    }

    public void SlideIn()
    {
        sliderState = SliderState.SlideIn;
        sliderTargetX = sliderInX;
    }

    protected void CallAction(ButtonEvent action)
    {
        if (OnUserLevelButtonEvent != null)
            OnUserLevelButtonEvent(this, action);
    }

}
