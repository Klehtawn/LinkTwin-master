using UnityEngine;
using UnityEngine.UI;

public class ChapterSelectScreen : GameScreen
{
    public Widget settingsButton;
    public Button unlockAllButton;

    public Widget prevChapterButton, nextChapterButton;

    public GameObject levelButtonPrefab;
    public GameObject bonusLevelButtonPrefab;
    public GameObject chapterViewPrefab;

    ObjectPoolContext poolContext;

    RectTransform content;

    float contentInitialX = 0.0f;
    float contentTargetOffsetX = 0.0f;
    float chapterWidth = 0.0f;

    public Widget unlockChapterButton;
    public Text unlockChapterButtonCaption;
    public Text unlockChapterPriceText;

	protected override void Start()
    {
        base.Start();

        OnWindowClosed += _OnWindowClosed;

        settingsButton.transform.SetAsLastSibling();

        unlockAllButton.OnClick += OnUnlockAllPressed;

        content = transform.Find("Content/Chapters").GetComponent<RectTransform>();

        contentInitialX = content.anchoredPosition.x;
        contentTargetOffsetX = contentInitialX;

        FillChapters();

        nextChapterButton.OnClick += OnNextChapterClicked;
        prevChapterButton.OnClick += OnPrevChapterClicked;

        OnWindowStartClosing += _OnWindowStartClosing;
        OnWindowFinishedShowing += _OnWindowFinishedShowing;

        unlockChapterButton.active = false;
        unlockChapterButton.OnClick += OnUnlockChapterClicked;

        GameEvents.Send(GameEvents.Sender.System, GameEvents.EventType.ChapterSelectEntered, currentChapter.ToString());

        ShopScreen.RegisterEvents();
    }

    void CheckChapterUsage()
    {
        int chapterIndex = currentChapter + 1;
        string chapterStr = "Chapter" + chapterIndex.ToString() + "Usage";
        int chapterUsage = PlayerPrefs.GetInt(chapterStr, 0);
        chapterUsage++;
        PlayerPrefs.SetInt(chapterStr, chapterUsage);

        /*
        if(chapterUsage == 1) // first time
        {
            Destroy(gameObject);
            WindowA w = WindowA.Create("UI/Chapter" + chapterIndex + "Intro");
            if(w != null)
                w.Show();
        }
         */
    }

    void FillChapters()
    {
        if(poolContext != null)
        {
            poolContext.Cleanup();
        }

        Widget.DeleteAllChildren(content);

        GameSession.LoadChapters();
        poolContext = new ObjectPoolContext();

        for (int i = 0; i < GameSession.chapters.Length; i++)
        {
            GameObject obj = GameObject.Instantiate<GameObject>(chapterViewPrefab);
            obj.transform.SetParent(content);
            obj.transform.localScale = Vector3.one;
            obj.name = "Chapter" + (i + 1).ToString();
            RectTransform rt = obj.GetComponent<RectTransform>();
            chapterWidth = Desktop.main.width;// rt.rect.width;
            rt.anchoredPosition = Vector2.up * (content.rect.height - rt.rect.height) * 0.5f + Vector2.right * chapterWidth * (float)i;

            ChapterView cv = obj.GetComponent<ChapterView>();
            cv.chapterIndex = i;
            cv.Fill();

            cv.OnSwipe += OnChaptersSwipe;

            cv.unlockThisChapterButton.OnClick += OnUnlockThisChapter;
        }
    }

    bool firstChapterSet = false;

    protected override void Update()
    {
        base.Update();

        Vector2 p = content.anchoredPosition;
        p.x = Mathf.Lerp(p.x, contentTargetOffsetX, Time.deltaTime * 8.0f);
        content.anchoredPosition = p;

        if (GameSession.BackKeyPressed() && Desktop.main.IsWindowOnTop(this))
            GameSession.AskQuitApplication();

        if(firstChapterSet == false)
        {
            currentChapter = GameSession.GetMaxUnlockedChapter();
            CheckChapterUsage();

            firstChapterSet = true;
        }
    }

    void _OnWindowClosed(WindowA sender, int retValue)
    {
        poolContext.Cleanup();
    }

    void OnNextChapterClicked(MonoBehaviour sender, Vector2 pos)
    {
        currentChapter++;
    }

    void OnPrevChapterClicked(MonoBehaviour sender, Vector2 pos)
    {
        currentChapter--;
    }

    void _OnWindowFinishedShowing(WindowA sender)
    {
        // --- disable for RGDA
        //if (GameSession.GetMaxUnlockedLevel() >= 15)
        //    GameSession.TryToShowRateThisApp();
    }

