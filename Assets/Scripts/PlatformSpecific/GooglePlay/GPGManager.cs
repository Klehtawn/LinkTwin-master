using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif
using UnityEngine.SocialPlatforms;
using System;
using System.IO;

public class GPGManager : MonoBehaviour {

	public static GPGManager Instance = null;

    public bool mAuthenticated = false;

#if UNITY_ANDROID && enableGPGServices

#if enableGPGAchievements
    public const string ACH_FILE_NAME = "sysfile.cff";

	public const int GP_ACH_CHAPTERS = -1;
	public const int GP_ACH_CHAPTER_1 = 0;
	public const int GP_ACH_CHAPTER_2 = 1;
	public const int GP_ACH_CHAPTER_3 = 2;
	public const int GP_ACH_CHAPTER_4 = 3;
	public const int GP_ACH_CHAPTER_5 = 4;
	public const int GP_ACH_5_BUTTERFLIES			= 5;
	public const int GP_ACH_30_JEWELS_CONNECTION	= 6;
	public const int GP_ACH_5_JEWELS_IN_LOOP		= 7;
	public const int GP_ACH_USE_5_POWERUPS			= 8;
	public const int GP_ACH_UNLOCK_ALL_POWERUPS		= 9;
	public const int COUNT							= 10;

	public const int ACH_SUBMIT_DISABLED	= -1;
	public const int ACH_TYPE_MAP			= 0;
	public const int ACH_TYPE_INGAME		= 1;
	public const int ACH_TYPE_ALL			= 2;

    private int activeFilter = ACH_SUBMIT_DISABLED;

	public class CFFAchievement
	{
		public CFFAchievement(string _id, int _progress, int _threshold, bool _reported, int _type)
		{
			ID = _id;
			progress = _progress;
			threshold = _threshold;
			reported = _reported;
			type = _type;
		}
		public string ID;
		public int progress;
		public int threshold;
		public bool reported;
		public int type;
	};

	const double ACH_UPDATE_IN_SECONDS = 5.0f;
	long lastAchSubmitTime = System.DateTime.Now.Ticks;
	bool needAchSubmission = false;

    public static CFFAchievement[] CFFAchievements = null;
    	public int AchFilter {
		get {
			return activeFilter;
		}
		
		set {
			if (Authenticated)
				activeFilter = value;
			else
				activeFilter = ACH_SUBMIT_DISABLED;
		}
	}
#endif
    private bool mAuthenticating = false;

	// cloud save callbacks
	private bool mSaving;
	private Texture2D mScreenImage;


	private const string silentAuthenticationPrefKey = "GPGTrySilentAuthentication";
	private bool trySilentAuthentication = false;
	
	public bool CanTrySilentAuthentication {
		get {
			return trySilentAuthentication;
		}
		
		private set {
			trySilentAuthentication = value;
			PlayerPrefs.SetInt(silentAuthenticationPrefKey, System.Convert.ToInt32(trySilentAuthentication));
			PlayerPrefs.Save();
		}
	}

	public bool IsAuthenticating {
		get {
			return mAuthenticating;
		}
	}

	private bool CanRetrySilentAuthentication ()
	{
		bool retry = trySilentAuthentication && Application.internetReachability != NetworkReachability.NotReachable;
		return retry;
	}

	
	// Use this for initialization
	void Start () {
		
	}
	
