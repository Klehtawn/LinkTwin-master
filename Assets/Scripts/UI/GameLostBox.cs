using UnityEngine;
using System.Collections;

public class GameLostBox: MessageBox {

    public Button backButton;
    public Button retryButton;

    public enum ReturnValues
    {
        PressedBack,
        PressedRetry
    }
	// Use this for initialization
    protected override void Start()
    {
        base.Start();

        UpdateWindowParameters();

        backButton.OnTouchUp += OnBackButtonPressed;
        retryButton.OnTouchUp += OnRetryButtonPressed;
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.RightArrow))
            Close((int)ReturnValues.PressedRetry);
#endif
	
	}

    void OnBackButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedBack);
    }

    void OnRetryButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedRetry);
    }
}
