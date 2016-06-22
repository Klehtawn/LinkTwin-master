using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GamePausedBox : MessageBox {

    public Widget resumeButton;
    public Widget exitButton;

    public Text levelNumberText;

    public enum ReturnValues
    {
        PressedResume,
        PressedExit,
    }

	protected override void Start () {
        base.Start();

        resumeButton.OnClick += OnResumeButton;
        exitButton.OnClick += OnExitButton;

        int li = GameSession.GetStartIndexForChapter(GameSession.currentChapter) + GameSession.currentPlaying + 1;
        levelNumberText.text = "LEVEL " + li.ToString();
	}
	
	protected override void Update () {
        base.Update();

        if (GameSession.BackKeyPressed() && isClosingTimer < 0 && finishedShowing)
        {
            Close((int)ReturnValues.PressedResume);
        }
    }

    void OnExitButton(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedExit);
    }

    void OnResumeButton(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedResume);
    }
}
