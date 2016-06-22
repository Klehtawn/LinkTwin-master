using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlayScreen : GameScreen
{
    public RawImage tableRenderMask = null;

    public Widget retryButton;
    public Button undoButton;

    protected Transform gameArea;
    protected Transform gameAreaProjection;
    private Camera gameCamera;

    protected LevelRoot levelRoot;

    private RenderTexture gameTexture;

    protected TheGame game;
    int mustActivateProjection = -1;

    protected TableMask tableMask = null;

    protected override void Awake()
    {
        base.Awake();

        if (GameSession.currentPlaying == -1)
        {
            GameSession.currentPlaying = 1;
            PlayerPrefs.SetInt("CurrentLevel", 1);
        }

        gameArea = transform.Find("GameArea");
        gameAreaProjection = transform.Find("GameAreaProjection");
        gameAreaProjection.transform.SetAsFirstSibling();
        gameAreaProjection.gameObject.SetActive(false);

        OnWidgetShown += OnWindowShown;

        GameSession.gameState = GameSession.GameState.Playing;
    }

    bool firstGame = true;
	protected override void Start ()
    {
        base.Start();   

        retryButton.OnTouchUp += OnRetryButton;
        undoButton.OnTouchUp += OnUndoButton;

        if (Application.isPlaying)
        {
            if (firstGame)
            {
                LoadGame();
                firstGame = false;
            }
            LoadGame();
            TheGame.Instance.introFinished = false;
        }

        OnWindowFinishedShowing += _OnWindowFinishedShowing;

        TheGame.Instance.input.parentWidget = gameAreaProjection.GetComponent<Widget>();

        GameEvents.Send(GameEvents.EventType.GameplayEntered);
	}

    void _OnWindowFinishedShowing(WindowA sender)
    {
        game.introFinished = true;
        if(tableMask != null)
            tableMask.gameObject.SetActive(true);
    }

    void OnWindowShown(Widget sender)
    {
        if (visible == false) return;

        //float delay = 0.0f;
        //if (GetComponent<ShowEffect>() != null)
        //    delay = GetComponent<ShowEffect>().duration;

        //if (GameSession.IsUserLevel())
        //{
        //    //ShowTopMessage("TEST", delay + 0.1f);
        //    //ShowTopMessage(GameSession.customLevelId, delay + 0.1f);
        //}
        //else
        //{
        //    //int p = GameSession.currentPlaying + 1;
        //    //ShowTopMessage(p.ToString(), delay + 0.1f);
        //}
    }
	
	protected override void Update ()
    {
        base.Update();

        if(mustActivateProjection >= 0)
        {
            if (mustActivateProjection < 3)
                mustActivateProjection++;
            else
            {
                gameAreaProjection.gameObject.SetActive(true);
                mustActivateProjection = -1;
            }
        }

        if (tableRenderMask != null)
        {
            tableRenderMask.gameObject.SetActive(tableMask.GetMaskAlpha() > 0.05f);
            tableRenderMask.color = new Color(1.0f, 1.0f, 1.0f, tableMask.GetMaskAlpha());
        }
        else
        {
            if(tableMask != null)
                tableMask.gameObject.SetActive(false);
        }
	}

    void OnRetryButton(MonoBehaviour sender, Vector2 p)
    {
        GameSession.SendLevelEvent("Restart");
        RetryLevel();
    }

    void OnUndoButton(MonoBehaviour sender, Vector2 p)
    {
        DesktopUtils.ShowNotAvailable();
    }

    protected void ReloadTable()
    {
        /*float dur = 0.35f;
        LeanTween.scaleY(gameAreaProjection.gameObject, -1.0f, dur).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
        {
            LeanTween.scaleY(gameAreaProjection.gameObject, 1.0f, dur).setEase(LeanTweenType.easeOutBack);
        });*/

        levelRoot.CreateStructure();

        TableDescription td = null;

        if (GameSession.IsUserLevel())
        {
            TableLoadSave.LoadFromMemory(GameSession.customLevelBytes, ref td, levelRoot);
        }
        else
        {
            string levelName = string.Format("c{0:D2}l{1:D2}", GameSession.currentChapter + 1, GameSession.currentPlaying + 1);
            string path = "Levels/" + levelName;
            GameSession.lastPlayedLevel = levelName;

            game.levelPath = path;
            TableLoadSave.LoadAbsolutePath(path, ref td, levelRoot);
        }

        Transform table = TableLoadSave.ConvertMapDescriptionToScene(td, levelRoot);
        table.SetParent(gameArea);
        Widget.SetLayer(gameArea.gameObject, LayerMask.NameToLayer("Game"));

        levelRoot.RefreshStructure();
    }

    protected void LoadGame()
    {
        DeleteGame();

        PreLoadDefaults();

        if (GameSession.IsUserLevel())
        {
            game.levelPath = null;

            TableDescription td = null;
            TableLoadSave.LoadFromMemory(GameSession.customLevelBytes, ref td, levelRoot);
            Transform table = TableLoadSave.ConvertMapDescriptionToScene(td, levelRoot);
            game.solution = td.solutionSteps;
            table.SetParent(gameArea);
        }
        else
        {
            string levelName = string.Format("c{0:D2}l{1:D2}", GameSession.currentChapter + 1, GameSession.currentPlaying + 1);
            string path = "Levels/" + levelName;
            GameSession.lastPlayedLevel = levelName;

            game.levelPath = path;

            TableDescription td = null;
            TableLoadSave.LoadAbsolutePath(path, ref td, levelRoot);
            Transform table = TableLoadSave.ConvertMapDescriptionToScene(td, levelRoot);
            game.solution = td.solutionSteps;
            table.SetParent(gameArea);
        }

        PostLoadDefaults();

        if (Application.isPlaying)
        {
            gameArea.transform.SetParent(Desktop.main.transform);
            gameArea.transform.localScale = Vector3.one;
        }

        game.OnGameEnded += OnGameEnded;

        game.moves = 0;
        game.startTime = Time.realtimeSinceStartup;

        Desktop.main.sounds.ResumeMusic();
        Desktop.main.sounds.SetMusicDefaultVolume();
    }

    void DeleteGame()
    {
        Widget.DeleteAllChildren(gameArea);
        gameAreaProjection.gameObject.SetActive(false);
    }

    void PreLoadDefaults()
    {
        GameObject obj = new GameObject();
        obj.name = "TheGame";
        game = obj.AddComponent<TheGame>();
        obj.transform.SetParent(gameArea);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        levelRoot = gameArea.gameObject.GetComponent<LevelRoot>();
        if (levelRoot == null)
            levelRoot = gameArea.gameObject.AddComponent<LevelRoot>();
        levelRoot.CreateStructure();

        obj = new GameObject();
        obj.name = "GameCamera";
        obj.transform.SetParent(gameArea);

		obj.AddComponent<GameCamera>();

        gameCamera = obj.AddComponent<Camera>();
        gameCamera.transparencySortMode = TransparencySortMode.Orthographic;
        gameCamera.transform.position = Vector3.up * 236.0f;
        gameCamera.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        gameCamera.orthographic = true;
        gameCamera.clearFlags = CameraClearFlags.SolidColor;

        Color c = Desktop.main.theme.generalAppearance.desktop;
        c.a = 0.0f;
        gameCamera.backgroundColor = c;
        
        int layer = LayerMask.NameToLayer("Game");
        gameCamera.cullingMask = 1 << layer;
        gameCamera.transform.SetAsFirstSibling();

        Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Game"));

        gameTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);

        gameTexture.filterMode = FilterMode.Bilinear;
        gameTexture.generateMips = false;
        gameTexture.Create();

        gameCamera.targetTexture = gameTexture;

        gameAreaProjection.GetComponent<RawImage>().texture = gameTexture;
        gameAreaProjection.gameObject.SetActive(false);
        mustActivateProjection = 0;

        GameObject bkgPrefab = GameSession.GetPreferredBackgroundForChapter();
        if (bkgPrefab != null)
        {
            GameObject bkg = GameObject.Instantiate<GameObject>(bkgPrefab);
            bkg.transform.SetParent(gameArea);
            if (bkg.GetComponentInChildren<FitToScreen>() != null)
                bkg.GetComponentInChildren<FitToScreen>().renderCamera = gameCamera;
            else
                if (bkg.GetComponentInChildren<GameplayBackground>() != null)
                {
                    bkg.GetComponentInChildren<GameplayBackground>().renderCamera = gameCamera;
                }
            bkg.transform.SetAsFirstSibling();
        }

        CreateTableMask();
    }

    void PostLoadDefaults()
    {
        TheGame.Instance.blocks.Fill(gameArea.gameObject);

        RenderSettings.ambientLight = Color.white;
        RenderSettings.ambientIntensity = 1.0f;

        Widget.SetLayer(gameArea.gameObject, LayerMask.NameToLayer("Game"));

        levelRoot.RefreshStructure();

        TheGame.Instance.input.parentWidget = gameAreaProjection.GetComponent<Widget>();
    }

    void OnDestroy()
    {
        if(Application.isPlaying)
            GameObject.Destroy(gameArea.gameObject);
        else
            GameObject.DestroyImmediate(gameArea.gameObject);
    }

    public void RetryLevel(float delay = 0.0f)
    {
        if (delay == 0.0f)
            _RetryLevel();
        else
            StartCoroutine(_RetryLevel(delay));
    }

    IEnumerator _RetryLevel(float delay)
    {
        yield return new WaitForSeconds(delay);

        _RetryLevel();
    }

    void _RetryLevel()
    {
        //ShowTopMessage("R", 0.1f);

        ReloadTable();
        TheGame.Instance.Restart();
        //LoadGame();

        mustActivateProjection = 4;

        if (tableMask != null)
            tableMask.gameObject.SetActive(true);

        
    }

    protected WindowA ShowTopMessage(string msg, float delay, bool autoClose = true)
    {
        WindowA w = autoClose ? WindowA.Create("UI/LevelStarted") : WindowA.Create("UI/LevelTopMessage");
        w.GetComponentInChildren<Text>().text = msg;
        Desktop.main.ShowWindowDelayed(w, delay);
        return w;
    }

    protected virtual void OnGameEnded(bool hasWon)
    {

    }

    void CreateTableMask()
    {
        if (tableRenderMask == null) return;

        GameObject obj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("UI/TableMask"));
        obj.transform.SetParent(gameArea);
        obj.name = "Table Highlights";

        tableMask = obj.GetComponent<TableMask>();
        tableMask.gameCamera = gameCamera;

        tableRenderMask.texture = tableMask.GetMaskTexture();

        game.highlightMask = tableMask;

        tableMask.gameObject.SetActive(false);
    }
}
