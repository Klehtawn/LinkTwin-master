using UnityEngine;
using System;

//#define isUnlocked  unlockedBuild
using UnityEngine.Advertisements;

public class AdUtils : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_IPHONE
    public enum AdType
    {
        VIDEO,
        INTERSTITIAL,
        BANNER,
        NONE
    };

    public enum AdStatus
    {
        REQUESTED,
        LOADED,
        DISPLAYED,
        ERROR,
        DISMISSED,
        NONE
    };

    public enum Location
    {
        MainMenu,
        LevelSuccess,
        DoubleReward,
        UnlockBonus
    };

#if UNITY_ANDROID
    //banner main menu
    public string mainManuBannerTabletId = "7cd436b3607f4272879f496944c7c299";
    public string mainManuBannerPhoneId = "ccfab62a2275414ca10a84f9db4f21f6";

    //level success
    public string levelSuccessInterstitialTabletId = "65ef53983a6b474baf57741a0d26228e";
    public string levelSuccessInterstitialPhoneId = "18113f80e20d42f18082ff21f7e0fe40";
#endif

#if UNITY_IPHONE
    //banner main menu
    public string mainManuBannerTabletId = "7fa1249fe8a243c8b545417866356f36";
    public string mainManuBannerPhoneId = "95253660fecb4af596902debcae3999f";

    //level success
    public string levelSuccessInterstitialTabletId = "7e8415e5f11a485bbce89f6332dd4703";
    public string levelSuccessInterstitialPhoneId = "cc52a6f5a2074683932a3de90500e711";
#endif

    //video level success - double reward
    public string doubleRewardVideoTabletId = "rewardedVideo";
    public string doubleRewardVideoPhoneId = "rewardedVideo";

    //video unlock bonus level
    public string unlockBonusVideoTabletId = "rewardedVideo";
    public string unlockBonusVideoPhoneId = "rewardedVideo";

    public float tabletThreshold = 6f;
    public string appId = "53695586973";

    public static AdUtils Instance = null;
    public static event Action<bool> rewardShowEvent;

    private string requestedUnityAdId = "";
    private AdType requestedUnityAdType = AdType.NONE;
    private AdStatus requestedUnityAdStatus = AdStatus.NONE;
#if !useMopubRewardVideo
    private ShowOptions options;
#endif

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            GameObject.Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!CheckAgeGate())
            return;

        ReportAplicationOpen();
#if useMopubRewardVideo
        MoPub.initializeRewardedVideo();
#else
        options = new ShowOptions();
#endif
}

