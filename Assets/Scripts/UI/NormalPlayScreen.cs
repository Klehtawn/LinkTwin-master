using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NormalPlayScreen : PlayScreen
{
    public Widget pauseButton;
    public Button editButton;

    public Widget solutionsIndicator;

    public Widget exitReplayButton;

    public SolutionHint solutionHint;

    private ManagePowerups powerups;

    GameplaySteps gameplaySteps = null;
    public SliderBar gameplayStepsBar;

    public Widget topBar;

    private SliderBarIndicator gameplayStepsIndicator;

    public KnobControl knobControl;

    int startCalls = 0;

    protected override void Awake()
    {
        base.Awake();

        gameplaySteps = GetComponent<GameplaySteps>();
        if (gameplaySteps == null)
            gameplaySteps = gameObject.AddComponent<GameplaySteps>();
    }

    protected override void Start()
    {
        startCalls++;

        if (startCalls > 1) return;

        base.Start();

        GameEvents.ClearEvents();

        powerups = GetComponentInChildren<ManagePowerups>();
        if(powerups != null)
            powerups.OnPowerupSelected += OnPowerupSelected;

        pauseButton.OnClick += OnPauseButtonPressed;
        editButton.OnClick += OnEditButtonPressed;
#if !UNITY_EDITOR
        editButton.gameObject.SetActive(false);
#endif

        solutionsIndicator.OnClick += OnSolutionsPressed;

        exitReplayButton.active = false;
        //exitReplayButton.OnClick += OnExitReplayButtonPressed;
        //exitReplayButton.gameObject.SetActive(false);

        gameplayStepsBar.value = 0.0f;
        gameplayStepsBar.SetFillValueDirect(0.0f);
        gameplayStepsBar.OnValueChanged += OnGameplayStepsBarValueChanged;
        gameplayStepsBar.OnStartDragging += OnGameplayStepsBarStartDragging;
        gameplayStepsBar.OnEndDragging += OnGameplayStepsBarEndDragging;
        //gameplayStepsIndicator = gameplayStepsBar.AddIndicator(0.5f, 10.0f, false);

        gameplayStepsBar.gameObject.SetActive(false);

        RegisterGameEvents();

        solutionHint.visible = false;

        knobControl.OnRoundedValueChanged += OnKnobControlValueChanged;
        knobControl.OnBeginDragging += OnKnobControlBeginDragging;
        knobControl.OnEndDragging += OnKnobControlEndDragging;
        knobControl.minValue = 0.0f;
    }

    void RegisterGameEvents()
    {
        TheGame.Instance.OnPlayersStartToMove += OnPlayersStartToMove;
        TheGame.Instance.OnGameRestarted += OnGameRestarted;
        TheGame.Instance.OnEverybodyFinishedMoving += OnEverybodyFinishedMoving;
    }

    void OnPauseButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        ShowPauseMenu();
    }

    private void ShowPauseMenu()
    {
        WindowA w = WindowA.Create("UI/GamePausedBox");
        w.OnWindowClosed += OnGamePausedBoxStartClosing;
        w.ShowModal();
        game.paused = true;
        Desktop.main.sounds.SetMusicVolumeLow();
    }

    void OnEditButtonPressed(MonoBehaviour sender, Vector2 p)
    {
#if UNITY_EDITOR
        Close();
        string path = TableLoadSave.LoadSaveFolder() + game.levelPath;
        WindowA w = WindowA.Create("UI/WorkshopScreen", "editplayed " + path);
        w.Show();
#endif
    }

    void OnGamePausedBoxStartClosing(WindowA sender, int retValue)
    {
        if (retValue == (int)GamePausedBox.ReturnValues.PressedExit)
        {
            Desktop.main.windowsFlow.Backward();
        }
        else
        {
            game.paused = false;
            Desktop.main.sounds.SetMusicDefaultVolume();
        }
    }

    void OnSettingsBoxClosed(WindowA sender, int retValue)
    {
        //OnPauseButtonPressed(null, Vector2.zero);
    }

    protected override void OnGameEnded(bool hasWon)
    {
        solutionHint.visible = false;
        TheGame.Instance.solutionDisplayActive = false;
        solutionsIndicator.active = true;
        gameplayStepsBar.active = true;
        topBar.active = true;

        if (TheGame.Instance.isReplaying)
        {
            DesktopUtils.ShowMessageBox("Replay complete.", () =>
                {
                    //solutionsIndicator.active = true;
                    //exitReplayButton.active = false;
                    RetryLevel();
                });
            replayMessage.Close();
        }
        else
        {
            StartCoroutine(_OnGameEnded(hasWon, 0.5f));
        }
    }

    IEnumerator _OnGameEnded(bool hasWon, float delay)
    {
        yield return new WaitForSeconds(delay);

        WindowA w = null;

        if (hasWon)
        {
            Desktop.main.sounds.SetMusicVolumeLow();

            GameEvents.Send(GameEvents.EventType.LevelCompleted);
            GameSession.SendLevelEvent("Complete");
            GameSession.MarkCurrentLevelAsFinished();

            if(GameSession.HasFinishedChapter() == false)
            {
                GameSession.UnlockNextLevel();
            }

#if PLAYTEST
            w = WindowA.Create("UI/GameWonBoxDiffRating");
            w.OnWindowStartClosing += OnGameWonDiffRatingClosed;
#else
            w = WindowA.Create("UI/GameWonBoxEx");
            GameWonBoxEx gwb = w.GetComponent<GameWonBoxEx>();
            //gwb.showReward = GameSession.IsUserLevel() == false;

            if (game.solution != null)
            {
                int targetmoves = game.solution.Length;

                gwb.movesPerStar = new int[3];
                gwb.movesPerStar[0] = (int)(targetmoves * 2.0f);
                gwb.movesPerStar[1] = (int)(targetmoves * 1.15f);
                gwb.movesPerStar[2] = (int)(targetmoves * 1.05f);

                //Debug.Log("finished in " + game.moves + " moves out of " + game.solution.Length);
                if (game.moves <= (int)(targetmoves * 1.05f))
                    gwb.rating = 3.0f;
                else if (game.moves <= (int)(targetmoves * 1.25f))
                    gwb.rating = 2.0f;
                else
                    gwb.rating = 1.0f;
            }
            else
            {
                gwb.rating = 3.0f;
            }

            float prevStars = GameSession.GetStarsForCurrentLevel();
            float diffStars = Mathf.Max(0.0f, gwb.rating - prevStars);

            gwb.reward = (int)(diffStars * GameSession.GetRewardPerStartForCurrentLevel());

            if(gwb.reward > prevStars)
                GameSession.SetStarsForCurrentLevel((int)gwb.rating);

            w.OnWindowStartClosing += OnGameWonClosed;
#endif
        }
        else
        {
            GameSession.SendLevelEvent("Failed");
            //w = WindowA.Create("UI/GameLostBox");
            //w.OnWindowStartClosing += OnGameLostClosed;
            RetryLevel(0.35f);
        }

        if(w != null)
            w.ShowModal(true);
    }

    public void NextLevel()
    {
        if (GameSession.IsUserLevel())
        {
            GameSession.LoadRandomLevel(() =>
                {
                    LoadGame();
                });
        }
        else
        {
            GameSession.IncrementLevel();

            LoadGame();
            //int p = GameSession.currentPlaying + 1;
            //ShowTopMessage(p.ToString(), 0.1f);

            tableMask.gameObject.SetActive(true);
        }

        RegisterGameEvents();
        myTween.Instance.FadeSaturation(gameAreaProjection.gameObject, 1.0f, 0.4f);
    }

    void UnlockNextChapter()
    {
        Close(0);
        WindowA.Create("UI/ChapterSelectScreen", "bonuslevels").Show();
    }

    void OnGameWonClosed(WindowA sender, int retValue)
    {
        if (retValue == (int)GameWonBoxEx.ReturnValues.PressedNext)
        {
            if (GameSession.HasFinishedChapter())
            {
                UnlockNextChapter();
            }
            else
            {
                NextLevel();
            }
        }

        if (retValue == (int)GameWonBoxEx.ReturnValues.PressedExit)
        {
            Desktop.main.windowsFlow.Backward();
        }

        if (retValue == (int)GameWonBoxEx.ReturnValues.PressedRetry)
        {
            RetryLevel();
        }        
    }

    void OnGameWonDiffRatingClosed(WindowA sender, int retValue)
    {
        GameSession.RateCurrentLevel(retValue);
        NextLevel();
    }

    void OnGameLostClosed(WindowA sender, int retValue)
    {
        if (retValue == (int)GameLostBox.ReturnValues.PressedRetry)
        {
            RetryLevel();
        }
    }

    void OnSolutionsPressed(MonoBehaviour sender, Vector2 pos)
    {
        int numSolutions = GameEconomy.boughtAmmount(GameEconomy.EconomyItemType.Solutions);

        if(numSolutions == 0)
        {
            Minimize();
            WindowA.Create("UI/ShopScreen", "overlay,solutions").Show();
            return;
        }

        ConfirmationBox.Show(Locale.GetString("USE_SOLUTION_CONFIRM"), UseSolution);
    }

    WindowA replayMessage = null;

    void UseSolution()
    {
        GameEconomy.EconomyItem solutions = GameEconomy.bought(GameEconomy.EconomyItemType.Solutions);

        if(TheGame.Instance.isHintAvailable)
        {
            solutions.ammount--;
            //TheGame.Instance.hintPressed = true;
            TheGame.Instance.Restart();
            TheGame.Instance.solutionDisplayActive = true;
            TheGame.Instance.currentSolutionStep = 0;
            solutionsIndicator.active = false;
            //exitReplayButton.gameObject.SetActive(true);
            //exitReplayButton.active = true;
            UpdateHintDirection();
            solutionHint.visible = true;

            GameEconomy.SaveLocal();

            //replayMessage = ShowTopMessage(Locale.GetString("SOLUTION"), 0.0f, false);

            gameplayStepsBar.active = false;
            topBar.active = false;
        }
        else
        {
            DesktopUtils.ShowLocalizedMessageBox("NO_SOLUTION");
        }
    }

    void OnExitReplayButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        if (TheGame.Instance.isReplaying)
        {
            solutionsIndicator.active = true;
            //exitReplayButton.active = false;
            RetryLevel();
            replayMessage.Close();
        }
    }

    void OnPowerupSelected(GameEconomy.EconomyItemType powerup, bool selected)
    {
        tableMask.ClearHighlights();

        if(powerup == GameEconomy.EconomyItemType.Invalid)
        {
            // not enough powerups

            Minimize();
            WindowA.Create("UI/ShopScreen", "overlay,powerups").Show();

            return;
        }

        if (selected)
        {
            TheGame.Instance.powerupActive = powerup;

            List<Vector3> hl = new List<Vector3>();

            if(powerup == GameEconomy.EconomyItemType.PowerupFreezeCharacter)
            {
                foreach(Player p in TheGame.Instance.players)
                {
                    hl.Add(p.position);
                }
            }
            else if (powerup == GameEconomy.EconomyItemType.PowerupPlaceATile)
            {
                hl = TheGame.Instance.blocks.GetEmptyNeighbours();
            }

            tableMask.HighlightBlocks(ref hl);
        }
        else
        {
            TheGame.Instance.powerupActive = GameEconomy.EconomyItemType.Invalid;
        }
    }

    bool clicked = false;
    bool hasTouch = false;
    Vector2 clickPosition = Vector2.zero;

    bool touchDown()
    {
        return Input.touchCount > 0 || Input.GetMouseButton(0);
    }

    Vector2 touchPos()
    {
        if (Input.touchCount > 0)
        {
            return Input.touches[0].position;
        }

        if (Input.GetMouseButton(0))
        {
            return Input.mousePosition;
        }

        return Vector2.zero;
    }

    bool UpdatePowerups()
    {
        GameEconomy.EconomyItemType pu = TheGame.Instance.powerupActive;
        if (pu == GameEconomy.EconomyItemType.Invalid)
            return false;

        if (touchDown())
        {
            hasTouch = true;
            clickPosition = touchPos();
        }
        else
        {
            if (hasTouch)
                clicked = true;
            else
                clicked = false;
            hasTouch = false;
        }

        if (clicked == false) return false;

        if (clicked)
        {
            Camera gameCam = GameObject.FindObjectOfType<GameCamera>().getCamera();
            Vector3 worldPos = gameCam.ScreenToWorldPoint(clickPosition);

            if (tableMask.GetHighlightAtPos(worldPos, ref worldPos) == false)
                return false;

            if (pu == GameEconomy.EconomyItemType.PowerupFreezeCharacter)
            {
                Player p = TheGame.Instance.PlayerAt(worldPos);
                if (p != null)
                {
                    Debug.Log("Player clicked: " + p.gameObject.name);
                    p.ApplyPowerup(pu);
                    TheGame.Instance.powerupActive = GameEconomy.EconomyItemType.Invalid;

                    GameEvents.Send(GameEvents.Sender.System, GameEvents.EventType.PowerupUsed, "freeze_character");
                    GameEconomy.Consume(GameEconomy.EconomyItemType.PowerupFreezeCharacter, 1);

                    tableMask.ClearHighlights();
                }
            }

            if (pu == GameEconomy.EconomyItemType.PowerupPlaceATile)
            {
                worldPos = SnapToGrid.Snap(worldPos);
                if(tableMask.Contains(worldPos))
                {
                    GameEvents.Send(GameEvents.Sender.System, GameEvents.EventType.PowerupUsed, "place_a_tile");

                    GameObject g = TheGame.Instance.blocks.ground[0].gameObject;
                    worldPos.y = g.transform.position.y;

                    GameObject obj = GameObject.Instantiate<GameObject>(g);

                    obj.transform.SetParent(g.transform.parent);
                    obj.transform.position = worldPos;
                    obj.name = "TerraNova";

                    TheGame.Instance.powerupActive = GameEconomy.EconomyItemType.Invalid;

                    TheGame.Instance.blocks.Fill(gameArea.gameObject);
                    levelRoot.RefreshStructure();

                    TheGame.Instance.CenterCamera();

                    GameEconomy.Consume(GameEconomy.EconomyItemType.PowerupPlaceATile, 1);

                    tableMask.ClearHighlights();
                }
            }
        }

        return true;
    }

    protected override void Update()
    {
        base.Update();
        UpdatePowerups();
        if (!game.paused && GameSession.BackKeyPressed())
            ShowPauseMenu();
    }

    int refMoves = 11;
    float maxGameplayStepsValue = 0.0f;

    void OnPlayersStartToMove()
    {
    }

    void OnGameplayStepsBarValueChanged(float currentValue, float previousValue)
    {
        gameplayStepsBar.value = Mathf.Clamp(gameplayStepsBar.value, 0.0f, maxGameplayStepsValue);
        if (gameplayStepsBar.valueChangedByInput == false) return;

        int step = Mathf.RoundToInt(gameplayStepsBar.value * (float)refMoves);
        gameplaySteps.Cursor = step;

        TheGame.Instance.moves = gameplaySteps.Cursor;
    }

    void OnGameRestarted()
    {
        gameplaySteps.Clear();
        gameplayStepsBar.value = 0.0f;
        refMoves = 11;
        gameplayStepsBar.SetFillValueDirect(0.0f);

        RepositionGameplayStepsIndicator();

        knobControl.value = 0.0f;

        myTween.Instance.FadeSaturation(gameAreaProjection.gameObject, 1.0f, 0.4f);
    }

    void OnEverybodyFinishedMoving()
    {
        int m = TheGame.Instance.moves;

        float f = (float)m / (float)refMoves;
        if (f > 0.7f)
        {
            refMoves = (refMoves * 17) / 10;
        }

        RepositionGameplayStepsIndicator();

        gameplayStepsBar.value = (float)m / (float)refMoves;
        gameplayStepsBar.fillValue = (float)m / (float)refMoves;
        maxGameplayStepsValue = gameplayStepsBar.value;

        gameplaySteps.RecordStep(TheGame.Instance);

        UpdateHintDirection();

        knobControl.value = m;
        knobControl.maxValue = m;
    }

    void UpdateHintDirection()
    {
        if (TheGame.Instance.solutionDisplayActive)
        {
            if (TheGame.Instance.currentSolutionStep < TheGame.Instance.solution.Length)
                solutionHint.SetDirection(TheInput.indexedDirections[TheGame.Instance.solution[TheGame.Instance.currentSolutionStep]]);
            else
                solutionHint.visible = false;
        }
    }

    void RepositionGameplayStepsIndicator()
    {
        if(gameplayStepsIndicator != null)
        {
            gameplayStepsIndicator.valueScaling = (float)refMoves;
            gameplayStepsIndicator.value = 10.0f / (float)refMoves;
        }
    }

    void OnGameplayStepsBarStartDragging()
    {
        TheGame.Instance.blocks.EnableBehaviour(false);
    }

    void OnGameplayStepsBarEndDragging()
    {
        TheGame.Instance.blocks.EnableBehaviour(true);
    }

    void OnKnobControlValueChanged(float v)
    {
        gameplaySteps.Cursor = (int)v;
        TheGame.Instance.moves = gameplaySteps.Cursor;
    }

    void OnKnobControlBeginDragging()
    {
        TheGame.Instance.blocks.EnableBehaviour(false);
        myTween.Instance.FadeSaturation(gameAreaProjection.gameObject, 0.15f, 0.4f);
    }

    void OnKnobControlEndDragging()
    {
        TheGame.Instance.blocks.EnableBehaviour(true);
        myTween.Instance.FadeSaturation(gameAreaProjection.gameObject, 1.0f, 0.4f);
    }
}
