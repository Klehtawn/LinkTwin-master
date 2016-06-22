using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuScreen : GameScreen {

    public Button buttonGoogleLogin;
    public Button buttonGoogleAchievements;

    public RectTransform titleObject;

    public Widget playButton;

#if enableGPGServices
    private bool bGPGAuthenticated = false;
#endif

    // to be removed
    static int firstTimeCounter = 0;

    public bool animateTitle = true;

    protected override void Start()
    {
        base.Start();

        playButton.active = false;

        string locale = Locale.DetectLocale();
        Locale.LoadLocale(locale);

        if(firstTimeCounter == 0 && GameSession.GetSessionIndex() == 1)
        {
            WindowA.Create("UI/FirstTimeSequence").Show();
            Destroy(gameObject);
            firstTimeCounter++;
        }
        else
        {
            GameEconomy.LoadLocal();
            GameEconomy.Initialize();

    #if UNITY_ANDROID || UNITY_IOS
            AdUtils.Instance.ShowBanner(AdUtils.Instance.GetAdId(AdUtils.Location.MainMenu));
    #endif

            GameEvents.Send(GameEvents.EventType.MainMenuEntered);
        }

        ShopScreen.RegisterEvents();

#if enableGPGServices
        if (buttonGoogleAchievements != null)
            buttonGoogleAchievements.gameObject.SetActive(false);
        if (GPGManager.Instance.CanTrySilentAuthentication)
        {
            Debug.Log("mainMenuScreen CanTrySilentAuthentication");
            GPGManager.Instance.Authenticate();
        }
        if (buttonGoogleLogin != null)
            buttonGoogleLogin.OnClick += onButtonGoogleLogin;
        if (buttonGoogleAchievements != null)
            buttonGoogleAchievements.OnClick += onButtonGoogleAchievements;
#else
        if (buttonGoogleLogin != null)
		{
			buttonGoogleLogin.gameObject.SetActive(false);
			buttonGoogleLogin.enabled = false;
			buttonGoogleAchievements.gameObject.SetActive(false);
			buttonGoogleAchievements.enabled = false;
		}
#endif
    }

#if enableGPGServices
    void onButtonGoogleLogin(MonoBehaviour sender, Vector2 pos)
    {
        if (!GPGManager.Instance.IsAuthenticating && !GPGManager.Instance.Authenticated)
        {
            Debug.Log("mainMenuScreen authenticate");
            GPGManager.Instance.Authenticate(true);
        }
    }

    void onButtonGoogleAchievements(MonoBehaviour sender, Vector2 pos)
    {
        if (GPGManager.Instance.Authenticated)
        {
            Social.ShowAchievementsUI();
        }
    }
#endif

    float timer = 0.75f;
    float playButtonTimer = 0.8f;
    protected override void Update()
    {
        //if (menu_active)
        //{
        //    ScaleToScreen();
        //    return;
        //}
        base.Update();
        //Debug.Log("MainMenuScreen Update");
#if enableGPGServices
        if (GPGManager.Instance != null && bGPGAuthenticated != GPGManager.Instance.mAuthenticated)
        {
            Debug.Log("MainMenuScreen Update authenticated");
            bGPGAuthenticated = GPGManager.Instance.mAuthenticated;
            buttonGoogleLogin.gameObject.SetActive(!bGPGAuthenticated);
            buttonGoogleAchievements.gameObject.SetActive(bGPGAuthenticated);
        }
#endif
        if (animateTitle)
        {
            if (timer > 0.0f)
                timer -= Time.deltaTime;
            else
            {
                Vector3 p = titleObject.anchoredPosition3D;
                p.y = Mathf.Lerp(p.y, Desktop.main.height * 0.16f, 3.0f * Time.deltaTime);
                titleObject.anchoredPosition3D = p;
            }
        }

        if (GameSession.BackKeyPressed() && Desktop.main.IsWindowOnTop(this))
            GameSession.AskQuitApplication();

        if(playButtonTimer > 0.0f)
        {
            playButtonTimer -= Time.deltaTime;
            if (playButtonTimer < 0.0f)
                playButton.active = true;
        }
    }

    void OnDestroy()
    {
#if UNITY_ANDROID || UNITY_IOS
        AdUtils.Instance.DestroyBanner();
#endif
    }
}
