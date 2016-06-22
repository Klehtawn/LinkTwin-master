using UnityEngine;

public class RateThisApp : MessageBox
{
    public Widget rateNowButton;
    public Widget rateLaterButton;
    public Widget rateNeverButton;

    public enum ReturnValues
    {
        Now,
        Later,
        Never
    }

    protected override void Start()
    {
        base.Start();

        rateNowButton.OnClick += OnRateNowPressed;
        rateLaterButton.OnClick += OnRateLaterPressed;
        rateNeverButton.OnClick += OnRateNeverPressed;
    }

    private void OnRateNowPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.Now);
    }

    private void OnRateLaterPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.Later);
    }

    private void OnRateNeverPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.Never);
    }
}
