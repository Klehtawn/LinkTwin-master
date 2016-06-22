using UnityEngine;
using System.Collections;

public class GameWonBox : MessageBox {

    public Button backButton;
    public Button nextButton;
    public Button starButton;

    public enum ReturnValues
    {
        PressedBack,
        PressedNext,
        PressedStar
    }
	// Use this for initialization
    protected override void Start()
    {
        base.Start();

        UpdateWindowParameters();

        backButton.OnTouchUp += OnBackButtonPressed;

        if (GameSession.IsUserLevel())
        {
            nextButton.gameObject.SetActive(false);
            starButton.GetComponent<RectTransform>().anchoredPosition = nextButton.GetComponent<RectTransform>().anchoredPosition;
        }
        else
            nextButton.OnTouchUp += OnNextButtonPressed;

        starButton.OnTouchUp += OnStarButtonPressed;
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightArrow))
            Close((int)ReturnValues.PressedNext);
#endif
	}

    void OnBackButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedBack);
    }

    void OnNextButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedNext);
    }

    void OnStarButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedStar);
    }
}
