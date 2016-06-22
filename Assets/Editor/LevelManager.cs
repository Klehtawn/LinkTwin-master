#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.IO;

public class LevelManager : EditorWindow
{
    public const string editorScenePath = "Assets/Scenes/LevelEditor.unity";

    static public LevelManager lmWindow = null;
    static public void Init()
    {
        if (lmWindow == null)
        {
            lmWindow = ScriptableObject.CreateInstance<LevelManager>();
            lmWindow.titleContent = new GUIContent("Level Manager");
            lmWindow.Show();
        }
        else
        {
            FocusWindowIfItsOpen<LevelManager>();
        }
    }

    #region Internal variables
    private LevelLink levelLink;
    private TableDescription tableDesc;
    private LevelRoot levelRoot;
    private string lastFileOpenPath;

    private int gridWidth;
    private int gridHeight;

    private int genHoles;
    private int genBoxes;
    private int genMinMoves;

    private bool cleanAfterGenerate;

    private bool foldGenerator = false;
    private bool foldSolutions = false;
    private bool foldManualDetails = false;
    private bool foldDetails = false;
    private bool showZeroes = true;

    private string generatorMessage = null;

    private string author = System.Environment.UserName;
    private List<LevelSimulation.Solution> solutions = null;
    private string levelTitle = "UserGeneratedMap";
    private int levelIndex = 0;
    private bool enableGUI = true;
    private int maxSimTime = 200000;
    private int maxSolutions = 100;
    private bool editSimParams = false;

    private Vector2 scrollPos = Vector2.zero;
    #endregion

    void Start()
    {
        InitValues();
    }

    void Update()
    {
    }

    void OnInspectorUpdate()
    {
        //AutoAquireLevelRoot();
    }

    private void AutoAquireLevelRoot()
    {
        EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;

        if (LevelNotPresent() || levelLink == null)
        {
            LevelRoot[] roots = FindObjectsOfType<LevelRoot>();
            if (roots.Length > 0)
            {
                if (roots.Length > 1)
                    Debug.LogWarning("[LevelManager] Multiple roots found in scene: " + roots.Length);

                levelRoot = roots[0];
                levelRoot.InitStructure();
                Debug.Log("[LevelManager] Auto-aquired level root");

                InitValues();
            }
        }

        EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
    }

    void OnEnable()
    {
        if (lmWindow == null)
            lmWindow = this;

        lastFileOpenPath = EditorPrefs.GetString("LevelManager.LastFileOpenPath", Application.dataPath);

        AutoAquireLevelRoot();
    }

    void OnDisable()
    {
        EditorPrefs.SetString("LevelManager.LastFileOpenPath", lastFileOpenPath);
    }

    void OnFocus()
    {
        //InitValues();
    }

    bool LevelNotPresent()
    {
        return levelRoot == null;
    }

    void InitValues(bool runSimulation = false)
    {
        if (LevelNotPresent())
            return;

        EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;

        if (levelLink == null)
            levelLink = new LevelLink();

        levelLink.LoadFromCurrent(levelRoot);

        if (runSimulation)
        {
            levelLink.RunLevelSimulation(maxSimTime, maxSolutions);
            solutions = levelLink.levelSim.Solutions;
        }

        EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;

        Repaint();
    }

    void OnHierarchyChanged()
    {
        if (LevelNotPresent())
            return;

        if (Selection.activeGameObject != null)
        {
            Block block = Selection.activeGameObject.GetComponent<Block>();
            if (block != null && block.blockType != Block.BlockType.Ground && block.gameObject.activeInHierarchy)
            {
                AutoAquireLevelRoot();

                if (block.transform.parent == null)
                {
                    block.transform.parent = levelRoot.Table.transform;
                    InitValues();
                }
            }
        }
    }

