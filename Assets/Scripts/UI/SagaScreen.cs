using UnityEngine;
using UnityEngine.UI;

public class SagaScreen : GameScreen
{
//    public Button settingsButton;

//    public GameObject levelButtonPrefab;

//    public RectTransform levelsPanel;
//    //public LevelList levelList;
//    public ScrollRect scrollRect;

//	protected override void Start()
//    {
//        base.Start();

//        settingsButton.OnTouchUp += OnSettingsButtonPressed;

//        OnWindowClosed += _OnWindowClosed;

//        InitLevelButtons();
//        scrollRect.verticalNormalizedPosition = 0;
//    }
	
//    protected override void Update()
//    {
//        base.Update();
//	}

//    void LateUpdate()
//    {
//        Vector3 p = Camera.main.ScreenToWorldPoint(firstButton.screenCenter);
//        p -= lineInitialPos;
//        p.z = 5.0f;
//        lineRenderer.transform.position = p;
//    }

//    void OnSettingsButtonPressed(MonoBehaviour sender, Vector2 p)
//    {
//        Close();
//        WindowA.Create("UI/SettingsScreen").Show();
//    }

//    ObjectPoolContext poolContext = new ObjectPoolContext();

//    Vector3 lineInitialPos = Vector3.zero;
//    LineRenderer2 lineRenderer = null;
//    SagaButton2 firstButton = null;

//    void InitLevelButtons()
//    {
//        int numLevels = GameSession.GetNumLevels();
//        int maxUnlockedLevel = GameSession.GetMaxUnlockedLevel();
//        SagaButtonPosition[] buttons = levelsPanel.GetComponentsInChildren<SagaButtonPosition>();

//        Vector3[] points = new Vector3[Mathf.Min(numLevels, buttons.Length)];

//        for (int i = 0; i < buttons.Length; i++)
//        {
//            SagaButtonPosition buttonPos = buttons[i];

//            if (i < numLevels)
//            {
//                GameObject obj = ObjectPool.Spawn(levelButtonPrefab);
//                obj.transform.SetParent(levelsPanel);
//                obj.transform.localScale = Vector3.one;
//                obj.transform.position = buttonPos.transform.position;

//                SagaButton2 button = obj.GetComponent<SagaButton2>();

//                if (firstButton == null)
//                    firstButton = button;

//                button.levelIndex = i;
//                button.Refresh();
//                button.gameObject.name = "Level" + i.ToString();
//                button.active = true;// i <= maxUnlockedLevel;
//                button.highlight = i == maxUnlockedLevel;
//                button.OnClick = OnLevelButtonPressed;

//                if(i == maxUnlockedLevel)
//                    obj.transform.localScale = Vector3.one * 1.2f;
//                else
//                    obj.transform.localScale = Vector3.one * 0.9f;

//                points[i] = Camera.main.ScreenToWorldPoint(button.screenCenter);
//            }
            
//            Destroy(buttonPos.gameObject);
//        }


//        lineRenderer = GetComponentInChildren<LineRenderer2>();
//        lineRenderer.SetPositions(points, 0.1f);
//        lineRenderer.SetWidth(0.02f);
//        lineRenderer.transform.SetParent(null);
//        lineRenderer.transform.position = Vector3.zero;
//        lineRenderer.transform.localScale = Vector3.one;
//        lineRenderer.GetComponent<Renderer>().material.SetColor("_Color", Desktop.main.theme.generalAppearance.background);

//        lineInitialPos = points[0];
//    }

//    void OnDestroy()
//    {
//        if(lineRenderer != null)
//            GameObject.Destroy(lineRenderer.gameObject);
//    }

//    void OnLevelButtonPressed(MonoBehaviour sender, Vector2 p)
//    {
//        GameSession.currentPlaying = sender.GetComponent<SagaButton2>().levelIndex;
//        GameSession.customLevelBytes = null;
//        Close();
//        WindowA.Create("UI/NormalPlayScreen").Show();
//        if (GameSparksManager.Instance.Available())
//        {
//            GameSparksManager.Instance.RecordAnalytics("EVT_PLAY_LEVEL", "level", "" + GameSession.currentPlaying);
////            GameSparks.Api.Requests.LogEventRequest req = new GameSparks.Api.Requests.LogEventRequest();
//        }
//    }

//    void _OnWindowClosed(WindowA sender, int retValue)
//    {
//        poolContext.Cleanup();
//    }
}
