using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public class GameSession
{
    class LevelInfo
    {
        public bool unlocked = false;
        public bool finished = false;
        public byte stars = 0;
        public int numberOfTries = 0;

        public void Reset()
        {
            unlocked = false;
            finished = false;
            stars = 0;
            numberOfTries = 0;
        }
    };

    class ChapterInfo
    {
        public LevelInfo[] levels = new LevelInfo[23]; // 20 + 3 bonus levels

        public ChapterInfo()
        {
            for(int i = 0; i < levels.Length; i++)
            {
                levels[i] = new LevelInfo();
                levels[i].Reset();
            }
        }

        public void ResizeLevelArray(int newSize)
        {
            int c = levels.Length;
            Array.Resize<LevelInfo>(ref levels, newSize);

            for(int i = c; i < newSize; i++)
            {
                levels[i] = new LevelInfo();
                levels[i].Reset();
            }
        }

        public void Reset()
        {
            foreach (LevelInfo li in levels)
                li.Reset();
        }
    };

    static ChapterInfo[] chaptersInfo = new ChapterInfo[10];

    public static void ResizeChapterInfo(int newSize)
    {
        if (newSize <= chaptersInfo.Length) return;

        int prevSize = chaptersInfo.Length;
        Array.Resize<ChapterInfo>(ref chaptersInfo, newSize);

        for(int i = prevSize; i < newSize; i++)
        {
            chaptersInfo[i] = new ChapterInfo();
        }
    }

    public static float gridUnit = 10.0f;
    public static float minSwipeLength = 0.4f; // centimeters; if DPI not avail, 90 is assumed

    //private static int numLevels = -1;

    public static byte[] customLevelBytes = null;
    public static string customLevelId = "";

    public static int currentPlaying = 0;
    public static int currentChapter = 0;

    public static int chapterMaxLevels = 25;

    public static bool unlockAll = false;

    private static int unlockBonusReward = 10;

    private static int buildNumber = 0;

    static void WriteFinishedLevels()
    {
        WriteLevelsList("FinishedLevelsChapter", finishedLevels);
    }

    static void WriteStarRating()
    {
        WriteLevelsList("LevelsStarRating", levelsStarRating);
    }

    public static void WriteUnlockedLevels()
    {
        WriteLevelsList("UnlockedLevelsChapter", unlockedLevels);
    }

    #region analytics counters
    public static string lastPlayedLevel = "Menu";
    private static int sessionCount;
    private static long sessionStartTime;
    private static int totalPlayTime;

    public static void StartSession()
    {
        sessionCount = PlayerPrefs.GetInt("SessionCount", 0) + 1;
        PlayerPrefs.SetInt("SessionCount", sessionCount);
        lastPlayedLevel = "Menu";
        totalPlayTime = PlayerPrefs.GetInt("TotalPlayTime", 0);
        sessionStartTime = DateTime.Now.Ticks;
    }

    public static void EndSession()
    {
        long timeThisSession = DateTime.Now.Ticks - sessionStartTime;
        PlayerPrefs.SetInt("TotalPlayTime", GetTotalTimePlayed());
    }

    public static int GetSessionCount()
    {
        return sessionCount;
    }

    // in seconds
    public static int GetTotalTimePlayed()
    {
        return totalPlayTime + (int)((DateTime.Now.Ticks - sessionStartTime) / (10 * 1000 * 1000));
    }

    public static void IgnoreNextInterrupt()
    {
        GameSparksManager.Instance.ignoreNextStart = true;
        GameSparksManager.Instance.ignoreNextPause = true;
        Desktop.main.ignoreNextPause = true;
        Desktop.main.ignoreNextStart = true;
    }
    #endregion

    static public void ResetProgress()
    {
        //SetProgressInfo(0, 0);

        for (int ci = 0; ci < chapters.Length; ci++)
        {
            unlockedLevels[ci].levels.Clear();
            finishedLevels[ci].levels.Clear();
            levelsStarRating[ci].levels.Clear();
        }

        WriteFinishedLevels();
        WriteUnlockedLevels();
        WriteStarRating();

        if (GameSparksManager.Instance.Available())
        {
            DesktopUtils.ShowLoadingIndicator();
            GameSparks.Api.Requests.LogEventRequest req = new GameSparks.Api.Requests.LogEventRequest();
            req.SetEventKey("ResetUserProgress").Send((response) =>
            {
                DesktopUtils.HideLoadingIndicator();
                if (response.HasErrors)
                {
                    Debug.LogWarning("failed reset progress");
                }
                else
                {
                    Debug.Log("user progress reset");
                }
            });
        }
        else
        {
            DesktopUtils.ShowLocalizedMessageBox("GS_CANNOT_CONNECT");
        }
    }

    static public void LoadLevel(int index, int chapter)
    {
        currentPlaying = index;
        currentChapter = chapter;
    }

    static public bool IncrementLevel()
    {
        if (!IsStandardLevel()) return false;

        LoadChapters();

        int currentLevel = currentPlaying;
        currentLevel++;

        int cc = chapters[currentChapter].levelCount;// Mathf.Min(chapters[currentChapter], chapterMaxLevels);
        if (currentLevel >= cc)
        {
            currentChapter++;
            if (currentChapter >= chapters.Length)
            {
                currentChapter = 0;
                currentLevel = 0;
                return false;
            }
            currentLevel = 0;
        }
        currentPlaying = currentLevel;

        UnlockCurrentLevel();

        //SetProgressInfo(currentChapter, currentLevel);

        GameSparksManager.Instance.UpdateUserProgress(currentChapter, currentLevel);

        return true;
    }

    //public static void SetProgressInfo(int chapter, int level)
    //{
    //    PlayerPrefs.SetInt("CurrentChapter", chapter);
    //    PlayerPrefs.SetInt("CurrentLevel", level);
    //}

    static public void LoadRandomLevel(Action onLoad)
    {
        if (GameSparksManager.Instance.Available())
        {
            DesktopUtils.ShowLoadingIndicator();
            GameSparks.Api.Requests.LogEventRequest req = new GameSparks.Api.Requests.LogEventRequest();
            req.SetEventKey("GetRandomLevel").Send((response) =>
            {
                DesktopUtils.HideLoadingIndicator();
                if (response.HasErrors)
                {
                    Debug.LogWarning("failed to get random level");
                }
                else
                {
                    GameSparks.Core.GSData levelData = response.ScriptData.GetGSData("level");
                    if (levelData != null)
                    {
                        string levelTextData = levelData.GetString("levelData");
                        GameSession.customLevelBytes = System.Convert.FromBase64String(levelTextData);
                        GameSession.customLevelId = levelData.GetString("id");
                        Debug.Log("retrieved level: " + GameSession.customLevelId);
                        if (onLoad != null)
                            onLoad();
                    }
                    else
                    {
                        DesktopUtils.ShowLocalizedMessageBox("GS_CANNOT_CONNECT");
                    }
                }
            });
        }
        else
        {
            DesktopUtils.ShowLocalizedMessageBox("GS_CANNOT_CONNECT");
        }
    }

    //static public int GetCurrentLevel()
    //{
    //    return PlayerPrefs.GetInt("CurrentLevel", 0);
    //}

    //static public int GetCurrentChapter()
    //{
    //    return PlayerPrefs.GetInt("CurrentChapter", 0);
    //}

    //static public int GetNumLevels()
    //{
    //    int c = 0;
    //    for (int i = 0; i < chapters.Length; i++)
    //        c += chapters[i];
    //    return c;
    //}

    public static string GetAppVersion()
    {
#if UNITY_EDITOR
        return Application.version + ".editor";
#else
        return Application.version;
#endif
    }

    public static int GetBuildNumber()
    {
        if (buildNumber == 0)
        {
            TextAsset buildInfoText = Resources.Load("svnrev") as TextAsset;
            if (buildInfoText == null)
                buildNumber = -1;
            else
                buildNumber = int.Parse(buildInfoText.text);
        }
        return buildNumber;
    }

    public static string GetBuildInfo()
    {
        TextAsset buildInfoText = Resources.Load("svnrev") as TextAsset;
        if (buildInfoText != null)
            return "ver " + GetAppVersion() + " build " + buildInfoText.text;
        else
            return "manual build, kill it with fire";
    }

    public static void SendLevelEvent(string eventName)
    {
        if (GameSparksManager.Instance.Available())
        {
            if (!IsUserLevel())
            {
                GameSparks.Api.Requests.LogEventRequest req = new GameSparks.Api.Requests.LogEventRequest();
                req.SetEventKey("SagaLevelEvent");
                req.SetEventAttribute("Event", eventName);
                req.SetEventAttribute("Level", currentPlaying + 1);
                req.SetEventAttribute("Chapter", currentChapter + 1);
                //req.SetEventAttribute("Path", TheGame.Instance.levelPath);
                req.SetEventAttribute("Moves", TheGame.Instance.moves);
                req.SetEventAttribute("Time", (Time.realtimeSinceStartup - TheGame.Instance.startTime).ToString());
                req.SetEventAttribute("Version", GetAppVersion());
                req.SetEventAttribute("Build", GetBuildNumber());
                req.Send((response) =>
                {
                    if (response.HasErrors)
                        Debug.LogWarning("failed to send level event");
                    else
                        Debug.Log("successfully sent level event");
                });
            }
        }
    }

    public static void RateCurrentLevel(int rating)
    {
        if (GameSparksManager.Instance.Available())
        {
            DesktopUtils.ShowLoadingIndicator();
            GameSparks.Api.Requests.LogEventRequest req = new GameSparks.Api.Requests.LogEventRequest();
            if (IsUserLevel())
            {
                req.SetEventKey("RateUserLevel");
                req.SetEventAttribute("Level", customLevelId);
                req.SetEventAttribute("Rating", rating);
                req.Send((response) =>
                {
                    DesktopUtils.HideLoadingIndicator();
                    if (response.HasErrors)
                        Debug.LogWarning("failed to rate level");
                    else
                        Debug.Log("level successfully rated");
                });
            }
            else
            {
                req.SetEventKey("RateSagaLevel");
                req.SetEventAttribute("Level", currentPlaying + 1);
                req.SetEventAttribute("Path", TheGame.Instance.levelPath);
                req.SetEventAttribute("Rating", rating);
                req.SetEventAttribute("Version", GetAppVersion());
                req.Send((response) =>
                {
                    DesktopUtils.HideLoadingIndicator();
                    if (response.HasErrors)
                        Debug.LogWarning("failed to rate level");
                    else
                        Debug.Log("level successfully rated");
                });
            }
        }

    }

    public static bool GetSettingForMusic()
    {
        return PlayerPrefs.GetInt("Music", 1) == 1;
    }

    public static void SetSettingForMusic(bool musicIsOn)
    {
        PlayerPrefs.SetInt("Music", musicIsOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool GetSettingForSoundEffects()
    {
        return PlayerPrefs.GetInt("SoundEffects", 1) == 1;
    }

    public static void SetSettingForSoundEffects(bool sfxIsOn)
    {
        PlayerPrefs.SetInt("SoundEffects", sfxIsOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool GetSettingForVibrations()
    {
        return PlayerPrefs.GetInt("Vibrations", 1) == 1;
    }

    public static void SetSettingForVibrations(bool vibrationsAreOn)
    {
        PlayerPrefs.SetInt("Vibrations", vibrationsAreOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool GetSettingForNotifications()
    {
        return PlayerPrefs.GetInt("Notifications", 1) == 1;
    }

    public static void SetSettingForNotifications(bool notificationsAreOn)
    {
        PlayerPrefs.SetInt("Notifications", notificationsAreOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public enum GameState
    {
        Default,
        Playing,
        Workshop
    }

    public static GameState gameState = GameState.Default;

    public struct Chapter
    {
        public int levelCount;
        public int bonusLevelCount;

        public Chapter(int l, int b)
        {
            levelCount = l;
            bonusLevelCount = b;
        }
    }

    public static Chapter[] chapters;
    public static int chaptersVersion = -1;

    public class LevelsList
    {
        public List<int> levels = new List<int>();
    }

    public static LevelsList[] unlockedLevels;
    public static LevelsList[] finishedLevels;
    public static LevelsList[] levelsStarRating;
    private static int mopubLevelCnt = 0;

    public static int GetContentVersion()
    {
        LoadChapters();
        return chaptersVersion;
        //string text = ContentManager.LoadTextFile("Levels/chapters");
        //string[] lines = text.Split('\n');
        //return int.Parse(lines[0]);
    }

    public static void LoadChapters()
    {
        string text = ContentManager.LoadTextFile("Levels/chapters");
        //string[] lines = text.Split('\n');
        //int version = int.Parse(lines[0]);
        //int numChapters = int.Parse(lines[1]);
        //chapters = new Chapter[numChapters];
        //for (int i = 0; i < numChapters; i++)
        //{
        //    chapters[i] = int.Parse(lines[i + 2]);
        //}

        JsonReader reader = new JsonReader(text);

        JsonData data = JsonMapper.ToObject(reader);

        chaptersVersion = (int)data["version"];

        JsonData chapterData = data["chapters"];
        int numChapters = chapterData.Count;
        chapters = new Chapter[numChapters];
        for (int i = 0; i < numChapters; i++)
        {
            JsonData c = chapterData[i];
            chapters[i] = new Chapter((int)c["levels"], (int)c["bonus"]);
        }

        ChapterInfo ci = new ChapterInfo();

        //throw new NotImplementedException("implement chapter data read");
        ReadLevelsList("UnlockedLevelsChapter", ref unlockedLevels);
        ReadLevelsList("FinishedLevelsChapter", ref finishedLevels);
        ReadLevelsList("LevelsStarRating", ref levelsStarRating);

        if (IsLevelUnlocked(0, 0) == false)
            UnlockLevel(0, 0);
    }

    //public static void UnlockAllTo(int chapter, int level)
    //{
    //    int levelindex = 0;
    //    int chapterindex = 0;
    //    while (chapterindex < chapter)
    //    {
    //        while (levelindex < chapters[chapterindex])
    //        {
    //            UnlockLevelWithoutSaving(chapterindex, levelindex);
    //            levelindex++;
    //        }
    //        chapterindex++;
    //        levelindex = 0;
    //    }
    //    while (levelindex <= level)
    //    {
    //        UnlockLevelWithoutSaving(chapterindex, levelindex);
    //        levelindex++;
    //    }

    //    WriteLevelsList("UnlockedLevelsChapter", unlockedLevels);
    //}

    //private static void UnlockLevelWithoutSaving(int chapterIndex, int levelIndex)
    //{
    //    if (chapterIndex >= unlockedLevels.Length)
    //    {
    //        Array.Resize<LevelsList>(ref unlockedLevels, chapterIndex + 1);
    //    }

    //    if (unlockedLevels[chapterIndex].levels.Contains(levelIndex) == false)
    //        unlockedLevels[chapterIndex].levels.Add(levelIndex);
    //}

    public static int GetStartIndexForChapter(int chapterIndex)
    {
        if (chapters == null) return 0;
        if (chapterIndex >= chapters.Length) return -1;

        int i = 0;
        int ci = 0;
        while (ci < chapterIndex)
        {
            i += chapters[ci].levelCount;// Mathf.Min(chapters[ci].levelCount, chapterMaxLevels);
            ci++;
        }

        return i;
    }

    public static bool IsLevelUnlocked(int chapterIndex, int levelIndex)
    {
        if (unlockAll) return true;

        if (chapterIndex >= unlockedLevels.Length) return false;

        return unlockedLevels[chapterIndex].levels.Contains(levelIndex);
    }

    public static bool IsLevelFinished(int chapterIndex, int levelIndex)
    {
        if (unlockAll) return true;

        if (chapterIndex >= finishedLevels.Length) return false;

        return finishedLevels[chapterIndex].levels.Contains(levelIndex);
    }

    public static void UnlockLevel(int chapterIndex, int levelIndex)
    {
        if(chapterIndex >= unlockedLevels.Length)
        {
            Array.Resize<LevelsList>(ref unlockedLevels, chapterIndex + 1);
        }

        if (unlockedLevels[chapterIndex].levels.Contains(levelIndex) == false)
            unlockedLevels[chapterIndex].levels.Add(levelIndex);

        WriteUnlockedLevels();

        GameSparksManager.Instance.UpdateProgressInfo();
    }

    static void UnlockCurrentLevel()
    {
        UnlockLevel(currentChapter, currentPlaying);
    }

    public static bool IsChapterComplete(int chapterIndex)
    {
        for (int i = 0; i < chapters[chapterIndex].levelCount; i++)
            if (!finishedLevels[chapterIndex].levels.Contains(i))
                return false;
        return true;
    }

    public static bool IsUserLevel()
    {
        return customLevelBytes != null;
    }

    public static bool IsBonusLevel()
    {
        Chapter chapter = chapters[currentChapter];
        return currentPlaying >= chapter.levelCount && !IsUserLevel();
    }

    public static bool IsStandardLevel()
    {
        if (chapters == null || currentChapter >= chapters.Length) return true;
        Chapter chapter = chapters[currentChapter];
        return currentPlaying < chapter.levelCount && !IsUserLevel();
    }

    public static bool HasFinishedChapter()
    {
        return HasFinishedChapter(currentChapter);
    }

    public static bool HasFinishedChapter(int index)
    {
        if (!IsStandardLevel()) return false;
        if (index < 0) return false;

        if (chapters == null) return true;

        int nextLevel = currentPlaying + 1;

        int cc = chapters[index].levelCount;

        return nextLevel >= cc;
    }

    public static void UnlockChapter(int index)
    {
        UnlockLevel(index, 0);
        GameEvents.Send(GameEvents.EventType.ChapterUnlock);
    }

    public static void UnlockNextLevel()
    {
        if (!IsStandardLevel()) return;

        int nextLevel = currentPlaying + 1;
        int nextChapter = currentChapter;

        int cc = chapters[currentChapter].levelCount;// Mathf.Min(chapters[currentChapter], chapterMaxLevels);

        if (nextLevel >= cc)
        {
            nextChapter++;
            nextLevel = 0;
            if (nextChapter >= chapters.Length)
            {
                nextChapter = 0;
            }
//#if UNITY_ANDROID || UNITY_IOS
//            AdUtils.rewardShowEvent += UnlockBonusLevels;
//            AdUtils.Instance.InitVideo(AdUtils.Instance.GetAdId(AdUtils.Location.UnlockBonus));
//#else
//            UnlockBonusLevels(false);
//#endif
//            UnlockBonusLevels(false);
        }
        else if(++mopubLevelCnt >= 5)
        {
#if UNITY_ANDROID || UNITY_IOS
            AdUtils.Instance.InitInterstitial(AdUtils.Instance.GetAdId(AdUtils.Location.LevelSuccess));
#endif
            mopubLevelCnt = 0;
        }
        UnlockLevel(nextChapter, nextLevel);
    }

    public static void UnlockBonusLevels(bool success)
    {
#if UNITY_ANDROID || UNITY_IOS
        AdUtils.rewardShowEvent -= UnlockBonusLevels;
#endif
        //int chapter = currentChapter - 1;
        //if (currentChapter < 0)
        //    currentChapter = chapters.Length - 1;

        if (success)
        {
            GameEconomy.currency += unlockBonusReward;
        }

        for (int i = 0; i < chapters[currentChapter].bonusLevelCount; i++)
            UnlockLevel(currentChapter, chapters[currentChapter].levelCount + i);

        GameEconomy.SaveLocal();
    }

    static void ReadLevelsList(string keyPrefix, ref LevelsList[] list)
    {
        list = new LevelsList[chapters.Length];

        for (int i = 0; i < chapters.Length; i++)
        {
            list[i] = new LevelsList();

            string key = keyPrefix + (i + 1).ToString();
            if (PlayerPrefs.HasKey(key) == false) continue;

            string value = PlayerPrefs.GetString(key);
            if (value == null) continue;

            string[] values = value.Split(',');
            foreach (string v in values)
            {
                if (v.Length == 0) break;

                int l = int.Parse(v);
                list[i].levels.Add(l);
            }
        }
    }

    static void WriteLevelsList(string keyPrefix, LevelsList[] list)
    {
        for (int i = 0; i < list.Length; i++)
        {
            string values = "";

            foreach (int v in list[i].levels)
                values += v.ToString() + ",";

            string key = keyPrefix + (i + 1).ToString();
            PlayerPrefs.SetString(key, values);
        }
    }

    public static int GetMaxUnlockedLevel()
    {
        LoadChapters();
        for (int i = 0; i < unlockedLevels.Length; i++)
        {
            if (unlockedLevels[i].levels.Count == 0) break;
            if (unlockedLevels[i].levels.Count < chapters[i].levelCount)
                return unlockedLevels[i].levels.Count - 1;
        }

        return 0;
    }

    public static int GetMaxUnlockedChapter()
    {
        LoadChapters();
        int c = 0;
        for(int i = 0; i < unlockedLevels.Length; i++)
        {
            if (unlockedLevels[i].levels.Count == 0) break;
            c = i;
        }

        return c;
    }

    public static void MarkCurrentLevelAsFinished()
    {
        if (finishedLevels[currentChapter].levels.Contains(currentPlaying) == false)
            finishedLevels[currentChapter].levels.Add(currentPlaying);

        WriteFinishedLevels();
    }

    public static GameObject GetPreferredBackgroundForChapter()
    {
        if (IsUserLevel())
            return Resources.Load<GameObject>("Blocks/GameBackground");

        int cc = currentChapter;
        Transform bkg = ChapterThemes.GetChapterInGameBackground(cc);
        if(bkg != null)
            return bkg.gameObject;
        return Resources.Load<GameObject>("Blocks/GameBackground");
    }

    public static int GetSessionIndex()
    {
        return PlayerPrefs.GetInt("SessionIndex", 0);
    }

    public static void IncrementSessionIndex()
    {
        int si = GetSessionIndex();
        si++;
        PlayerPrefs.SetInt("SessionIndex", si);
    }

    public static bool IsChapterLocked(int chapter)
    {
        return unlockedLevels[chapter].levels.Count == 0;
    }

    #region Rate this app

#if UNITY_ANDROID
    private const string kRateThisAppUrl = "http://www.samsungapps.com/appquery/appDetail.as?appId=com.amberstudio.linktwin_sapps";
#else
    private const string kRateThisAppUrl = "www.amberstudio.com";
#endif

    private const string kRateThisAppDone = "RateThisAppDone";
    private const string kRateThisAppLast = "RateThisAppLast";
    private const string kRateThisAppCount = "RateThisAppCount";
    private const int kRateThisAppIntervalDays = 7;

    public static void TryToShowRateThisApp()
    {
        // --- check if either user already rated the app or pressed never
        if (PlayerPrefs.GetInt(kRateThisAppDone, 0) == 1)
            return;

        long lastTimeTicks = Convert.ToInt64(PlayerPrefs.GetString(kRateThisAppLast, "0"));
        DateTime lastTime = DateTime.FromBinary(lastTimeTicks);
        DateTime currentTime = DateTime.Now;

        TimeSpan diff = currentTime.Subtract(lastTime);
        int days = Mathf.FloorToInt((float)currentTime.Subtract(lastTime).TotalDays);

        //Debug.Log("last time rate this app shown: " + lastTime);
        //Debug.Log("time since rate this app last shown: " + diff);

        if (days < kRateThisAppIntervalDays)
            return;

        PlayerPrefs.SetString(kRateThisAppLast, currentTime.ToBinary().ToString());
        PlayerPrefs.SetInt(kRateThisAppCount, PlayerPrefs.GetInt(kRateThisAppCount, 0) + 1);

        RateThisApp rta = WindowA.Create("UI/RateThisApp") as RateThisApp;
        rta.OnWindowClosed += OnRateThisAppClosed;
        rta.ShowModal();
    }

    private static void OnRateThisAppClosed(WindowA sender, int retValue)
    {
        switch (retValue)
        {
            case (int)RateThisApp.ReturnValues.Now:
                PlayerPrefs.SetInt(kRateThisAppDone, 1);
                Application.OpenURL(kRateThisAppUrl);
                break;
            case (int)RateThisApp.ReturnValues.Later:
                break;
            case (int)RateThisApp.ReturnValues.Never:
                PlayerPrefs.SetInt(kRateThisAppDone, 1);
                break;
        }

        GameSparksManager.Instance.RecordAnalytics("RATING",
            "Display_Nb", PlayerPrefs.GetInt(kRateThisAppCount, 0),
            "Last_Map", GameSession.lastPlayedLevel,
            "Popup_Review_Action", retValue.ToString(),
            "Max_Chapter", GameSession.GetMaxUnlockedChapter(),
            "Max_Level", GameSession.GetMaxUnlockedLevel()
            );
    }

#endregion

#region Back key support

    public static bool BackKeyPressed()
    {
#if UNITY_EDITOR
        return Input.GetKeyDown(KeyCode.Backspace);
#elif UNITY_ANDROID
        return Input.GetKeyDown(KeyCode.Escape);
#else
        return false;
#endif
    }

    public static void AskQuitApplication()
    {
        ConfirmationBox.Show(Locale.GetString("QUIT_CONFIRM"), QuitApplication);
    }

    private static void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    static int getIndexForLevel(List<int> list, int level)
    {
        for(int i = 0; i < list.Count; i+=2)
        {
            if (list[i] == level)
                return i;
        }
        return -1;
    }
#endregion

    public static int GetStarsForLevel(int chapter, int level)
    {
        int index = getIndexForLevel(levelsStarRating[chapter].levels, level);
        if (index >= 0)
        {
            return levelsStarRating[chapter].levels[index + 1];
        }

        return 0;
    }

    public static int GetStarsForCurrentLevel()
    {
        return GetStarsForLevel(currentChapter, currentPlaying);
    }

    public static void SetStarsForLevel(int chapter, int level, int numStars)
    {
        int index = getIndexForLevel(levelsStarRating[chapter].levels, level);
        if (index >= 0)
        {
            levelsStarRating[chapter].levels[index + 1] = numStars;
        }
        else
        {
            levelsStarRating[chapter].levels.Add(level);
            levelsStarRating[chapter].levels.Add(numStars);
        }

        WriteStarRating();
    }

    public static void SetStarsForCurrentLevel(int numStars)
    {
        SetStarsForLevel(currentChapter, currentPlaying, numStars);
    }

    public static float GetRewardPerStartForLevel(int chapter, int level)
    {
        return 100.0f;
    }

    public static float GetRewardPerStartForCurrentLevel()
    {
        return GetRewardPerStartForLevel(currentChapter, currentPlaying);
    }
}