void OnEnable()
    {
        // Listen to all events for illustration purposes
        MoPubManager.onAdLoadedEvent += onAdLoadedEvent;
        MoPubManager.onAdFailedEvent += onAdFailedEvent;
        MoPubManager.onAdClickedEvent += onAdClickedEvent;
        MoPubManager.onAdExpandedEvent += onAdExpandedEvent;
        MoPubManager.onAdCollapsedEvent += onAdCollapsedEvent;

        MoPubManager.onInterstitialLoadedEvent += onInterstitialLoadedEvent;
        MoPubManager.onInterstitialFailedEvent += onInterstitialFailedEvent;
        MoPubManager.onInterstitialShownEvent += onInterstitialShownEvent;
        MoPubManager.onInterstitialClickedEvent += onInterstitialClickedEvent;
        MoPubManager.onInterstitialDismissedEvent += onInterstitialDismissedEvent;
        MoPubManager.onInterstitialExpiredEvent += onInterstitialExpiredEvent;

#if useMopubRewardVideo
        MoPubManager.onRewardedVideoLoadedEvent += onRewardedVideoLoadedEvent;
        MoPubManager.onRewardedVideoFailedEvent += onRewardedVideoFailedEvent;
        MoPubManager.onRewardedVideoExpiredEvent += onRewardedVideoExpiredEvent;
        MoPubManager.onRewardedVideoShownEvent += onRewardedVideoShownEvent;
        MoPubManager.onRewardedVideoFailedToPlayEvent += onRewardedVideoFailedToPlayEvent;
        MoPubManager.onRewardedVideoReceivedRewardEvent += onRewardedVideoReceivedRewardEvent;
        MoPubManager.onRewardedVideoClosedEvent += onRewardedVideoClosedEvent;
        MoPubManager.onRewardedVideoLeavingApplicationEvent += onRewardedVideoLeavingApplicationEvent;
#endif
    }

    void OnDisable()
    {
        // Remove all event handlers
        MoPubManager.onAdLoadedEvent -= onAdLoadedEvent;
        MoPubManager.onAdFailedEvent -= onAdFailedEvent;
        MoPubManager.onAdClickedEvent -= onAdClickedEvent;
        MoPubManager.onAdExpandedEvent -= onAdExpandedEvent;
        MoPubManager.onAdCollapsedEvent -= onAdCollapsedEvent;

        MoPubManager.onInterstitialLoadedEvent -= onInterstitialLoadedEvent;
        MoPubManager.onInterstitialFailedEvent -= onInterstitialFailedEvent;
        MoPubManager.onInterstitialShownEvent -= onInterstitialShownEvent;
        MoPubManager.onInterstitialClickedEvent -= onInterstitialClickedEvent;
        MoPubManager.onInterstitialDismissedEvent -= onInterstitialDismissedEvent;
        MoPubManager.onInterstitialExpiredEvent -= onInterstitialExpiredEvent;

#if useMopubRewardVideo
        MoPubManager.onRewardedVideoLoadedEvent -= onRewardedVideoLoadedEvent;
        MoPubManager.onRewardedVideoFailedEvent -= onRewardedVideoFailedEvent;
        MoPubManager.onRewardedVideoExpiredEvent -= onRewardedVideoExpiredEvent;
        MoPubManager.onRewardedVideoShownEvent -= onRewardedVideoShownEvent;
        MoPubManager.onRewardedVideoFailedToPlayEvent -= onRewardedVideoFailedToPlayEvent;
        MoPubManager.onRewardedVideoReceivedRewardEvent -= onRewardedVideoReceivedRewardEvent;
        MoPubManager.onRewardedVideoClosedEvent -= onRewardedVideoClosedEvent;
        MoPubManager.onRewardedVideoLeavingApplicationEvent -= onRewardedVideoLeavingApplicationEvent;
#endif
    }

    public Action<bool> OnVideoFinished;

    public void InitVideo(string adUnitId)
    {
        if (!CheckAgeGate()) {
            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            DesktopUtils.ShowLocalizedMessageBox("NO_INTERNET");
            return;
        }

        AdRequested(adUnitId, AdType.VIDEO);
#if useMopubRewardVideo
        MoPub.requestRewardedVideo(adUnitId);
#else
        options.resultCallback = HandleShowResult;

        if (Advertisement.IsReady(adUnitId))
            ShowRewardVideo(adUnitId);
        else
            DesktopUtils.ShowLoadingIndicator(true, 0, Locale.GetString("Please wait"));
#endif
    }

    public void InitInterstitial(string adUnitId)
    {
        if (!CheckAgeGate()) {
            return;
        }
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }

        AdRequested(adUnitId, AdType.INTERSTITIAL);
        string keywords = GetKeyWords();
        if (keywords != "")
            MoPub.requestInterstitialAd(adUnitId, GetKeyWords());
        else
            MoPub.requestInterstitialAd(adUnitId);
    }

#if useMopubRewardVideo
    public void PlayVideo(string adUnitId)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }

        if (!CheckAgeGate()) {
            return;
        }

#if UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
        Desktop.main.sounds.PauseSoundtrack();
        MoPub.showRewardedVideo(adUnitId);
#endif

    }
#endif

    public void ShowBanner(string adUnitId)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }

        if (!CheckAgeGate())
        {
            return;
        }

        AdRequested(adUnitId, AdType.BANNER);
        MoPub.createBanner(adUnitId, MoPubAdPosition.BottomCenter);
    }

    public void HideBanner()
    {
        if (!CheckAgeGate()) {
            return;
        }

        MoPub.showBanner(false);
    }

    public void UnHideBanner()
    {
        if (!CheckAgeGate()) {
            return;
        }

        MoPub.showBanner(true);

    }

    public void DestroyBanner()
    {
        if (!CheckAgeGate()) {
            return;
        }

        MoPub.destroyBanner();
    }

    public void ShowInterstitial(string adUnitId)
    {
        if (!CheckAgeGate()) {
            return;
        }

        MoPub.showInterstitialAd(adUnitId);
    }

    private bool IsTablet()
    {
#if UNITY_IPHONE
        if ((UnityEngine.iOS.Device.generation.ToString()).IndexOf("iPad") > -1)
        {
            return true;
        }
        else
        {
            return false;
        }
#elif UNITY_ANDROID
        float screenSize = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) / Screen.dpi;

        return screenSize >= tabletThreshold;

