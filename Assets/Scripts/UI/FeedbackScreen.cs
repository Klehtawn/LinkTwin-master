using UnityEngine;
using UnityEngine.UI;

public class FeedbackScreen : WindowA
{
    public Widget sendButton;
    //public Widget backButton;

    public InputField nameText;
    public InputField emailText;
    public InputField bodyText;

    protected override void Start()
    {
        base.Start();

        sendButton.OnTouchUp += OnSendPressed;
        //backButton.OnTouchUp += OnBackPressed;

        UpdateSendButton();

        Desktop.main.windowsFlow.Push(this);
    }

    protected override void Update()
    {
        base.Update();
    }

    protected void UpdateSendButton()
    {
        sendButton.active = !string.IsNullOrEmpty(emailText.text) && !string.IsNullOrEmpty(bodyText.text);
    }

    public void OnTextFieldChanged()
    {
        UpdateSendButton();
    }

    protected void OnSendPressed(MonoBehaviour sender, Vector2 pos)
    {
        GameSparksManager.Instance.SendUserFeedback(nameText.text, emailText.text, bodyText.text);

        DesktopUtils.ShowLocalizedMessageBox("THANKS_FOR_FEEDBACK", () => Desktop.main.windowsFlow.Backward());
    }
}
