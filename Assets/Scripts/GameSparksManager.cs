#define GAMESPARKS
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

#if GAMESPARKS
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using GameSparks.Core;
#endif

using ICSharpCode.SharpZipLib.Zip;

using Facebook.Unity;

#if GAMESPARKS
public class GameSparksManager : MonoBehaviour
{
    #region singleton stuff
    private static GameSparksManager instance = null;

    public static GameSparksManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            GS.GameSparksAvailable += GameSparksAvailable;
            GS.GameSparksAuthenticated += GameSparksAuthenticated;

            InitializeFacebook();
        }
        else
        {
            Debug.LogWarning("Another GameSparksManager found in scene, self-destruct");
            Destroy(this.gameObject);
        }
    }
    #endregion

#if UNITY_ANDROID
    //public string GcmSenderId = "53695586973";
    [System.NonSerialized]
    public string GcmSenderId = "685460901027";
#endif

    // --- don't count session start/end for iap activities taking over
    public bool ignoreNextStart = false;
    public bool ignoreNextPause = false;

    private bool firstAuthenticationExecuted = false;

    public bool canCollectFbReward = false;

    public Action OnFacebookStatusChanged;

    public void GameSparksAvailable(bool available)
    {
        Debug.Log("GameSparksManager Available event: " + available);

        if (available)
        {
            if (!GS.Authenticated)
            {
                if (FB.IsLoggedIn)
                {
                    AuthenticateWithFacebook();
                }
                else
                {
                    StartDeviceAuthentication();
                }
            }
        }
    }

    private string GetLaunchType()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass javaController = new AndroidJavaClass("com.amberstudio.amberlib.MainController");
        if (javaController != null)
        {
            string intentType = javaController.CallStatic<string>("GetIntentType");
            Debug.Log("intent type: " + intentType);
            return intentType;
        }
        else
        {
            return "ERROR";
        }
#else
        return "NOT_IMPLEMENTED";
#endif
    }

    private void GameSparksAuthenticated(string obj)
    {
        Debug.Log("GameSparks Authenticated event: " + obj);

        if (!ignoreNextStart)
        {
            RecordAnalytics("START_SESSION",
                "Platform", Application.platform.ToString(),
                "Max_Chapter", GameSession.GetMaxUnlockedChapter(),
                "Max_Level", GameSession.GetMaxUnlockedLevel(),
                "Connection_Type", Application.internetReachability.ToString(),
                "Game_Language", Locale.GetCurrentLocale(),
                "Operating_System", SystemInfo.operatingSystem,
                "Graphics_Device_Name", SystemInfo.graphicsDeviceName,
                "Graphics_Device_Vendor", SystemInfo.graphicsDeviceVendor,
                "Processor_Type", SystemInfo.processorType,
                "Processor_Frequency", SystemInfo.processorFrequency,
                "Processor_Count", SystemInfo.processorCount,
                "Device_Model", SystemInfo.deviceModel,
                "Player_Gains", GameEconomy.currency,
                "Global_Playtime", GameSession.GetTotalTimePlayed(),
                "User_TimeOffset", TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours,
                "User_Time", DateTime.Now,
                "Launch_From", GetLaunchType(),
                "Screen_Width", Screen.width,
                "Screen_Height", Screen.height
                );
        }

        ignoreNextStart = false;

        if (!firstAuthenticationExecuted)
        {
            OnFirstAuthentication();
            firstAuthenticationExecuted = true;
        }
    }

    private void OnFirstAuthentication()
    {
#if !UNITY_EDITOR
        CheckForContentPackUpdates(() => {
            string locale = Locale.DetectLocale();
            Locale.LoadLocale(locale);
            Debug.Log("Downloaded level pack");
        });
#endif
        RegisterGcm();
    }

    public void RegisterGcm()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass javaController = new AndroidJavaClass("com.amberstudio.amberlib.PushNotificationController");
        if (javaController != null)
            javaController.CallStatic("InitRegistrationReceiver", GcmSenderId, GameSparksManager.Instance.gameObject.name);
#endif
    }

    public void SetGcmToken(string token)
    {
        Debug.Log("Received Gcm key from native code: " + token);
        PushRegistrationRequest req = new PushRegistrationRequest();
        req.SetPushId(token);
        req.Send((response) =>
        {
            if (response.HasErrors)
                Debug.LogWarning("Failed to send Gcm token to GameSparks");
            else
                Debug.Log("Sent Gcm token key to GameSparks");
        });

    }

    private void ScheduleComeBackNotification()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        long time = 3600 * 24 * 5;
        //long time = 10;
        AndroidJavaClass messageControllerClass = new AndroidJavaClass("com.amberstudio.amberlib.LocalNotificationController");
        if (messageControllerClass != null)
            messageControllerClass.CallStatic("ScheduleNotification", "LTComeBack", Locale.GetString("COME_BACK_TITLE"), Locale.GetString("COME_BACK_MESSAGE"), time);
