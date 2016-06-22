using UnityEngine;
using System.Collections;


public class SettingsScreen : GameScreen {

    public Widget facebookLoginButton;
    public Widget facebookLogoutButton;
    public Widget facebookLikeButton;
    //public Widget aboutButton;

    public Widget sendFeedbackButton;
    public Widget resetProgressButton;
    public Widget restorePurchasesButton;

    public SettingsOnOff musicButton;


    protected override void Start()
    {
        base.Start();

        musicButton.OnSettingChanged += OnSoundSettingChanged;
        facebookLoginButton.OnClick += OnFacebookLoginButtonPressed;
        facebookLogoutButton.OnClick += OnFacebookLogoutButtonPressed;
        facebookLikeButton.OnClick += OnFacebookLikeButtonPressed;
        resetProgressButton.OnClick += OnResetProgressPressed;
        restorePurchasesButton.OnClick += OnRestorePurchases;

        UpdateFacebookButtons();

        GameSparksManager.Instance.OnFacebookStatusChanged += OnFacebookStatusChanged;
    }

    void OnFacebookStatusChanged()
    {
        UpdateFacebookButtons();
        DesktopUtils.HideLoadingIndicator();
        if (GameSparksManager.Instance.canCollectFbReward)
            GameSparksManager.Instance.CollectFbReward();
    }

    void OnDestroy()
    {
        GameSparksManager.Instance.OnFacebookStatusChanged -= OnFacebookStatusChanged;
    }

    private void UpdateFacebookButtons()
    {
        bool fb = GameSparksManager.Instance.IsLoggedToFacebook();

        facebookLoginButton.gameObject.SetActive(!fb);
        facebookLogoutButton.gameObject.SetActive(fb);
    }

    void OnSoundSettingChanged(MonoBehaviour sender)
    {
        if (GameSession.GetSettingForMusic())
            Desktop.main.sounds.ResumeMusic();
    }

    void OnFacebookLoginButtonPressed(MonoBehaviour sender, Vector2 pos)
    {
        if (GameSparksManager.Instance.IsLoggedToFacebook() == false)
        {
            GameSession.IgnoreNextInterrupt();
            GameSparksManager.Instance.StartFacebookAuthentication();
            DesktopUtils.ShowLoadingIndicator();
        }
    }

    void OnFacebookLogoutButtonPressed(MonoBehaviour sender, Vector2 pos)
    {
        if (GameSparksManager.Instance.IsLoggedToFacebook())
            GameSparksManager.Instance.FacebookLogout();
    }

    void OnFacebookLikeButtonPressed(MonoBehaviour sender, Vector2 pos)
    {

    }

    void OnResetProgressPressed(MonoBehaviour sender, Vector2 pos)
    {
        ConfirmationBox.Show(Locale.GetString("RESET_PROGRESS_QUESTION"), ResetProgress);
    }

    void OnRestorePurchases(MonoBehaviour sender, Vector2 pos)
    {

    }

    void ResetProgress()
    {
        GameSession.ResetProgress();
    }
}
