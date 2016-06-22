using UnityEngine;
using System.Collections;

public class ChapterIntroScreen : GameScreen {

    public Transform titleBox;
    public Transform messageBox;
    public Transform scene;

    public float delay = 0.8f;
    public float messageDelayOffset = 1.5f;

	// Use this for initialization
	protected override void Start () {
        base.Start();

        FadeAnimatorSpeed fas = GetComponentInChildren<FadeAnimatorSpeed>();
        if (fas != null)
        {
            fas.delay = delay;
            fas.fadeSpeed = 2.0f;
        }

        CanvasGroup cg = titleBox.GetComponent<CanvasGroup>();
        cg.alpha = 0.0f;
        myTween.Instance.FadeCanvasGroup(cg.gameObject, 1.0f, 2.2f, delay);

        CanvasGroup cg2 = messageBox.GetComponent<CanvasGroup>();
        cg2.alpha = 0.0f;
        myTween.Instance.FadeCanvasGroup(cg2.gameObject, 1.0f, 2.2f, delay + messageDelayOffset);
	}
}
