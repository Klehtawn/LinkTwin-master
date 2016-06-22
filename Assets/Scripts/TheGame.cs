using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class TheGame : MonoBehaviour
{
    public float unitSize = 1.0f;

    private static TheGame _instance = null;

    public List<Player> players;

    private bool _paused = false;
    public bool paused
    {
        get
        {
            return _paused;
        }
        set
        {
            input.Clear();
            _paused = value;
            if (_paused)
            {
                pauseTime = Time.realtimeSinceStartup;
                ghostsSimulation.PauseSimulation();
            }
            else
            {
                startTime += Time.realtimeSinceStartup - pauseTime;
                ghostsSimulation.ResumeSimulation();
            }
        }
    }

    private FinishingPoint[] finishingPoints;

    bool gameEnded = false;
    //public bool hintPressed = false;

    //int levelIndex;
    public string levelPath;

    public TheInput input;

    public int moves;
    public float startTime;
    public float pauseTime;

    [NonSerialized]
    public bool isReplaying = false;
    [NonSerialized]
    public byte[] solution;
    [NonSerialized]
    public bool solutionDisplayActive = false;
    [NonSerialized]
    public int currentSolutionStep;

    float timer;

    InputRecorder inputRecorder;

    [HideInInspector]
    [NonSerialized]

    public Blocks blocks = new Blocks();

    [HideInInspector]
    [NonSerialized]
    public TableMask highlightMask;

    [HideInInspector]
    [NonSerialized]
    public bool introFinished = true;

    [HideInInspector]
    [NonSerialized]
    public bool mustRefreshShadows = false;

    private bool everybodyFinisheMovingState = false;

    GhostsSimulation ghostsSimulation = null;

    [NonSerialized]
    public bool mustPlayGhostSimulation = false;

    void Awake()
    {
        _instance = this;
        RegisterInput();
    }

    // Use this for initialization
    void Start()
    {
        gameEnded = false;

        //string levelName = SceneManager.GetActiveScene().name;
        //if (levelName.Length >= 6)
        //{
        //    levelName = levelName.Remove(0, 5);
        //    //if (int.TryParse(levelName, out levelIndex) == false)
        //    //    levelIndex = 0;
        //}
        //levelIndex++;

        //string levelStart = "LEVEL " + levelIndex.ToString();// + " STARTED";
        //string levelStart = levelIndex.ToString();// + " STARTED";

        //GameSession.currentPlaying = levelIndex;
        //string levelStart = GameSession.currentPlaying.ToString();

        //Blocks.ScaleToZero();
        //Blocks.ScaleToOne();
        if (Desktop.main == null)
            blocks.Fill(transform.parent);
		
        inputRecorder = new InputRecorder();

        timer = 0;

        if (OnGameRestarted != null)
            OnGameRestarted();

        ghostsSimulation = GhostsSimulation.Create(transform);
    }

    void OnDestroy()
    {
        //_instance = null;
    }

    void RegisterInput()
    {
        input = GetComponent<TheInput>();
        if (input == null)
        {
            input = gameObject.AddComponent<TheInput>();
        }
    }

//    float timeOfStart = -1.0f;
    void StartTimer()
    {
//        timeOfStart = Time.time;
    }

    // Update is called once per frame

    public Action OnGameRestarted;

    public void Restart()
    {
        paused = false;
        gameEnded = false;

        moves = 0;
        startTime = Time.realtimeSinceStartup;

        foreach (Player p in players)
            Destroy(p.gameObject);

        players.Clear();

        StartTimer();

        foreach (Block b in blocks.items)
        {
            b.playersOnSite.Clear();
        }

        foreach (Block b in blocks.ground)
        {
            b.playersOnSite.Clear();
        }

        calledGameStarted = false;

        everybodyFinisheMovingState = false;

        if (OnGameRestarted != null)
            OnGameRestarted();
    }

    bool calledGameStarted = false;

    public Action OnEverybodyFinishedMoving;

    void Update()
    {
        if (paused) return;

        if(calledGameStarted == false)
        {
            blocks.Fill(transform.parent);

            FillPlayers();
            FillFinishingPoints();

            foreach (Block b in blocks.ground)
                b.OnGamePreStart();

            foreach (Block b in blocks.items)
                b.OnGamePreStart();

            foreach (Block b in blocks.ground)
                b.OnGameStarted();

            foreach (Block b in blocks.items)
                b.OnGameStarted();

            foreach (Block b in blocks.ground)
                b.OnGamePostStart();

            foreach (Block b in blocks.items)
                b.OnGamePostStart();

            foreach(Player p in players)
                GameEvents.Send(p.kind, GameEvents.EventType.Spawned);

            calledGameStarted = true;

            UnityEngine.Random.seed = (int)Time.realtimeSinceStartup * 123;

            mustRefreshShadows = false;

            //ShowCreateTableEffect();
        }

        if (mustPlayGhostSimulation && ghostsSimulation.IsPlaying == false)
        {
            ghostsSimulation.PlaySimulation();
            mustPlayGhostSimulation = false;
        }

        ConvertToNewVersion();

        timer += Time.deltaTime;

        CheckPlayersOverlapping();

        if (isReplaying)
        {
            if (inputRecorder.replayQueue.Count > 0)
            {
                InputEvent ev = inputRecorder.replayQueue.Peek();
                if (ev.time <= timer && EverybodyFinishedMoving())
                {
                    MovePlayers(inputRecorder.replayQueue.Dequeue().data);
                }
            }
        }
        else
        {
            bool finishedMoving = EverybodyFinishedMoving();
            if(finishedMoving != everybodyFinisheMovingState)
            {
                if(finishedMoving)
                {
                    if (OnEverybodyFinishedMoving != null)
                        OnEverybodyFinishedMoving();
                }
                everybodyFinisheMovingState = finishedMoving;
            }
            if (finishedMoving)
            {
                TheInput.InputData data = input.Pop();

                if (data != null)
                {
                    bool isMoveAllowed = true;
                    if (solutionDisplayActive)
                    {
                        isMoveAllowed = (data.data == TheInput.indexedDirections[solution[currentSolutionStep]]);
                        Debug.Log("current allowed dir is " + TheInput.indexedDirections[solution[currentSolutionStep]] + ", user went " + data.data + " allowed: " + isMoveAllowed);
                    }
                    if (isMoveAllowed)
                    {
                        if (CanMovePlayers(data))
                        {
                            inputRecorder.replayQueue.Enqueue(new InputEvent(timer, data));

                            moves++;

                            if (solutionDisplayActive)
                                currentSolutionStep++;

                            if (OnPlayersStartToMove != null)
                                OnPlayersStartToMove();
                        }
                        MovePlayers(data);
                    }
                }
            }

            //if (hintPressed)
            //{
            //    hintPressed = false;

            //    if (inputRecorder.Load(levelPath))
            //    {
            //        isReplaying = true;
            //        timer = 0;
            //        Restart();
            //        return;
            //    }
            //    else
            //    {
            //        Debug.Log("Replay file not found: " + name);
            //    }
            //}
        }
        
        CheckLoseCondition();
        CheckWinCondition();

        if(mustRefreshShadows)
        {
            BlockShadow.RefreshShadows();
            mustRefreshShadows = false;
        }
    }

    public bool isHintAvailable
    {
        get
        {
            //return inputRecorder.Load(levelPath);
            return solution != null;
        }
    }

    public bool EverybodyFinishedMoving()
    {
        bool everybodyFinishedMoving = true;
        foreach (Player p in players)
        {
            if (p.IsMoving() && p.isControlledByPlayer)
            {
                everybodyFinishedMoving = false;
                break;
            }
        }

        if (everybodyFinishedMoving)
        {
            foreach (Block b in blocks.items)
            {
                MovableBox mb = b as MovableBox;
                if (mb == null) continue;
                if (mb.isTeleporting)
                {
                    everybodyFinishedMoving = false;
                    break;
                }
            }
        }

        return everybodyFinishedMoving;
    }

    public bool CanMovePlayers(TheInput.InputData ind)
    {
        foreach (Player p in players)
        {
            if (p.CanMove(ind.data) == true && p.isControlledByPlayer)
                return true;
        }
        return false;
    }

    public bool MovePlayers(TheInput.InputData ind, bool fast = false)
    {
        if (ind == null)
            return false;

        if (ind.type != TheInput.InputData.InputDataType.Movement)
            return false;

        foreach (Player p in players)
            if(p.isControlledByPlayer)
                p.StartMovement();

        
        foreach (Player p in players)
        {
            if (p.isControlledByPlayer)
                p.TryToMove(ind.data);
        }

        bool doneMovement = false;
        foreach (Player p in players)
            if (p.isControlledByPlayer)
            {
                doneMovement = p.CompleteMovement(fast) || doneMovement;
            }

        return doneMovement;
    }

    static public TheGame Instance
    {
        get
        {
            return _instance;
        }
    }

    void FillPlayers()
    {
        players = new List<Player>();

        Spawner[] allSpawners = blocks.GetBlocksOfType<Spawner>();

        foreach (Spawner s in allSpawners)
        {
            SpawnPlayer(s);
            //s.gameObject.SetActive(false);
        }
    }

    void SpawnPlayer(Spawner location)
    {
        GameObject playerToSpawn = location.playerToSpawn;

        // spawn boy and girl
        if(players.Count > 0)
        {
            string n1 = players[0].name;
            string n2 = playerToSpawn.name;

            if (n1 == n2)
            {
                playerToSpawn = Resources.Load<GameObject>(n1 == "Player" ? "PlayerGirl" : "Player");
            }
        }
        GameObject p = GameObject.Instantiate<GameObject>(playerToSpawn);
        Player player = p.GetComponent<Player>();
        players.Add(player);
        p.transform.localPosition = location.transform.localPosition;
        p.transform.localRotation = Quaternion.identity;
        p.transform.parent = transform.parent;

        p.name = location.playerToSpawn.name;// +players.Count.ToString();

        if (players.Count == 2 && players[0].kind == players[1].kind)
            players[1].kind = Player.Kind.Girl;


        Widget.SetLayer(p, LayerMask.NameToLayer("Game"));
    }

    void FillFinishingPoints()
    {
        finishingPoints = blocks.GetBlocksOfType<FinishingPoint>();
    }

    void CheckLoseCondition()
    {

    }

    public void TriggerGameLost()
    {
        Debug.Log("GAME OVER !!!");
        paused = true;
        if (gameEnded) return;
        gameEnded = true;

        if (OnGameEnded != null)
            OnGameEnded(false);

        isReplaying = false;
    }

    void CheckWinCondition()
    {
        if (finishingPoints == null) return;
        if (players.Count == 0) return;

        bool gameWon = true;

        foreach(FinishingPoint fp in finishingPoints)
        {
            if(fp.playersOnSite.Count == 0 || fp.state == Block.BlockState.State_Off)
            {
                gameWon = false;
                break;
            }
        }

        if (gameWon)
        {
            isReplaying = false;

            if (isReplaying)
            {
                timer = 0;
                Restart();
                return;
            }
            else
            {
#if UNITY_EDITOR
                string editorScenePath = "Assets/Scenes/LevelEditor.unity";
                if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path.Equals(editorScenePath))
                {
                    InputRecorder oldReplay = new InputRecorder();
                    if (oldReplay.Load(levelPath))
                    {
                        if (inputRecorder.BetterThan(oldReplay))
                        {
                            //Debug.Log("replacing old replay, this one was better");
                            inputRecorder.Save(levelPath);
                        }
                        else
                        {
                            Debug.Log("ignoring this replay, old one was better");
                        }
                    }
                    else
                    {
                        Debug.Log("saving new replay, no previous one found");
                        inputRecorder.Save(levelPath);
                    }
                }
#endif
            }
        }

        if (gameWon && finishingPoints.Length > 0)
        {
            if (gameEnded == false)
            {
                ghostsSimulation.StopSimulation();
                gameEnded = true;

                if (OnGameEnded != null)
                    OnGameEnded(true);
                DoWinAnimation();
                paused = true;
            }
        }
    }

    public Action<bool> OnGameEnded;
    public Action OnPlayersStartToMove;

    void CheckPlayersOverlapping()
    {
        float minDist = 10000.0f;

        bool thereAreBots = false;
        List<Player> overlappingPlayers = new List<Player>();
        foreach(Player p1 in players)
        {
            foreach(Player p2 in players)
            {
                if (p1 == p2) continue;

                float d = p1.DistanceTo3D(p2);
                if(d < minDist)
                {
                    minDist = d;
                    overlappingPlayers.Clear();
                    overlappingPlayers.Add(p1);
                    overlappingPlayers.Add(p2);

                    thereAreBots = p1.isBot || p2.isBot;
                }
            }
        }

        if (minDist < GameSession.gridUnit * 0.1f)
        {
            if (thereAreBots)
            {
                foreach (Player p in overlappingPlayers)
                    if (p.isBot)
                        p.Die(Player.DieMode.Overlap);
            }
            else
            {
                overlappingPlayers[0].Die(Player.DieMode.Overlap);
            }
        }
    }

    bool convertedToNewVersion = false;

    public Camera gameCamera;

    void GetGameCamera()
    {
        if(gameCamera != null) return;

        GameCamera gc = GameObject.FindObjectOfType<GameCamera>();
        if(gc != null)
        {
            gameCamera = gc.getCamera();
        }

#if UNITY_EDITOR
        if (gameCamera == null)
            gameCamera = Camera.main;
#endif
    }

    void ConvertToNewVersion()
    {
        if (convertedToNewVersion) return;
        convertedToNewVersion = true;

        //if (Desktop.main == null)
        blocks.Fill(transform.parent);

        GetGameCamera();

        if (gameCamera != null)
        {
            gameCamera.transparencySortMode = TransparencySortMode.Perspective;

            Vector3 gc = blocks.GetGroundCenter(); gc.y = 0.0f;
            gameCamera.transform.position = gc + Vector3.up * 236.0f;

            if(true)
            {
                if (gameCamera.orthographic)
                {
                    SetCameraSize();
                }
            }
        }

        Light dirL = GameObject.FindObjectOfType<Light>();
        if(dirL != null)
        {
            //dirL.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            dirL.transform.rotation = Quaternion.Euler(60.0f, 309.0f, 305.0f);
            dirL.intensity = 0.6f;
            dirL.shadows = LightShadows.None;
        }

        RenderSettings.fog = false;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogStartDistance = 0.21f;
        RenderSettings.fogEndDistance = 1.0f;// 0.81f;
        RenderSettings.fogDensity = 1.0f;
    }

    public void CenterCamera()
    {
        GetGameCamera();

        if (gameCamera != null)
        {
            gameCamera.transparencySortMode = TransparencySortMode.Perspective;

            Vector3 gc = blocks.GetGroundCenter(); gc.y = 0.0f;
            gameCamera.transform.position = gc + Vector3.up * 236.0f;

            if (true)
            {
                if (gameCamera.orthographic)
                {
                    SetCameraSize();
                }
            }
        }
    }

    void SetCameraSize()
    {
        Vector3 size = blocks.GetGroundSize() / GameSession.gridUnit;

        Vector2[] zoomSteps = new Vector2[3]
                    {
                        new Vector2(5.0f, 10.0f),
                        new Vector2(7.0f, 14.0f),
                        new Vector2(10.0f, 20.0f)
                    };

        for (int i = 0; i < zoomSteps.Length; i++)
        {
            if (size.x <= zoomSteps[i].x && size.z <= zoomSteps[i].y)
            {
                size.x = zoomSteps[i].x;
                break;
            }
        }

        float aspect = (float)Screen.width / (float)Screen.height;
        if (Desktop.main != null)
            aspect = Desktop.main.aspect;

        gameCamera.orthographicSize = 0.5f * (size.x + 2.0f) * GameSession.gridUnit / aspect;
        if (aspect >= 2.8f / 4.0f)
            gameCamera.orthographicSize *= 1.3f;
    }

    void DoWinAnimation()
    {
        foreach (Player p in players)
        {
            LeanTween.scale(p.gameObject, Vector3.one * 0.001f, 0.4f).setEase(LeanTweenType.easeOutBack).setDelay(0.4f);
        }
    }

    public Player PlayerAt(Vector3 pos)
    {
        float halfGrid = GameSession.gridUnit * 0.5f;
        foreach(Player p in players)
        {
            Vector3 d = p.position - pos;
            if (Mathf.Abs(d.x) <= halfGrid && Mathf.Abs(d.z) <= halfGrid)
                return p;
        }

        return null;
    }

    public GameEconomy.EconomyItemType _powerupActive = GameEconomy.EconomyItemType.Invalid;
    public GameEconomy.EconomyItemType powerupActive
    {
        get
        {
            return _powerupActive;
        }
        set
        {
            if (value != _powerupActive)
            {
                _powerupActive = value;
                if(_powerupActive == GameEconomy.EconomyItemType.Invalid)
                {
                    input.Clear();
                }
            }
        }
    }

    List<Block> blocksToCreate = new List<Block>();
    void ShowCreateTableEffect()
    {
        foreach(Block b in blocks.ground)
        {
            blocksToCreate.Add(b);
        }

        foreach (Block b in blocks.items)
        {
            if(b.blockType != Block.BlockType.Spawn)
                blocksToCreate.Add(b);
        }

        Block g1 = blocks.GroundAt(players[0].position);
        Block g2 = blocks.GroundAt(players[1].position);

        StartCoroutine(CreateGroundBlock(g1));
        StartCoroutine(CreateGroundBlock(g2));
    }

    void ShowDestroyTableEffect()
    {

    }

    IEnumerator CreateGroundBlock(Block b)
    {
        BoxCreateEffect[] bces = b.GetComponents<BoxCreateEffect>();
        float fxDuration = UnityEngine.Random.Range(1.0f, 1.5f) * 0.35f;
        if(bces != null && bces.Length > 0)
        {
            foreach (BoxCreateEffect bce in bces)
            {
                float delay = UnityEngine.Random.Range(0.1f, 0.3f);
                bce.duration = fxDuration;
                bce.ShowEffect(delay);
            }
        }

        blocksToCreate.Remove(b);

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.0f) * 0.016f);

        Block[] n = b.GetGroundNeighbours();
        bool newOne = false;
        foreach (Block nn in n)
        {
            if (blocksToCreate.Contains(nn))
            {
                StartCoroutine(CreateGroundBlock(nn));
                newOne = true;
            }
        }

        if(newOne == false)
        {
            if(blocksToCreate.Count > 0)
                StartCoroutine(CreateGroundBlock(blocksToCreate[0]));
        }
    }
}