	void Awake()
	{
		if (Instance != null)
		{
			GameObject.Destroy(gameObject);
			return;
		}
		
		Instance = this;
		DontDestroyOnLoad(gameObject);

#if enableGPGAchievements
        if (CFFAchievements == null)
		{
			CFFAchievements = new CFFAchievement[COUNT];
		}

		CFFAchievements[GP_ACH_CHAPTER_1] =				new CFFAchievement("CgkI-p7p5ZUSEAIQAQ", 0, -1, false, ACH_TYPE_MAP);
		CFFAchievements[GP_ACH_CHAPTER_2] =				new CFFAchievement("CgkI-p7p5ZUSEAIQAg", 0, -1, false, ACH_TYPE_MAP);
		CFFAchievements[GP_ACH_CHAPTER_3] =				new CFFAchievement("CgkI-p7p5ZUSEAIQAw", 0, -1, false, ACH_TYPE_MAP);
		CFFAchievements[GP_ACH_CHAPTER_4] =				new CFFAchievement("CgkI-p7p5ZUSEAIQBA", 0, -1, false, ACH_TYPE_MAP);
		CFFAchievements[GP_ACH_CHAPTER_5] =				new CFFAchievement("CgkI-p7p5ZUSEAIQBQ", 0, -1, false, ACH_TYPE_MAP);
		CFFAchievements[GP_ACH_5_BUTTERFLIES] =			new CFFAchievement("CgkI-p7p5ZUSEAIQBg", 0, 5, false, ACH_TYPE_INGAME);
		CFFAchievements[GP_ACH_30_JEWELS_CONNECTION] =	new CFFAchievement("CgkI-p7p5ZUSEAIQBw", 0, 30, false, ACH_TYPE_INGAME);
		CFFAchievements[GP_ACH_5_JEWELS_IN_LOOP] =		new CFFAchievement("CgkI-p7p5ZUSEAIQCA", 0, 5, false, ACH_TYPE_INGAME);
		CFFAchievements[GP_ACH_USE_5_POWERUPS] =		new CFFAchievement("CgkI-p7p5ZUSEAIQCQ", 0, 5, false, ACH_TYPE_INGAME);
		CFFAchievements[GP_ACH_UNLOCK_ALL_POWERUPS] =	new CFFAchievement("CgkI-p7p5ZUSEAIQCg", 0, -1, false, ACH_TYPE_ALL);

		LoadLocalAchievementProgress();
#endif
		// Enable/disable logs on the PlayGamesPlatform
		PlayGamesPlatform.DebugLogEnabled = true; //false;
		
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
			.Build();
		PlayGamesPlatform.InitializeInstance(config);
		
		// Activate the Play Games platform. This will make it the default
		// implementation of Social.Active
		PlayGamesPlatform.Activate();

		// Set the default leaderboard for the leaderboards UI
		//((PlayGamesPlatform) Social.Active).SetDefaultLeaderboardForUI(GameIds.LeaderboardId);

		if (PlayerPrefs.HasKey(silentAuthenticationPrefKey))
		{
			trySilentAuthentication = System.Convert.ToBoolean(PlayerPrefs.GetInt(silentAuthenticationPrefKey));
		}
	}

	public bool Authenticated {
		get {
			mAuthenticated = Social.Active.localUser.authenticated;
			return mAuthenticated;
		}
	}

	public void Authenticate(bool bUserLogin = false) {
        if (Authenticated || mAuthenticating) {
            Debug.LogWarning("Ignoring repeated call to Authenticate().");
            return;
        }

        // Sign in to Google Play Games
        mAuthenticating = true;
        if (bUserLogin)
        {
            //the user pressed Login
            Debug.Log("GPGManager Authenticate");

            Social.localUser.Authenticate((bool success) =>
            {
                ProcessAuthentication(success);
            });
        }
        else
        {
            //try silent login
            PlayGamesPlatform.Instance.Authenticate((bool success) =>
            {
                ProcessAuthentication(success);
            }, true);
        }
    }

    private void ProcessAuthentication(bool success)
    {
        Debug.Log("GPGManager ProcessAuthentication " + success);
        if (success) {
			CanTrySilentAuthentication = true;
#if enableGPGAchievements
            AchFilter = ACH_TYPE_ALL;
#endif
			mAuthenticated = true;
        } else {
            // no need to show error message (error messages are shown automatically
            // by plugin)
            Debug.LogWarning("Failed to sign in with Google Play Games.");
			CanTrySilentAuthentication = false;
#if enableGPGAchievements
            AchFilter = ACH_SUBMIT_DISABLED;
#endif
            mAuthenticated = false;
        }
		mAuthenticating = false;
    }

	public void CaptureScreenshot() {
		mScreenImage = new Texture2D(Screen.width, Screen.height);
		mScreenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		mScreenImage.Apply();
		//Debug.Log ("Captured screen: " + mScreenImage);
	}

	public void SignOut() {
		((PlayGamesPlatform) Social.Active).SignOut();
		CanTrySilentAuthentication = false;
		mAuthenticated = false;
	}

#if enableGPGAchievements
	private void SaveLocalAchievementProgress()
	{
		Debug.Log("[GPGManager.SaveLocalAchievementProgress] enter");

#if !UNITY_EDITOR
		string achFilepath = Application.persistentDataPath + "/" + ACH_FILE_NAME;
#else
		string achFilepath = "StoredData/" + ACH_FILE_NAME;
#endif
		Debug.Log("[GPGManager.SaveLocalAchievementProgress] achievements will be saved to = " + achFilepath);


		MemoryStream ms = new MemoryStream();
		BinaryWriter writer = new BinaryWriter(ms);

		int tempInt = COUNT;
		writer.Write(tempInt);

		for (int i = 0; i < COUNT; i++)
		{
			writer.Write(CFFAchievements[i].progress);
			writer.Write(CFFAchievements[i].reported ? 1 : 0);
		}

		byte[] ms_array = ms.ToArray();

		writer.Close();
		ms.Close();
		Debug.Log("[GPGManager.SaveLocalAchievementProgress] achievements binarized, writing to file");

		System.IO.File.WriteAllBytes(achFilepath, ms_array);
		Debug.Log("[GPGManager.SaveLocalAchievementProgress] achievements written to " + achFilepath);
	}

