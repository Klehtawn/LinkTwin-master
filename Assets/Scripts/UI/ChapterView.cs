using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChapterView : Widget {

    public int chapterIndex = 0;

    public Text titleText;
    public Transform buttonsContainer;
    public Transform bonusLevelsContainer;

    ChapterSelectScreen chapterSelectScreen;

    public Transform lockedMsg;
    public Text priceStr1;
    public Text priceStr2;

    public Widget unlockThisChapterButton;
	
	protected override void Start () {

        base.Start();

        string stringID = "CHAPTER_" + (chapterIndex + 1).ToString() + "_TITLE";
        if (Locale.StringExists(stringID))
        {
            titleText.text = Locale.GetString(stringID);
        }
        else
        {
            titleText.text = string.Format(Locale.GetString("CHAPTER_N"), RomanNumerals.FromArabic(chapterIndex + 1));
        }
        priceStr1.text = "   " + GameEconomy.GetPriceForChapter(chapterIndex).ToString();
        priceStr2.text = priceStr1.text;
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
	}

    public void Fill()
    {
        if(chapterSelectScreen == null)
            chapterSelectScreen = GetComponentInParent<ChapterSelectScreen>();

        Widget.DeleteAllChildren(buttonsContainer);

        int levelCount = GameSession.chapters[chapterIndex].levelCount;
        int levelsOffset = GameSession.GetStartIndexForChapter(chapterIndex);

        int count = Mathf.Min(levelCount, GameSession.chapterMaxLevels);

        ObjectPool pool = ObjectPool.GetPool(chapterSelectScreen.levelButtonPrefab);
        if (pool == null)
            pool = ObjectPool.CreatePool(chapterSelectScreen.levelButtonPrefab, 100);
        

        for (int i = 0; i < count; i++)
        {
            GameObject obj = ObjectPool.Spawn(chapterSelectScreen.levelButtonPrefab);
            obj.transform.SetParent(buttonsContainer);
            obj.transform.localScale = Vector3.one;

            ChapterButton button = obj.GetComponent<ChapterButton>();

            button.levelIndex = i;
            button.levelIndexOffset = levelsOffset;
            button.Refresh();
            button.gameObject.name = "Level" + i.ToString();

            if(GameSession.IsLevelUnlocked(chapterIndex, i) == false)
            {
                button.SetButtonState(ChapterButton.ButtonState.Locked);
            }
            else
            {
                if (GameSession.IsLevelFinished(chapterIndex, i))
                {
                    button.SetButtonState(ChapterButton.ButtonState.Played);
                    button.starsCount = GameSession.GetStarsForLevel(chapterIndex, i);
                    button.Refresh();
                }
                else
                    button.SetButtonState(ChapterButton.ButtonState.Unlocked);

                button.OnClick = OnLevelButtonPressed;
            }
        }

        FillBonusLevels();

        if (GameSession.IsChapterLocked(chapterIndex))
        {
            if(GameSession.HasFinishedChapter(chapterIndex - 1))
            {
                lockedMsg.gameObject.SetActive(false);
                unlockThisChapterButton.gameObject.SetActive(true);
            }
            else
            {
                lockedMsg.gameObject.SetActive(true);
                unlockThisChapterButton.gameObject.SetActive(true);
            }
        }
        else
        {
            lockedMsg.gameObject.SetActive(false);
            unlockThisChapterButton.gameObject.SetActive(false);
        }

    }

    void FillBonusLevels()
    {
        GameSession.Chapter chapter = GameSession.chapters[chapterIndex];
        int levelCount = chapter.bonusLevelCount;
//        int levelsOffset = GameSession.GetStartIndexForChapter(chapterIndex);

        //int count =  Mathf.Min(levelCount - GameSession.chapterMaxLevels, 5);

        for (int i = 0; i < levelCount; i++)
        {
            GameObject obj = ObjectPool.Spawn(chapterSelectScreen.bonusLevelButtonPrefab);
            obj.transform.SetParent(bonusLevelsContainer);
            obj.transform.localScale = Vector3.one;

            ChapterButton button = obj.GetComponent<ChapterButton>();

            button.levelIndex = chapter.levelCount + i;
            button.levelIndexOffset = 0;
            button.Refresh();
            button.gameObject.name = "BonusLevel" + i.ToString();
            button.OnClick = OnBonusLevelButtonPressed;
            button.SetButtonState(ChapterButton.ButtonState.Locked);
        }
    }

    void OnLevelButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        GameSession.currentChapter = chapterIndex;
        GameSession.currentPlaying = sender.GetComponent<ChapterButton>().levelIndex;
        GameSession.customLevelBytes = null;
        chapterSelectScreen.OnWindowClosed += OnChapterSelectScreenClosed;
        chapterSelectScreen.Close(1234);

        //Desktop.main.SetBackground(Desktop.main.theme.blackBackground, 0.55f);
    }

    void OnBonusLevelButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        GameSession.currentChapter = chapterIndex;
        GameSession.currentPlaying = sender.GetComponent<ChapterButton>().levelIndex;
        GameSession.customLevelBytes = null;
        if (GameSession.IsLevelUnlocked(chapterIndex, GameSession.currentPlaying))
            PlayLevel();
        else
            ConfirmationBox.Show("WATCH_VIDEO_CONFIRM", OnWatchVideoConfirm);
    }

    private void PlayLevel()
    {
        chapterSelectScreen.OnWindowClosed += OnChapterSelectScreenClosed;
        chapterSelectScreen.Close(1234);

        //Desktop.main.SetBackground(Desktop.main.theme.blackBackground, 0.55f);
    }

    void OnWatchVideoConfirm()
    {
#if UNITY_ANDROID || UNITY_IOS
        AdUtils.rewardShowEvent += UnlockBonusLevels;
        AdUtils.Instance.InitVideo(AdUtils.Instance.GetAdId(AdUtils.Location.UnlockBonus));
#else
        UnlockBonusLevels(false);
#endif
    }

    void UnlockBonusLevels(bool success)
    {
        GameSession.UnlockBonusLevels(success);
        PlayLevel();
    }

    void OnChapterSelectScreenClosed(WindowA sender, int ret)
    {
        if (ret == 1234)
        {
            WindowA.Create("UI/NormalPlayScreen").Show();
            //if (GameSparksManager.Instance.Available())
            //{
            //    GameSparksManager.Instance.RecordAnalytics("EVT_PLAY_LEVEL", "level", "" + GameSession.currentPlaying);
            //    //            GameSparks.Api.Requests.LogEventRequest req = new GameSparks.Api.Requests.LogEventRequest();
            //}
        }
    }

    public void ShowBonusLevels(bool animated = false)
    {
        for (int i = 0; i < bonusLevelsContainer.childCount; i++ )
        {
            ChapterButton cb = bonusLevelsContainer.GetChild(i).GetComponent<ChapterButton>();
            cb.SetButtonState(ChapterButton.ButtonState.Unlocked);
            cb.transform.localScale = Vector3.one * 0.001f;

            if(animated)
            {
                float d = (float)i * 0.4f + 1.25f;
                LeanTween.scale(cb.gameObject, Vector3.one, 0.4f).setFrom(Vector3.one * 0.1f).setEase(LeanTweenType.easeOutBack).setDelay(d);
            }
        }
    }
}