    static public void SetupSceneDefaults()
    {
        TheGame tg = GameObject.FindObjectOfType<TheGame>();
        if (tg == null)
        {
            GameObject obj = new GameObject();
            obj.name = "TheGame";
            tg = obj.AddComponent<TheGame>();
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            cam = new GameObject("Main camera").AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        cam.transparencySortMode = TransparencySortMode.Orthographic;
        cam.transform.position = Vector3.up * 236.0f;
        cam.transform.rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
        cam.orthographic = true;
        cam.backgroundColor = new Color32(251, 246, 240, 255);
    }

    static public void PostLoadSceneDefaults()
    {
        Blocks blocks = new Blocks();
        blocks.Fill();

        Vector3 size = blocks.GetGroundSize() / GameSession.gridUnit;

        Camera.main.orthographicSize = 2.0f * size.x * GameSession.gridUnit / ((float)Screen.width / (float)Screen.height);

        Vector3 gc = blocks.GetGroundCenter(); gc.y = 0.0f;
        Camera.main.transform.position = gc + Vector3.up * 236.0f;

        RenderSettings.ambientLight = Color.white;
        RenderSettings.ambientIntensity = 1.0f;
    }

    void ResetRoot()
    {
        levelLink = new LevelLink();

        if (levelRoot != null)
            LevelRoot.DestroyRoot(ref levelRoot);

        levelRoot = LevelRoot.CreateRoot("LevelRoot");
    }

    void Generate()
    {
        EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;

        SetupSceneDefaults();
        LevelSimulation.Generator generator;
        LevelSimulation.Solver solver;

        generatorMessage = null;
        int maxTries = 10000;
        int count = 0;

        do
        {
            ResetRoot();

            generator = new LevelSimulation.Generator();
            generator.Generate(
                gridWidth,
                gridHeight,
                genHoles,
                genBoxes
                );
            //generator.ConvertToLevel(levelRoot);

            //PostLoadSceneDefaults();

            solver = new LevelSimulation.Solver();

            solver.Init(generator.GetBoard());

            //InitValues(true);
            solver.RunSolver();

            count++;
        }
        while (count < maxTries && (solver.Solutions == null || solver.Solutions.Count == 0 || solver.Solutions[0].NumSteps < genMinMoves));

        if (count == maxTries)
        {
            generatorMessage = "Failed after " + maxTries + " tries. Stopped for a breath. Run me again.";
        }
        else
        {
            generator.ConvertToLevel(levelRoot);

            //GameObject oldRoot = GameObject.Find("LevelRootOld");
            //if (oldRoot != null)
            //    DestroyImmediate(oldRoot);
            if (cleanAfterGenerate)
                CleanUnusedTilesAndObjects(true);

            InitValues(true);
        }

        EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
    }

    void NewEmptyBoard()
    {
        EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;

        ResetRoot();

        SetupSceneDefaults();

        Vector3 offset = new Vector3(.5f * (gridWidth % 2), 0, .5f * (gridHeight % 2));
        levelRoot.Ground.CreateBlocks(gridWidth, gridHeight, offset);

        PostLoadSceneDefaults();

        InitValues();

        EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
    }

    public void LoadLevelRelativePath(string path)
    {
        LoadLevel(path);
    }

    void SetLevelPath(string path)
    {
        TheGame tg = GameObject.FindObjectOfType<TheGame>();
        tg.levelPath = path;
    }

    void LoadLevel(string path)
    {
        EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;

        ResetRoot();

        levelLink.SetPath(path);

        TableLoadSave.Load(levelLink.path, ref tableDesc, levelRoot);
        SetupSceneDefaults();
        TableLoadSave.ConvertMapDescriptionToScene(tableDesc, levelRoot);
        PostLoadSceneDefaults();

        author = tableDesc.author;
        levelIndex = tableDesc.index;
        levelTitle = tableDesc.name;
        solutions = null;

        levelLink.LoadFromCurrent(levelRoot);
        SetLevelPath(levelLink.path);

        levelRoot.tableDescription = tableDesc;

        //InitValues();

        EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;

        FocusWindowIfItsOpen<LevelManager>();
    }

    void LoadLevel()
    {
        string[] ext = new string[2] { "LinkTwin tables", "bytes" };
        string path = EditorUtility.OpenFilePanelWithFilters("Open table", lastFileOpenPath, ext);

        if (path.Length > 0)
        {
            lastFileOpenPath = Path.GetDirectoryName(path);
            LoadLevel(path);
        }
    }

    void SaveLevel()
    {
        string defaultName = levelLink.filename;
        if (defaultName == null || defaultName.Length > 0)
            defaultName = EditorSceneManager.GetActiveScene().name;
        string path = EditorUtility.SaveFilePanel("Save table ...", lastFileOpenPath, defaultName, "bytes");
        if (path.Length > 0)
        {
            lastFileOpenPath = Path.GetDirectoryName(path);

            EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;

            TableDescription td = null;

            TableLoadSave.ConvertSceneToMapDescription(ref td);

            td.author = author;
            td.index = (ushort)levelIndex;
            td.name = levelTitle;

            levelLink.LoadFromCurrent(levelRoot);
            levelLink.RunLevelSimulation(1.0f, 1);
            solutions = levelLink.levelSim.Solutions;

            if (solutions != null && solutions.Count > 0)
            {
                td.numSolutionSteps = (ushort)solutions[0].NumSteps;
                td.solutionSteps = new byte[td.numSolutionSteps];
                for (int i = 0; i < td.numSolutionSteps; i++)
                    td.solutionSteps[i] = solutions[0].steplist[i].id;
            }
            else
            {
                Debug.LogWarning("Unable to fast solve level and write solution to file!!!");
                td.numSolutionSteps = 0;
            }

            TableLoadSave.SaveAbsolutePath(path, td);

            levelLink.SetPath(path);
            SetLevelPath(levelLink.path);

            InitValues();

            EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
        }
    }

    void ClearLevel()
    {
        EditorApplication.hierarchyWindowChanged -= OnHierarchyChanged;

        if (levelRoot != null)
            LevelRoot.DestroyRoot(ref levelRoot);

        InitValues();

        EditorApplication.hierarchyWindowChanged += OnHierarchyChanged;
    }

    void KeyValueLabel(string label1, string label2)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label1);
        EditorGUILayout.LabelField(label2);
        EditorGUILayout.EndHorizontal();
    }

    void ManualStatField(ref byte stat, string name)
    {
        //byte oldValue = stat;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(name);
        //stat = (byte)EditorGUILayout.IntField(stat);
        EditorGUILayout.LabelField(stat.ToString());
        EditorGUILayout.EndHorizontal();
        //if (oldValue != stat)
        //    InitValues();
    }

    void OnGUI()
    {
        Handles.BeginGUI();

        enableGUI = !Application.isPlaying;

        if (!EditorSceneManager.GetActiveScene().path.Equals(editorScenePath))
            ShowRequiredScene();

        ShowLevelDetails();

        Handles.EndGUI();
    }

    private void ShowRequiredScene()
    {
        if (Application.isPlaying)
            return;
        EditorGUILayout.HelpBox("Level manager needs to run in the LevelEditor scene.", MessageType.Info);
        if (GUILayout.Button("Load LevelEditor scene"))
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                try
                {
                    EditorSceneManager.OpenScene(editorScenePath);
                }
                catch (System.ArgumentException e)
                {
                    Debug.Log(e);
                    UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
                    new GameObject("TheGame", typeof(TheGame));
                    EditorSceneManager.SaveScene(scene, editorScenePath);
                }
            }
        }
    }

    private void ShowButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Load", GUILayout.Width(50)))
            LoadLevel();

        if (LevelNotPresent())
            GUI.enabled = false;

        if (GUILayout.Button("Save", GUILayout.Width(50)))
            SaveLevel();
        if (GUILayout.Button("Clear", GUILayout.Width(50)))
            ClearLevel();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = levelRoot != null;
        if (GUILayout.Button("Fix camera", GUILayout.Width(80)))
        {
            Vector3 pos = levelRoot.transform.position + Vector3.back * 10;
            SceneView.lastActiveSceneView.LookAt(pos, Quaternion.Euler(90, 0, 0), 100, true);
        }
        if (GUILayout.Button("Clean unused", GUILayout.Width(90)))
        {
            CleanUnusedTilesAndObjects();
        }
        if (GUILayout.Button("Refresh", GUILayout.Width(80)))
        {
            InitValues(true);
            foldSolutions = true;
        }
        editSimParams = GUILayout.Toggle(editSimParams, "Edit params");
        EditorGUILayout.EndHorizontal();

        if (editSimParams)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max sim time", GUILayout.Width(100));
            maxSimTime = EditorGUILayout.IntField(maxSimTime, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Max solutions", GUILayout.Width(100));
            maxSolutions = EditorGUILayout.IntField(maxSolutions, GUILayout.Width(60));
            GUI.enabled = enableGUI;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();
    }

    private void ShowGeneratorParams()
    {
        GUI.enabled = enableGUI;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        foldGenerator = EditorGUILayout.Foldout(foldGenerator, "Generator");

        if (foldGenerator)
        {
            EditorGUILayout.BeginHorizontal();
            gridWidth = EditorGUILayout.IntField(gridWidth, GUILayout.Width(20));
            EditorGUILayout.LabelField("x", GUILayout.Width(10));
            gridHeight = EditorGUILayout.IntField(gridHeight, GUILayout.Width(20));

            if (gridWidth > 0 && gridHeight > 0)
                GUI.enabled = enableGUI;
            else
                GUI.enabled = false;

            if (GUILayout.Button("Blank", GUILayout.Width(50)))
                NewEmptyBoard();
            if (GUILayout.Button("Generate", GUILayout.Width(70)))
                Generate();

            EditorGUILayout.LabelField("Auto clean", GUILayout.Width(70));
            cleanAfterGenerate = EditorGUILayout.Toggle(cleanAfterGenerate);

            GUI.enabled = enableGUI;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Holes", GUILayout.Width(100));
            genHoles = EditorGUILayout.IntSlider(genHoles, 0, 50, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Boxes", GUILayout.Width(100));
            genBoxes = EditorGUILayout.IntSlider(genBoxes, 10, 50, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Min moves", GUILayout.Width(100));
            genMinMoves = EditorGUILayout.IntSlider(genMinMoves, 1, 30, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();

            if (generatorMessage != null)
                EditorGUILayout.LabelField(generatorMessage);
        }

        EditorGUILayout.EndVertical();
    }

    private void CleanUnusedTilesAndObjects(bool backup = true)
    {
        InitValues(true);
        levelLink.levelSim.CleanUnused();

        if (backup)
        {
            levelRoot.gameObject.name = "LevelRootOld";
            levelRoot.gameObject.SetActive(false);
        }
        else
        {
            LevelRoot.DestroyRoot(ref levelRoot);
        }

        levelRoot = LevelRoot.CreateRoot("LevelRoot");
        levelLink.levelSim.ConvertCleanedBoardToLevel(levelRoot);
    }

    private void ShowLevelDetails()
    {
        AutoAquireLevelRoot();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUI.enabled = enableGUI;

        ShowButtons();
        ShowGeneralInfo();
        ShowGeneratorParams();

        if (LevelNotPresent())
        {
            EditorGUILayout.HelpBox("No level data found in scene", MessageType.Info);
        }
        else
        {
            ShowSolutions();
            ShowManualStatistics();
            ShowAutomaticStatistics();
            ShowErrors();
        }

        GUI.enabled = true;

        EditorGUILayout.EndScrollView();
    }

    private void ShowErrors()
    {
        LevelStats levelStats = levelLink.levelStats;
        if (levelStats.error != null && levelStats.error != string.Empty)
        {
            EditorGUILayout.HelpBox(levelStats.error, MessageType.Error);
        }
    }

    private void ShowGeneralInfo()
    {
        if (LevelNotPresent())
            return;
        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Name: " + levelLink.filename);
        //EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Path: " + levelLink.path);
        EditorGUILayout.EndHorizontal();

        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Difficulty score: " + levelLink.score);
        //EditorGUILayout.EndHorizontal();

        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Author");
        //author = EditorGUILayout.TextField(author);
        //EditorGUILayout.EndHorizontal();

        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Level Title");
        //levelTitle = EditorGUILayout.TextField(levelTitle);
        //EditorGUILayout.EndHorizontal();
    }

    private void ShowSolutions()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (solutions != null)
        {
            foldSolutions = EditorGUILayout.Foldout(foldSolutions, "Solutions (found " + solutions.Count + ", max " + maxSolutions + ") ");
            if (foldSolutions)
            {
                EditorGUI.indentLevel += 1;
                foreach (LevelSimulation.Solution s in solutions)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("" + s.steps + " steps: ", GUILayout.Width(70));
                    EditorGUILayout.SelectableLabel(s.description, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel -= 1;
            }
        }
        else
        {
            EditorGUILayout.LabelField("Solutions not available. Run the solver.");
        }
        EditorGUILayout.EndVertical();
    }

    private void ShowAutomaticStatistics()
    {
        LevelStats levelStats = levelLink.levelStats;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        foldDetails = EditorGUILayout.Foldout(foldDetails, "Level statistics");
        showZeroes = EditorGUILayout.Toggle("Show empty stats", showZeroes);
        EditorGUILayout.EndHorizontal();

        if (foldDetails)
        {
            EditorGUI.indentLevel += 1;
            KeyValueLabel("Field size", string.Format("{0}x{1} ({2} total)", levelStats.width, levelStats.height, levelStats.width * levelStats.height));
            if (showZeroes || levelStats.boxes > 0)
                KeyValueLabel("Boxes", string.Format("{0}", levelStats.boxes));
            if (showZeroes || levelStats.buttons > 0)
                KeyValueLabel("Buttons", string.Format("{0}", levelStats.buttons));
            if (showZeroes || levelStats.pushbuttons > 0)
                KeyValueLabel("Push Buttons", string.Format("{0}", levelStats.pushbuttons));
            if (showZeroes || levelStats.doors > 0)
                KeyValueLabel("Doors", string.Format("{0}", levelStats.doors));
            if (showZeroes || levelStats.crumbling > 0)
                KeyValueLabel("Crumbling tiles", string.Format("{0}", levelStats.crumbling));
            if (showZeroes || levelStats.moveableBoxes > 0)
                KeyValueLabel("Moveable boxes", string.Format("{0}", levelStats.moveableBoxes));
            if (showZeroes || levelStats.movingPlatforms > 0)
                KeyValueLabel("Moving platforms", string.Format("{0}", levelStats.movingPlatforms));
            if (showZeroes || levelStats.portals > 0)
                KeyValueLabel("Portals", string.Format("{0}", levelStats.portals));
            if (showZeroes || levelStats.pistons > 0)
                KeyValueLabel("Pistons", string.Format("{0}", levelStats.pistons));
            if (showZeroes || levelStats.walls > 0)
                KeyValueLabel("Walls", string.Format("{0}", levelStats.walls));
            KeyValueLabel("Clone distance", string.Format("{0}", levelStats.cloneDistance));
            KeyValueLabel("Exit distance", string.Format("{0}", levelStats.exitDistance));
            EditorGUI.indentLevel -= 1;
        }
        EditorGUILayout.EndVertical();
    }

    private void ShowManualStatistics()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        foldManualDetails = EditorGUILayout.Foldout(foldManualDetails, "Manual statistics");
        EditorGUILayout.EndHorizontal();
        if (foldManualDetails)
        {
            LevelStats levelStats = levelLink.levelStats;
            EditorGUI.indentLevel += 1;
            KeyValueLabel("Minimum steps", levelStats.minSteps.ToString());
            KeyValueLabel("Distinct solutions", levelStats.numSolutions.ToString());
            EditorGUI.indentLevel -= 1;
        }
        EditorGUILayout.EndVertical();
    }
}
#endif