	private void LoadLocalAchievementProgress()
	{
#if !UNITY_EDITOR
		string achFilepath = Application.persistentDataPath + "/" + ACH_FILE_NAME;
#else
		string achFilepath = "StoredData/" + ACH_FILE_NAME;
#endif

		if (!File.Exists(achFilepath))
		{
			for (int i = 0; i < COUNT; i++)
			{
				CFFAchievements[i].progress = 0;
				CFFAchievements[i].reported = false;
			}

			return;
		}
		
		Stream stream = File.OpenRead(achFilepath);
		BinaryReader reader = new BinaryReader(stream);

		long stream_length = stream.Length;
		byte[] ms_array = new byte[stream_length];
		reader.Read(ms_array, 0, (int)(stream_length));

		int index = 0;

		int tempInt = BitConverter.ToInt32(ms_array, index);
		index += 4;
		
		if (tempInt != COUNT)
		{
			/* fatal error; treat this somehow */
		}
		else
		{
			for (int i = 0; i < COUNT; i++)
			{
				CFFAchievements[i].progress = BitConverter.ToInt32(ms_array, index);
				index += 4;
				tempInt = BitConverter.ToInt32(ms_array, index);
				index += 4;
				CFFAchievements[i].reported = (tempInt == 1);
			}
		}

		reader.Close();
		stream.Close();
	}

	private int GetIndexForString(string achievementID)
	{
		for (int cursor = GP_ACH_CHAPTER_1; cursor < COUNT; cursor++)
		{
			string val = CFFAchievements[cursor].ID;

			if (val.CompareTo(achievementID) == 0)
				return cursor;
		}

		return GP_ACH_CHAPTERS; /* -1 as error */
	}

	private bool UpdateLocalLevelAchievements()
	{
		bool needLocalSave = false;
		int lastLevelPlayed = UserData.Instance.lastLevelUnlocked;
		while (lastLevelPlayed > 0 && UserData.Instance.GetLevelStars(lastLevelPlayed) <= 0)
		{
			lastLevelPlayed--;
		}
		lastLevelPlayed++; //it's indexed from 0, so add 1 for the true level number
		
		Debug.Log("[AchievementData.CheckLevelPlayAchievements] last played level = " + lastLevelPlayed);

		int reachedChapter = lastLevelPlayed / 20;

		for (int i = 1; i <= reachedChapter && i <= GP_ACH_CHAPTER_5 + 1; i++)
		{
			if (CFFAchievements[i - 1].progress != 100)
			{
				CFFAchievements[i - 1].progress = 100;
				needLocalSave = true;
			}
		}
		return needLocalSave;
	}
	
	public void UpdateLocalAchievement (int achID, int progress = 1)
	{
		bool needLocalSave = false;
		switch (achID)
		{
		case GP_ACH_5_BUTTERFLIES:
			Debug.Log("[AchievementData.CheckAchievement] Obtain 5 Magic Butterflies, one more butterfly obtained");
			if (CFFAchievements[achID].progress < 100)
			{
				CFFAchievements[achID].progress += 1;
				if (CFFAchievements[achID].progress >= CFFAchievements[achID].threshold)
				{
					CFFAchievements[achID].progress = 100; // mark as completed 
                }
                needLocalSave = true;
			}
			break;
		case GP_ACH_30_JEWELS_CONNECTION:
			Debug.Log("[AchievementData.CheckAchievement] Connect 30 jewels in a single connection, current connection length = " + progress);
			if (progress >= CFFAchievements[achID].threshold && CFFAchievements[achID].progress != 100)
			{
				CFFAchievements[achID].progress = 100;
				needLocalSave = true;
			}
			break;
		case GP_ACH_5_JEWELS_IN_LOOP:
			Debug.Log("[AchievementData.CheckAchievement] Break 5 jewels inside a line loop , jewels in current loop = " + progress);
			if (progress >= CFFAchievements[achID].threshold && CFFAchievements[achID].progress != 100)
			{
				CFFAchievements[achID].progress = 100;
				needLocalSave = true;
			}
			break;
		case GP_ACH_USE_5_POWERUPS:
			Debug.Log("[AchievementData.CheckAchievement] Use 5 power-ups : used one more powerup");
			if (CFFAchievements[achID].progress != 100)
			{
				CFFAchievements[achID].progress += 1;
				if (CFFAchievements[achID].progress >= CFFAchievements[achID].threshold)
				{
					CFFAchievements[achID].progress = 100; // mark as completed
				}
				needLocalSave = true;
			}
			break;
		case GP_ACH_UNLOCK_ALL_POWERUPS:
			bool isCompleted = RewardManager.Instance.GetDidUnlockAllPowerups();
			Debug.Log("[AchievementData.CheckAchievement] Did unlock all power-ups = " + (isCompleted ? "YES" : "NO"));
			if (isCompleted && CFFAchievements[achID].progress != 100)
			{
				CFFAchievements[achID].progress = 100;
				needLocalSave = true;
			}
			break;
		case GP_ACH_CHAPTERS:
		case GP_ACH_CHAPTER_1:
		case GP_ACH_CHAPTER_2:
		case GP_ACH_CHAPTER_3:
		case GP_ACH_CHAPTER_4:
		case GP_ACH_CHAPTER_5:
			needLocalSave = UpdateLocalLevelAchievements();
			break;
		}
		if (needLocalSave)
		{
            SaveLocalAchievementProgress();
			needAchSubmission = true;
		}
	}