#else
        return false;
#endif
    }

    private bool IsPortrait()
    {
        if (Screen.width > Screen.height)
            return false;
        else
            return true;
    }

    public void ReportAplicationOpen()
    {
        MoPub.reportApplicationOpen(appId);
    }

    private bool CheckAgeGate()
    {
        return true;
        /*TODO?
        return (PlayerPrefs.GetInt(Intro.AGE_GATE_SHOWN) >= 13);
        */
    }

    public string GetKeyWords()
    {
        /*TODO
        string age = "0";
        string payer = "0";
        string orientation = "0";
        string device = SystemInfo.deviceModel.Replace(",", "_");
        int a = PlayerPrefs.GetInt(Intro.AGE_GATE_SHOWN);
        if (a <= 0)
            age = "0";
        else if (a <= 3)
            age = "1";
        else if (a <= 6)
            age="2";
        else if (a <= 9)
            age = "3";
        else if (a <= 12)
            age = "4";
        else if (a <= 15)
            age = "5";
        else if (a <= 17)
            age = "6";
        else if (a <= 24)
            age = "7";
        else if (a <= 34)
            age = "8";
        else if (a <= 44)
            age = "9";
        else if (a <= 54)
            age = "10";
        else if (a <= 64)
            age = "11";
        else 
            age = "12";

        if (PlayerProfile.HasSpentMoney())
            payer = "1";

        if (IsPortrait() == false)
            orientation = "1";
        string keywords = "DAS_rp_age:" + age + ",DAS_rp_mon:" + payer + ",DAS_rp_rot:" + orientation + ",DAS_rp_dev:" + device + ",DAS_cp_hlev:" + UserData.Instance.lastLevelUnlocked;

        return keywords;
        */
        return "";
    }

    public void onAdLoadedEvent(float height)
    {
        Debug.Log("onAdLoadedEvent. height: " + height);
        AdDisplayed();
    }


    public void onAdFailedEvent()
    {
        Debug.Log("onAdFailedEvent");
        AdError();
    }


    public void onAdClickedEvent()
    {
        Debug.Log("onAdClickedEvent");
        MoPub.destroyBanner();
        AdDismissed();
    }


    public void onAdExpandedEvent()
    {
        Debug.Log("onAdExpandedEvent");
    }


    public void onAdCollapsedEvent()
    {
        Debug.Log("onAdCollapsedEvent");
    }


    public void onInterstitialLoadedEvent()
    {
        Debug.Log("onInterstitialLoadedEvent");
        AdLoaded();
        ShowInterstitial(requestedUnityAdId);
    }


    public void onInterstitialFailedEvent()
    {
        Debug.Log("onInterstitialFailedEvent");
        AdError();
    }


    public void onInterstitialShownEvent()
    {
        Debug.Log("onInterstitialShownEvent");
        AdDisplayed();
    }


    public void onInterstitialClickedEvent()
    {
        Debug.Log("onInterstitialClickedEvent");
        AdDismissed();
    }


    public void onInterstitialDismissedEvent()
    {
        Debug.Log("onInterstitialDismissedEvent");
        AdDismissed();
    }


    public void onInterstitialExpiredEvent()
    {
        Debug.Log("onInterstitialExpiredEvent");
        AdDismissed();
    }

#if useMopubRewardVideo
    public void onRewardedVideoLoadedEvent(string adUnitId)
    {
        Debug.Log("onRewardedVideoLoadedEvent: " + adUnitId);
        AdLoaded();
        PlayVideo(adUnitId);
    }


    public void onRewardedVideoFailedEvent(string adUnitId)
    {
        Debug.Log("onRewardedVideoFailedEvent: " + adUnitId);
        AdError();
    }


    public void onRewardedVideoExpiredEvent(string adUnitId)
    {
        Debug.Log("onRewardedVideoExpiredEvent: " + adUnitId);
        AdDismissed();
    }


    public void onRewardedVideoShownEvent(string adUnitId)
    {
        Debug.Log("onRewardedVideoShownEvent: " + adUnitId);
        AdDisplayed();
    }


    public void onRewardedVideoFailedToPlayEvent(string adUnitId)
    {
        Debug.Log("onRewardedVideoFailedToPlayEvent: " + adUnitId);
        AdError();
    }


    public void onRewardedVideoReceivedRewardEvent(MoPubManager.RewardedVideoData rewardedVideoData)
    {
        Debug.Log("onRewardedVideoReceivedRewardEvent: " + rewardedVideoData);
    }


    public void onRewardedVideoClosedEvent(string adUnitId)
    {
        Debug.Log("onRewardedVideoClosedEvent: " + adUnitId);
        AdDismissed();
    }


    public void onRewardedVideoLeavingApplicationEvent(string adUnitId)
    {
        Debug.Log("onRewardedVideoLeavingApplicationEvent: " + adUnitId);
        AdDismissed();
    }