    private int _currentChapter = -1;
    public int currentChapter
    {
        get
        {
            return _currentChapter;
        }
        set
        {
            int v = Mathf.Clamp(value, 0, GameSession.chapters.Length - 1);

            if (v != _currentChapter)
            {
                _currentChapter = v;
                contentTargetOffsetX = contentInitialX - (float)_currentChapter * chapterWidth;

                if(isStarted == false)
                {
                    content.anchoredPosition = new Vector2(contentTargetOffsetX, content.anchoredPosition.y);
                }
                OnChapterChanged();
            }
        }
    }

    bool unlocksNextChapter = true;
    void OnChapterChanged()
    {
        nextChapterButton.active = (_currentChapter < GameSession.chapters.Length - 1);
        prevChapterButton.active = (_currentChapter > 0);

        Desktop.main.SetBackground(ChapterThemes.GetChapterBackground(_currentChapter), 0.7f);

        if (currentChapter == GameSession.currentChapter && GameSession.HasFinishedChapter() && GameSession.IsChapterLocked(_currentChapter + 1) && _currentChapter < GameSession.chapters.Length - 1)
        {
            string s = Locale.GetString("UNLOCK_CHAPTER_X");
            s = s.Replace("{0}", RomanNumerals.FromArabic(_currentChapter + 2));

            unlockChapterButtonCaption.text = s;
            unlockChapterPriceText.text = GameEconomy.GetPriceForChapter(_currentChapter + 1).ToString();
            unlockChapterButton.active = true;
            unlocksNextChapter = true;
        }
        else
        {
            unlockChapterButton.active = false;
        }

        GameEvents.Send(GameEvents.Sender.System, GameEvents.EventType.ChapterSelectEntered, currentChapter.ToString());
    }

    void OnChaptersSwipe(MonoBehaviour sender, Vector2 p, float duration)
    {
        float threshold = Desktop.main.width * 0.05f;
        if (duration > 1.0f) return;

        if (p.x < -threshold)
            currentChapter++;

        if (p.x > threshold)
            currentChapter--;
    }

    void OnUnlockAllPressed(MonoBehaviour sender, Vector2 p)
    {
        GameSession.unlockAll = true;

        FillChapters();
    }

    void _OnWindowStartClosing(WindowA sender, int retValue)
    {
        //Desktop.main.SetBackground(null, 1.0f);
    }

    protected override void HandleCreateArgs(string args)
    {
        base.HandleCreateArgs(args);

        if(args == "bonuslevels")
        {
            ChapterView currentChapter = content.GetChild(GameSession.currentChapter).GetComponent<ChapterView>();
            currentChapter.ShowBonusLevels(true);
        }
        else
        {
            for(int i = 0; i < content.childCount; i++)
            {
                ChapterView cv = content.GetChild(i).GetComponent<ChapterView>();
                if (GameSession.IsChapterComplete(cv.chapterIndex))
                    cv.ShowBonusLevels(false);
            }
        }
    }

    void OnUnlockChapterClicked(MonoBehaviour mb, Vector2 p)
    {
        int nextChapterIndex = _currentChapter + 1;
        if (unlocksNextChapter == false)
            nextChapterIndex = _currentChapter;

        int chapterPrice = GameEconomy.GetPriceForChapter(nextChapterIndex);

        string msg = Locale.GetString("UNLOCK_CHAPTER_WITH_RUNES");
        msg = msg.Replace("{0}", RomanNumerals.FromArabic(nextChapterIndex + 1));

        UnlockChapterWithRunes.Show(msg, chapterPrice, () =>
            {
                if (GameEconomy.ConsumeCurrency(chapterPrice))
                {
                    GameSession.UnlockChapter(nextChapterIndex);
                    unlockChapterButton.active = false;

                    GameObject obj = Resources.Load<GameObject>("UI/IntroScreens/ChapterIntro" + (currentChapter + 2).ToString());

                    if (obj == null)
                    {
                        ChapterView nextChapter = content.GetChild(_currentChapter + 1).GetComponent<ChapterView>();
                        nextChapter.Fill();
                        currentChapter++;
                    }
                    else
                    {
                        Close();
                        WindowA.Create(obj.GetComponent<WindowA>()).Show();
                    }
                }
            });
    }

    void OnUnlockThisChapter(MonoBehaviour sender, Vector2 pos)
    {
        unlocksNextChapter = false;
        OnUnlockChapterClicked(sender, pos);
    }
}