	public void SubmitAchievements()
	{
		//Debug.Log("[CFFAchievement] activeFilter = " + activeFilter + ", needAchSubmission = " + (needAchSubmission ? "YES" : "NO"));
		if (activeFilter == ACH_SUBMIT_DISABLED || !needAchSubmission)
			return;

		long nowTime = System.DateTime.Now.Ticks;
		long elapsedTime = nowTime - lastAchSubmitTime;
		TimeSpan elapsedSpan = new TimeSpan(elapsedTime);

		if (elapsedSpan.TotalSeconds < ACH_UPDATE_IN_SECONDS)
		{
			//Debug.Log("[CFFAchievement] RETURNING, " + ACH_UPDATE_IN_SECONDS + " seconds did not pass since last submission");
			return;
		}

		bool needLocalSave = false;
		bool needRemoteSave = false;
		int achID = -1;

		for (int i = GP_ACH_CHAPTER_1; i < COUNT && needRemoteSave == false; i++)
		{
			if (CFFAchievements[i].type != ACH_TYPE_ALL && activeFilter != ACH_TYPE_ALL && CFFAchievements[i].type != activeFilter)
			{
				//Debug.Log("[CFFAchievement] Skipping achievement " + i + " of type " + CFFAchievements[i].type + ", activeFilter = " + activeFilter);
				continue;
			}

			if (CFFAchievements[i].reported && CFFAchievements[i].progress == 100)
			{
				//Debug.Log("[CFFAchievement] Skipping achievement " + i
				//          + " , reported = " + (CFFAchievements[i].reported ? "YES" : "NO")
				//          + " , progress = " + CFFAchievements[i].progress);
				continue;
			}
			if (CFFAchievements[i].reported && CFFAchievements[i].progress != 100)
			{
				//Debug.Log("[CFFAchievement] Found already reported achievement " + i
				//          + " , reported = " + (CFFAchievements[i].reported ? "YES" : "NO")
				//          + " , progress = " + CFFAchievements[i].progress);

				CFFAchievements[i].progress = 100;
				needLocalSave = true;
			}
			else if (!CFFAchievements[i].reported && CFFAchievements[i].progress == 100)
			{
				//Debug.Log("[CFFAchievement] Will report achievement " + i
				//          + " , reported = " + (CFFAchievements[i].reported ? "YES" : "NO")
				//          + " , progress = " + CFFAchievements[i].progress);

				achID = i;
				needRemoteSave = true;
			}
		}

		if (needRemoteSave)
		{
			if (Authenticated)
			{
				//Debug.Log("[CFFAchievement] Reporting achievement " + achID
				//          + " , reported = " + (CFFAchievements[achID].reported ? "YES" : "NO")
				//          + " , progress = " + CFFAchievements[achID].progress);

				Social.ReportProgress(CFFAchievements[achID].ID, 100.0f, (bool success) => {
					if (success)
					{
						CFFAchievements[achID].reported = true;
						//Debug.Log("[CFFAchievement] Reported achievement " + achID
						//          + " , reported = " + (CFFAchievements[achID].reported ? "YES" : "NO")
						//          + " , progress = " + CFFAchievements[achID].progress);
					}
					if (needLocalSave)
						SaveLocalAchievementProgress();
					lastAchSubmitTime = System.DateTime.Now.Ticks;
				});
			}
			else
			{
				//Debug.Log("[CFFAchievement] CAN'T REPORT achievement " + achID
				//          + " , reported = " + (CFFAchievements[achID].reported ? "YES" : "NO")
				//          + " , progress = " + CFFAchievements[achID].progress
				//          + ", NOT AUTHENTICATED INTO GP");
			}
		}
		else
		{
			Debug.Log("[CFFAchievement] NO achievement to report, temp. disabling submission ");
			needAchSubmission = false;
		}
	}

	void Update()
	{
		SubmitAchievements();
	}
#endif

#endif
}