#endif

    private void AdDisplayed()
    {
        requestedUnityAdStatus = AdStatus.DISPLAYED;
        Debug.Log("AdShown");
    }

    private void AdDismissed()
    {
        requestedUnityAdStatus = AdStatus.DISMISSED;
        Debug.Log("AdDismissed");
    }

    private void AdError()
    {
        requestedUnityAdStatus = AdStatus.ERROR;
        Debug.Log("AdError");
    }

    public void AdRequested(string adUnitId, AdType adType)
    {
        requestedUnityAdId = adUnitId;
        requestedUnityAdStatus = AdStatus.REQUESTED;
        requestedUnityAdType = adType;
        Debug.Log("AdRequested " + adUnitId);
    }

    private void AdLoaded()
    {
        requestedUnityAdStatus = AdStatus.LOADED;
        Debug.Log("AdLoaded");
    }

    public bool IsAdRequested(string adUnitId)
    {
        bool retVal = requestedUnityAdId == adUnitId && requestedUnityAdStatus == AdStatus.REQUESTED;
        return retVal;
    }

    public bool IsAdReady(string adUnitId)
    {
        bool retVal = requestedUnityAdId == adUnitId && requestedUnityAdStatus == AdStatus.LOADED;
        return retVal;
    }

    public bool IsAdDismissed(string adUnitId)
    {
        bool retVal = requestedUnityAdId == adUnitId && requestedUnityAdStatus == AdStatus.DISMISSED;
        return retVal;
    }

    public bool IsAdError(string adUnitId)
    {
        bool retVal = requestedUnityAdId == adUnitId && requestedUnityAdStatus == AdStatus.ERROR;
        return retVal;
    }

    private bool IsAdDisplayed(string adUnitId)
    {
        bool retVal = requestedUnityAdId == adUnitId && requestedUnityAdStatus == AdStatus.DISPLAYED;
        return retVal;
    }

    public string GetAdId(Location location)
    {
        switch (location)
        {
            case Location.MainMenu:
                if (IsTablet()) return mainManuBannerTabletId;
                return mainManuBannerTabletId;
            case Location.LevelSuccess:
                if (IsTablet()) return levelSuccessInterstitialTabletId;
                return levelSuccessInterstitialPhoneId;
            case Location.DoubleReward:
                if (IsTablet()) return doubleRewardVideoTabletId;
                return doubleRewardVideoPhoneId;
            default: //Location.UnlockBonus
                if (IsTablet()) return unlockBonusVideoTabletId;
                return unlockBonusVideoPhoneId;
        }
    }

#if !useMopubRewardVideo
    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("Video completed. User rewarded 2x credits.");
                AdDisplayed();
                break;
            case ShowResult.Skipped:
                Debug.LogWarning("Video was skipped.");
                AdDismissed();
                break;
            case ShowResult.Failed:
                Debug.LogError("Video failed to show.");
                AdError();
                break;
        }
        if (rewardShowEvent != null)
            rewardShowEvent(result == ShowResult.Finished);
//        Desktop.main.sounds.PlaySoundtrack();
    }

    private void ShowRewardVideo(string adUnitId)
    {
        AdLoaded();
//        Desktop.main.sounds.PauseSoundtrack();
        Advertisement.Show(adUnitId, options);
    }
#endif

    void Update()
    {
        if (IsAdRequested(requestedUnityAdId) && Advertisement.IsReady(requestedUnityAdId)
            && Desktop.main.modalWindows.Length > 0)
        {
            DesktopUtils.HideLoadingIndicator();
            ShowRewardVideo(requestedUnityAdId);
        }
    }

#endif
}