#endif
    }

    void OnApplicationQuit()
    {
        ScheduleComeBackNotification();
    }

    public void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            ScheduleComeBackNotification();
            if (!ignoreNextPause)
            {
                RecordAnalytics("END_SESSION",
                    "Max_Chapter", GameSession.GetMaxUnlockedChapter(),
                    "Max_Level", GameSession.GetMaxUnlockedLevel(),
                    "Last_Map", GameSession.lastPlayedLevel,
                    "Global_Playtime", GameSession.GetTotalTimePlayed(),
                    "Player_Gains", GameEconomy.currency
                    );
            }

            ignoreNextPause = false;
        }
    }

    public void StartDeviceAuthentication()
    {
        new DeviceAuthenticationRequest().Send((AuthenticationResponse response) =>
        {
            if (response.HasErrors)
            {
                Debug.LogWarning("device auth error");
            }
            else
            {
                Debug.Log("device auth ok");
                CheckProgressInfo(response);
                if (response.NewPlayer.HasValue && response.NewPlayer.Value == true)
                    UpdateUserDetails();
            }
        });
    }

    public void StartFacebookAuthentication()
    {
        FB.LogInWithReadPermissions(
            new List<string>() { "public_profile", "email", "user_friends" },
            FBAuthCallback);
    }

    public void FacebookLogout()
    {
        FB.LogOut();
        if (OnFacebookStatusChanged != null)
            OnFacebookStatusChanged();
    }

    public void InitializeFacebook()
    {
        if (!FB.IsInitialized)
            FB.Init(FBInitCallback, FBOnHideUnity);
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        else
            FB.ActivateApp();
#endif
    }

    private void FBInitCallback()
    {
        if (!FB.IsInitialized)
            Debug.Log("failed to initialize Facebook");
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        else
            FB.ActivateApp();
#endif
    }

    private void FBAuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            AuthenticateWithFacebook();
        }
        else
        {
            Debug.Log("fb login failed");
            if (OnFacebookStatusChanged != null)
                OnFacebookStatusChanged();
        }
    }

    private void AuthenticateWithFacebook()
    {
        new FacebookConnectRequest().SetAccessToken(AccessToken.CurrentAccessToken.TokenString).Send((response) =>
        {
            if (response.HasErrors)
            {
                Debug.LogWarning("failed to authenticate to GameSparks with the Facebook access token");
            }
            else
            {
                Debug.Log("successfully logged in to GameSparks with the Facebook access token");
                CheckProgressInfo(response);
            }
            if (OnFacebookStatusChanged != null)
                OnFacebookStatusChanged();
        });
    }

    #region progress sync
    public void UpdateProgressInfo()
    {
        LogEventRequest req = new LogEventRequest();
        req.SetEventKey("UpdateUserProgress");
        GSRequestData data = new GSRequestData();
        for (int i = 0; i < GameSession.unlockedLevels.Length; i++)
        {
            data.AddNumberList("Chapter" + (i + 1).ToString(), GameSession.unlockedLevels[i].levels);
        }
        req.SetEventAttribute("UnlockedLevels", data);
        req.Send((response) =>
        {
            if (response.HasErrors)
                Debug.LogWarning("Failed to send unlocked levels");
            else
                Debug.Log("Unlocked levels sent");
        });
    }

    private void CheckProgressInfo(AuthenticationResponse response)
    {
        if (response.ScriptData == null)
            return;

        GameSession.LoadChapters();

        if (!(response.ScriptData.ContainsKey("FbReward") && response.ScriptData.GetBoolean("FbReward").Value))
            canCollectFbReward = true;

        GSData unlockdata = response.ScriptData.GetGSData("UnlockedLevels");
        int numChapters = unlockdata.BaseData.Keys.Count;
        for (int i = 0; i < numChapters; i++)
        {
            List<int> chapterInfo = unlockdata.GetIntList("Chapter" + (i + 1).ToString());
            foreach (int lvl in chapterInfo)
                if (!GameSession.unlockedLevels[i].levels.Contains(lvl))
                    GameSession.unlockedLevels[i].levels.Add(lvl);
        }

        GameSession.WriteUnlockedLevels();
    }
        #endregion

        #region Facebook stuff
    public bool IsLoggedToFacebook()
    {
        return FB.IsLoggedIn;
    }

    private void FBOnHideUnity(bool isShown)
    {
        Debug.Log("on Facebook hide Unity: " + isShown);
    }

    public void ShareUserLevel(string id, Texture2D thumbnail)
    {
        if (FB.IsLoggedIn)
        {
            byte[] imagedata = thumbnail.EncodeToPNG();
            var wwwForm = new WWWForm();
            wwwForm.AddBinaryData("image", imagedata, "Level" + id + ".png");

            FB.API("me/photos", HttpMethod.POST, UploadThumbnailCallback, wwwForm);
        }
        else
        {
            StartFacebookAuthentication();
        }
    }

    private void UploadThumbnailCallback(IGraphResult result)
    {
        if (result.Cancelled || !String.IsNullOrEmpty(result.Error))
        {
            Debug.Log("failed to upload thumbnail");
        }
        else
        {
            Debug.Log(result.RawResult);
            if (result.ResultDictionary.ContainsKey("id"))
            {
                //string photoid = result.ResultDictionary["id"] as string;
                
            }
        }
    }

    private void ShareCallback(IShareResult result)
    {
        if (result.Cancelled || !String.IsNullOrEmpty(result.Error))
        {
            Debug.Log("ShareLink Error: " + result.Error);
        }
        else if (!String.IsNullOrEmpty(result.PostId))
        {
            // Print post identifier of the shared content
            Debug.Log(result.PostId);
        }
        else {
            // Share succeeded without postID
            Debug.Log("ShareLink success!");
        }
    }

    public void CollectFbReward()
    {
        if (!Available()) return;

        DesktopUtils.ShowLoadingIndicator();
        new LogEventRequest().SetEventKey("CollectFbReward").Send((response) =>
        {
            DesktopUtils.HideLoadingIndicator();
            if (response.HasErrors)
            { 
                Debug.LogWarning("Failed to collect FB reward");
            }
            else
            {
                Debug.Log("Collected FB reward");
                canCollectFbReward = false;
                GameEconomy.currency += 300;
                GameEconomy.SaveLocal();
                WindowA w = WindowA.Create("UI/FacebookReward");
                w.ShowModal();
            }
        });
    }
        #endregion

    public void UpdateUserDetails()
    {
        ChangeUserDetailsRequest cr = new ChangeUserDetailsRequest();
#if UNITY_EDITOR
        cr.SetDisplayName(Environment.UserName);
#else
        cr.SetDisplayName(Application.platform.ToString() + UnityEngine.Random.Range(1234, 9999));
#endif
        cr.Send((ChangeUserDetailsResponse response) =>
        {
            if (response.HasErrors)
            {
                Debug.LogWarning("change user details error");
            }
            else
            {
                Debug.Log("change user details ok");
            }
        });
    }

    public void UpdateUserProgress(int chapter, int level)
    {
        if (!Available())
            return;

        LogEventRequest req = new LogEventRequest();
        req.SetEventKey("UpdateUserProgress");
        req.SetEventAttribute("Chapter", chapter);
        req.SetEventAttribute("Level", level);
        req.Send((response) =>
        {
            DesktopUtils.HideLoadingIndicator();
            if (response.HasErrors)
            {
                Debug.LogWarning("failed to update user progress");
            }
            else
            {
                Debug.Log("updated user progress");
            }
        });
    }

    public void RecordAnalytics(string key, params object[] parameters)
    {
        //if (!GS.Available || !GS.Authenticated)
        //{
        //    Debug.LogError("failed to record analytics, GS.Available: " + GS.Available + ", GS.Authenticated: " + GS.Authenticated);
        //return;
        //}

        List<GSData> allData = new List<GSData>();
        GSRequestData data = new GSRequestData();
        data.AddString("Event", key);
        data.AddNumber("Session_nb", GameSession.GetSessionCount());
        data.AddNumber("Build", GameSession.GetBuildNumber());
        for (int i = 0; i < parameters.Length / 2; i++)
        {
            string paramKey = (string)parameters[2 * i];
            object paramVal = parameters[2 * i + 1];
            if (paramVal.GetType() == typeof(DateTime))
                data.AddDate(paramKey, (DateTime)paramVal);
            else if (paramVal.GetType() == typeof(double))
                data.AddNumber(paramKey, (double)paramVal);
            else if (paramVal.GetType() == typeof(float))
                data.AddNumber(paramKey, (float)paramVal);
            else if (paramVal.GetType() == typeof(int))
                data.AddNumber(paramKey, (int)paramVal);
            else if (paramVal.GetType() == typeof(string))
                data.AddString(paramKey, (string)paramVal);
            else
                throw new Exception("invalid parameter type: " + paramVal.GetType());
        }

        allData.Add(data);

        new LogEventRequest().SetEventKey("AnalyticsSend").SetEventAttribute("Params", allData).SetDurable(true).Send((response) =>
            {
                if (response.HasErrors)
                {
                    Debug.Log("failed to record analytics");
                }
                else
                {
                    Debug.Log("analytics sent successfully");
                }
            });
    }

    public void SendUserFeedback(string name, string email, string body)
    {
        if (!Available())
            return;

        LogEventRequest req = new LogEventRequest();
        req.SetEventKey("UserSendFeedback");
        req.SetEventAttribute("Name", name);
        req.SetEventAttribute("Email", email);
        req.SetEventAttribute("Body", body);
        req.SetEventAttribute("Build", GameSession.GetBuildNumber());
        req.SetEventAttribute("Platform", Application.platform.ToString());
        req.SetDurable(true);
        req.Send((response) =>
        {
            if (response.HasErrors)
                Debug.LogWarning("failed to send user feedback");
            else
                Debug.Log("user feedback sent successfully");
        });
    }

        #region Saga levels update
    public void CheckForContentPackUpdates(Action downloadCompleteCallback)
    {
        if (!Available())
            return;

        LogEventRequest req = new LogEventRequest();
        req.SetEventKey("GetContentUpdateLink");
        req.SetEventAttribute("SupportedFormat", TableDescription.CURRENT_VERSION);
        req.SetEventAttribute("CurrentVersion", GameSession.GetContentVersion());
        req.Send((response) =>
        {
            if (response.HasErrors)
            {
                Debug.LogError("[GameSparksManager] failed to check for level updates");
            }
            else
            {
                if (response.ScriptData != null && response.ScriptData.ContainsKey("link"))
                {
                    string url = response.ScriptData.GetString("link");
                    if (url.Length > 0)
                        StartCoroutine(DownloadContentPack(url, downloadCompleteCallback));
                    else
                        Debug.Log("no new content found");
                }
                else
                {
                    Debug.Log("no new content found");
                }
            }
        });
    }

    public IEnumerator DownloadContentPack(string url, Action downloadCompleteCallback)
    {
        WWW www = new WWW(url);

        while (!www.isDone)
            yield return null;

        //string fullPath = Application.persistentDataPath + "/Levels.zip";
        //File.WriteAllBytes(fullPath, www.bytes);

        //yield return null;

        ExtractZipFile(new MemoryStream(www.bytes));

        if (downloadCompleteCallback != null)
            downloadCompleteCallback();
    }

    protected bool ExtractZipFile(Stream stream)
    {
        string DataPath = ContentManager.DownloadPath;
        string debugStringPrefix = "[GS Unzip]";
        try
        {
            using (ZipInputStream zistream = new ZipInputStream(stream))
            {
                ZipEntry theEntry;
                while ((theEntry = zistream.GetNextEntry()) != null)
                {
                    if (theEntry.IsDirectory)
                    {
                        string path2 = Path.Combine(DataPath, theEntry.Name);
                        DirectoryInfo dir = new DirectoryInfo(path2);

                        if (dir.Exists)
                            dir.Delete(true);
                        dir.Create();
                    }
                    else
                    {
                        string fileName = Path.Combine(DataPath, theEntry.Name);

                        if (File.Exists(fileName))
                            File.Delete(fileName);

                        using (FileStream streamWriter = File.Create(fileName))
                        {
                            int size;
                            byte[] data = new byte[2048];
                            while ((size = zistream.Read(data, 0, data.Length)) > 0)
                                    streamWriter.Write(data, 0, size);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(debugStringPrefix + " ExtractZipFile: error unzipping file. Exception: " + ex.Message);
            return false;
        }

        return true;
    }
        #endregion

    public bool Available()
    {
        //Debug.Log("internet: " + Application.internetReachability + ", avail: " + GS.Available + ", auth: " + GS.Authenticated);
        return (
            Application.internetReachability != NetworkReachability.NotReachable &&
            GS.Available &&
            GS.Authenticated
            );
    }
}
#else
    public class GameSparksManager : MonoBehaviour
    {
        private static GameSparksManager instance = null;

        public static GameSparksManager Instance
        {
            get { return instance; }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Debug.LogWarning("Another GameSparksManager found in scene, self-destruct");
                Destroy(this.gameObject);
            }
        }

        public void RecordAnalytics(string key, params string[] parameters)
        {
        }
    }
#endif