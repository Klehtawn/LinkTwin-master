using UnityEngine;
using System.Collections;

public class GameWonBoxDiffRating : MessageBox
{
    public Button buttonDiff1;
    public Button buttonDiff2;
    public Button buttonDiff3;
    public Button buttonDiff4;
    public Button buttonDiff5;

    protected override void Start()
    {
        base.Start();

        UpdateWindowParameters();

        buttonDiff1.OnTouchUp += OnDiffButtonPressed;
        buttonDiff2.OnTouchUp += OnDiffButtonPressed;
        buttonDiff3.OnTouchUp += OnDiffButtonPressed;
        buttonDiff4.OnTouchUp += OnDiffButtonPressed;
        buttonDiff5.OnTouchUp += OnDiffButtonPressed;
    }

    private void OnDiffButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Button b = sender.GetComponent<Button>();
        if (b == buttonDiff1) Close(1);
        if (b == buttonDiff2) Close(2);
        if (b == buttonDiff3) Close(3);
        if (b == buttonDiff4) Close(4);
        if (b == buttonDiff5) Close(5);
    }
}
