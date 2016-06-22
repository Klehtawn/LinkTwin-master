using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UserLevelsScreen : GameScreen
{
//    public Button prevButton;
//    public Button nextButton;

    public RectTransform levelsPanel;

    public GameObject levelButtonPrefab;

    public Text nothingToDisplay;

    public Text titleText;

    List<LevelInfo> levelInfoList = new List<LevelInfo>();
    List<UserLevelButton> levelButtons = new List<UserLevelButton>();

//    float buttonWidth;
//    float buttonHeight;

    private int offset = 0;
    private int queryLimit = 1;
//    private int pagesize = 4;
    private static string query = null;
    private static string request = "";

    private int numLevels;

    public string Query
    {
        set { query = value; }
    }

    public string Request
    {
        set { request = value; }
    }

    public bool ButtonDetailsEnabled { get; set; }

    protected override void Start()
    {
        base.Start();

        nothingToDisplay.gameObject.SetActive(true);

//        prevButton.OnTouchUp += OnPrevButtonPressed;
//        nextButton.OnTouchUp += OnNextButtonPressed;

//        buttonWidth = levelButtonPrefab.GetComponent<RectTransform>().sizeDelta.x;
//        buttonHeight = levelButtonPrefab.GetComponent<RectTransform>().sizeDelta.y;

        //pagesize = 100;// (int)(levelsPanel.rect.height / (buttonHeight * 1.2f));
        //Debug.Log("pagesize: " + pagesize);

        InitLevelList();

        titleText.text = title;

        //RefreshLevelList();
    }

    private void InitLevelList()
    {
        DesktopUtils.ShowLoadingIndicator();

        DestroyLevelButtons();
        levelInfoList.Clear();

        if (GameSparksManager.Instance.Available())
        {
            GameSparks.Api.Requests.LogEventRequest req = new GameSparks.Api.Requests.LogEventRequest();
            req.SetEventKey(request);
            req.SetEventAttribute("Skip", offset);
            req.SetEventAttribute("Limit", 100);
            req.SetEventAttribute("Count", 1);
            if (query != null)
                req.SetEventAttribute("Query", new GameSparks.Core.GSRequestData(query));
            req.Send((response) =>
            {
                // --- check for response received after screen was destroyed
                if (this == null)
                    return;

                DesktopUtils.HideLoadingIndicator();
                if (response.HasErrors)
                {
                    Debug.LogWarning("failed to retrieve user level count");
                }
                else
                {
                    Debug.Log("retrieved level count");
                    if (response.ScriptData.ContainsKey("count"))
                    {
                        numLevels = response.ScriptData.GetInt("count").Value;
                        CreateLevelButtons();
                        offset = 0;
                        RefreshLevelList();
                    }
                    else
                    {
                        Debug.Log("response contains no levels");
                    }
                }
            });
        }
    }

    private void RefreshLevelList()
    {
        //DesktopUtils.ShowLoadingIndicator();

        //DestroyLevelButtons();
        //levelInfoList.Clear();

        if (GameSparksManager.Instance.Available())
        {
            GameSparks.Api.Requests.LogEventRequest req = new GameSparks.Api.Requests.LogEventRequest();
            req.SetEventKey(request);
            req.SetEventAttribute("Skip", offset);
            req.SetEventAttribute("Limit", queryLimit);
            req.SetEventAttribute("Count", 0);
            if (query != null)
                req.SetEventAttribute("Query", new GameSparks.Core.GSRequestData(query));
            req.Send((response) =>
            {
                // --- check for response received after screen was destroyed
                if (this == null)
                    return;
                //DesktopUtils.HideLoadingIndicator();
                if (response.HasErrors)
                {
                    Debug.LogWarning("failed to retrieve user levels");
                }
                else
                {
                    Debug.Log("retrieved level data");
                    if (response.ScriptData.ContainsKey("levels"))
                    {
                        List<GameSparks.Core.GSData> data = response.ScriptData.GetGSDataList("levels");
                        foreach (GameSparks.Core.GSData item in data)
                        {
                            levelInfoList.Add(new LevelInfo(
                                item.GetString("id"),
                                item.GetString("playerName"),
                                item.GetString("levelData"),
                                item.GetInt("numRatings").Value,
                                item.GetFloat("avgRating").Value));
                        }
                        Debug.Log("processed " + data.Count + " levels");
                        UpdateLevelButtons(offset, data.Count);
                        offset += data.Count;
                        if (offset < numLevels)
                        {
                            RefreshLevelList();
                        }
                    }
                    else
                    {
                        Debug.Log("response contains no levels");
                    }
                }
            });
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    //void OnPrevButtonPressed(MonoBehaviour sender, Vector2 p)
    //{
    //    if (offset > 0)
    //    {
    //        offset -= pagesize;
    //        if (offset < 0) offset = 0;
    //        RefreshLevelList();
    //    }
    //}

    //void OnNextButtonPressed(MonoBehaviour sender, Vector2 p)
    //{
    //    if (levelInfoList.Count == pagesize)
    //    {
    //        offset += pagesize;
    //        RefreshLevelList();
    //    }
    //}

    void DestroyLevelButtons()
    {
        foreach (Transform t in levelsPanel.transform)
        {
            Destroy(t.gameObject);
        }
        levelButtons.Clear();
    }

    void CreateLevelButtons()
    {
        for (int i = 0; i < numLevels; i++)
        {
            GameObject obj = GameObject.Instantiate<GameObject>(levelButtonPrefab);
            obj.transform.SetParent(levelsPanel);
            obj.transform.localScale = Vector3.one;

            int level = i + 1;

            UserLevelButton button = obj.GetComponent<UserLevelButton>();

            button.secondaryPanel = ButtonDetailsEnabled;

            button.levelIndex = i;
            obj.name = "Level" + level.ToString();

            levelButtons.Add(button);
        }

        nothingToDisplay.gameObject.SetActive(numLevels == 0);
    }

    void UpdateLevelButtons(int skip, int count)
    {
        GameObject tempRoot = new GameObject();
        LevelRoot levelRoot = tempRoot.AddComponent<LevelRoot>();
        levelRoot.CreateStructure();

        for (int i = skip; i < skip + count; i++)
        {
            LevelInfo info = levelInfoList[i];

            UserLevelButton button = levelButtons[i];

            button.levelIndex = i;
            string byName = info.author;
            if (byName.Length > 16)
                byName = byName.Substring(0, 16);
            button.levelId = info.id;
            button.author = /*"BY " + */byName;
            button.numVotes = info.numRatings;
            button.rating = info.avgRating;

            TableDescription td = null;
            TableLoadSave.LoadFromMemory(System.Convert.FromBase64String(info.data), ref td, levelRoot);

            button.thumbnail.texture = TableLoadSave.MakeThumbnail(td, 90);

            button.OnUserLevelButtonEvent = OnLevelButtonAction;

            button.FinishLoading();
        }

        GameObject.Destroy(tempRoot);
        //StartCoroutine(PopulateButtonData());

        TableLoadSave.Cleanup();
    }

    //IEnumerator PopulateButtonData()
    //{
    //    GameObject tempRoot = new GameObject();
    //    LevelRoot levelRoot = tempRoot.AddComponent<LevelRoot>();
    //    levelRoot.CreateStructure();

    //    WaitForEndOfFrame wait = new WaitForEndOfFrame();

    //    for (int i = 0; i < levelInfoList.Count; i++)
    //    {
    //        yield return wait;
    //        LevelInfo info = levelInfoList[i];
    //        GameObject obj = levelsPanel.GetChild(i).gameObject;
    //        UserLevelButton button = obj.GetComponent<UserLevelButton>();
    //        TableDescription td = null;
    //        TableLoadSave.LoadFromMemory(System.Convert.FromBase64String(info.data), ref td, levelRoot);
    //        button.thumbnail.texture = TableLoadSave.MakeThumbnail(td, 90);
    //    }

    //    GameObject.Destroy(tempRoot);
    //}

    void OnLevelButtonAction(MonoBehaviour sender, UserLevelButton.ButtonEvent action)
    {
        UserLevelButton button = sender.GetComponent<UserLevelButton>();
        string levelTextData = levelInfoList[button.levelIndex].data;

        switch (action)
        {
            case UserLevelButton.ButtonEvent.Play:
                GameSession.customLevelBytes = System.Convert.FromBase64String(levelTextData);
                GameSession.customLevelId = levelInfoList[button.levelIndex].id;
                Close();
                WindowA.Create("UI/NormalPlayScreen").Show();
                break;
            case UserLevelButton.ButtonEvent.Share:
                //DesktopUtils.ShowNotAvailable();
                GameObject tempRoot = new GameObject();
                LevelRoot levelRoot = tempRoot.AddComponent<LevelRoot>();
                levelRoot.CreateStructure();

                TableDescription td = null;
                TableLoadSave.LoadFromMemory(System.Convert.FromBase64String(levelTextData), ref td, levelRoot);

                Texture2D tex = TableLoadSave.MakeThumbnail(td, 256, false);
                GameSparksManager.Instance.ShareUserLevel(GameSession.customLevelId, tex);

                Destroy(levelRoot);
                break;
            case UserLevelButton.ButtonEvent.Edit:
                Close();
                WindowA w = WindowA.Create("UI/WorkshopScreen", null);
                w.GetComponent<WorkshopScreen>().levelSourceBytes = System.Convert.FromBase64String(levelTextData);
                w.Show();
                break;
            case UserLevelButton.ButtonEvent.Delete:
                if (GameSparksManager.Instance.Available())
                {
                    DesktopUtils.ShowLoadingIndicator();
                    GameSparks.Api.Requests.LogEventRequest req = new GameSparks.Api.Requests.LogEventRequest();
                    req.SetEventKey("DeleteUserLevel");
                    req.SetEventAttribute("Level", button.levelId);
                    req.Send((response) =>
                    {
                        DesktopUtils.HideLoadingIndicator();
                        if (response.HasErrors)
                        {
                            Debug.LogWarning("failed to delete user level");
                        }
                        else
                        {
                            Debug.Log("successfully deleted");
                            InitLevelList();
                        }
                    });
                }
                break;
            case UserLevelButton.ButtonEvent.SlideIn:
                foreach (UserLevelButton b in levelsPanel.transform.GetComponentsInChildren<UserLevelButton>())
                {
                    if (b != button)
                    {
                        b.SlideOut();
                    }
                }
                break;
        }
    }
}
