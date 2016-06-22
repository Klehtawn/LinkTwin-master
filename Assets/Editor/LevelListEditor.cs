using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LevelListEditor : EditorWindow
{
    #region Editor window code
    static public LevelListEditor lleWindow = null;
    static public void Init()
    {
        if (lleWindow == null)
        {
            lleWindow = ScriptableObject.CreateInstance<LevelListEditor>();
            lleWindow.titleContent = new GUIContent("Level List");
            lleWindow.Show();
        }
        else
        {
            FocusWindowIfItsOpen<LevelListEditor>();
            lleWindow = EditorWindow.GetWindow<LevelListEditor>();
        }
    }
    #endregion

    #region Internal variables
    private bool documentChanged = false;
    private string currentDocumentPath = string.Empty;
    private string lastDocumentFolder;
    private string suggestedFilename = "LevelList1";
    private bool enableGUI;
    private bool detailedView;
    private Vector2 scrollPos = Vector2.zero;

    // --- list data
    private List<LevelLink> levels = new List<LevelLink>();
    private int standardLevelCount = 20;

    private int lastThumbnailCounter = 0;

    private static GUIContent moveUpButtonContent = new GUIContent("\u25b2", "move up");
    private static GUIContent moveDnButtonContent = new GUIContent("\u25bc", "move down");
    private static GUIContent editButtonContent = new GUIContent("Edit", "Open level in editor");

    private static GUIContent deleteSelectionButtonContent = new GUIContent("Delete", "Delete all selected levels");

    private static GUIContent selectAllButtonContent = new GUIContent("All", "Select all");
    private static GUIContent selectNoneButtonContent = new GUIContent("None", "Select none");

    private static GUIContent reloadButtonContent = new GUIContent("Reload", "Reload level info");
    private static GUIContent addButtonContent = new GUIContent("Add file...", "Add level");
    private static GUIContent exportButtonContent = new GUIContent("Export...", "Export the levels to a folder in an orderly fashion");
    private static GUIContent exportCSVButtonContent = new GUIContent("CSV...", "Export the level stats to a csv file");

    private static GUIContent clearAllButtonContent = new GUIContent("Clear", "Delete all level references");
    private static GUIContent loadFolderButtonContent = new GUIContent("Add folder...", "Load all levels in a specific folder");

    private static GUIContent newButtonContent = new GUIContent("New", "New level list");
    private static GUIContent loadButtonContent = new GUIContent("Load", "Load level list");
    private static GUIContent saveButtonContent = new GUIContent("Save", "Save level list");
    private static GUIContent saveAsButtonContent = new GUIContent("Save As", "Save level list to another file");
    private static GUIContent importButtonContent = new GUIContent("Import", "Import level list from old-style prefab");
    private static GUIContent exportToGameButtonContent = new GUIContent("Export lists", "Export all lists to game-ready format");

    private static GUILayoutOption miniButtonWidthSmall = GUILayout.Width(25f);
    private static GUILayoutOption miniButtonWidthMedium = GUILayout.Width(50f);
    private static GUILayoutOption miniButtonWidthLarge = GUILayout.Width(80f);
    #endregion

    const int currentListVersion = 2;

    void OnGUI()
    {
        Handles.BeginGUI();

        enableGUI = !Application.isPlaying;

        ShowHeaderButtons();
        ShowContentButtons();
        if (levels.Count > 0)
        {
            ShowMoveSelectButtons();
            ShowListDetails();
        }
        else
        {
            GUILayout.Label("List is empty");
        }

        Handles.EndGUI();
    }

    void OnEnable()
    {
        detailedView = EditorPrefs.GetBool("LevelListEditor.DetailedView", true);
        lastDocumentFolder = EditorPrefs.GetString("LevelListEditor.LastDocumentFolder", Application.dataPath);
    }

    void OnDisable()
    {
        EditorPrefs.SetBool("LevelListEditor.DetailedView", detailedView);
        EditorPrefs.SetString("LevelListEditor.LastDocumentFolder", lastDocumentFolder);
    }

    void Update()
    {
        if (Application.isPlaying)
            return;

        if (lastThumbnailCounter++ > 5)
        {
            foreach (LevelLink ll in levels)
                if (ll.thumbnail == null)
                {
                    ll.LoadLevelThumbnail();
                    lastThumbnailCounter = 0;
                    Repaint();
                    break;
                }
        }
    }

    private bool ConfirmCloseCurrentDocument()
    {
        const int Yes = 0;
        const int No = 1;
        const int Cancel = 2;

        // --- not much of a document to speak of, skip confirmation
        if (!documentChanged || (currentDocumentPath == string.Empty && levels.Count == 0))
        {
            return true;
        }

        int result = EditorUtility.DisplayDialogComplex("Confirm", "Do you want so save existing list?", "Yes", "No", "Cancel");
        switch (result)
        {
            case Yes:
                SaveDocument();
                return true;
            case No:
                return true;
            case Cancel:
                return false;
        }
        return false;
    }

    private void CloseDocument()
    {
        levels.Clear();
        currentDocumentPath = string.Empty;
        documentChanged = false;
        suggestedFilename = "LevelList1";
    }

    private void ShowHeaderButtons()
    {
        string fileinfo = currentDocumentPath == string.Empty ? "[Untitled]" : currentDocumentPath;
        if (documentChanged)
            fileinfo += "*";
        fileinfo += ", " + levels.Count + " levels";

        EditorGUILayout.LabelField(fileinfo);

        GUI.enabled = enableGUI;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(newButtonContent, GUILayout.Width(60)))
        {
            if (ConfirmCloseCurrentDocument())
                CloseDocument();
        }
        if (GUILayout.Button(loadButtonContent, GUILayout.Width(60)))
        {
            if (ConfirmCloseCurrentDocument())
                LoadDocument();
        }
        GUI.enabled = documentChanged && enableGUI;
        if (GUILayout.Button(saveButtonContent, GUILayout.Width(60)))
        {
            SaveDocument();
        }
        GUI.enabled = enableGUI;
        if (GUILayout.Button(saveAsButtonContent, GUILayout.Width(60)))
        {
            SaveDocumentAs();
        }
        if (GUILayout.Button(importButtonContent, GUILayout.Width(60)))
        {
            if (ConfirmCloseCurrentDocument())
                ImportLevelList();
        }
        if (GUILayout.Button(exportToGameButtonContent, GUILayout.Width(80)))
        {
            BuildTools.Builder.ExportLevelLists();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    private void SaveDocument()
    {
        if (currentDocumentPath.Length > 0)
            SaveDocument(currentDocumentPath);
        else
            SaveDocumentAs();
    }

    private void LoadDocumentFromString(string text)
    {
        text = text.Replace("\r", "");
        string[] lines = text.Split('\n');
        int ver = int.Parse(lines[0]);
        if (ver == 1)
            LoadDocumentVer1(lines);
        else if (ver == 2)
            LoadDocumentVer2(lines);
        else
            EditorUtility.DisplayDialog("Error", "Unknown levelist version on first line", "Ok");
    }

    private void LoadDocumentVer1(string[] lines)
    {
        int count = int.Parse(lines[1]);
        standardLevelCount = 20;
        for (int i = 0; i < count; i++)
        {
            string levelpath = lines[2 * i + 2];
            string comment = lines[2 * i + 3].Replace("\\n", "\n");
            EditorUtility.DisplayProgressBar("Processing levels", "" + (i + 1) + " of " + count, (float)(i + 1) / count);
            AddFile(levelpath, comment);
        }
        EditorUtility.ClearProgressBar();
    }

    private void LoadDocumentVer2(string[] lines)
    {
        int count = int.Parse(lines[1]);
        standardLevelCount = int.Parse(lines[2]);
        for (int i = 0; i < count; i++)
        {
            string levelpath = lines[2 * i + 3];
            string comment = lines[2 * i + 4].Replace("\\n", "\n");
            EditorUtility.DisplayProgressBar("Processing levels", "" + (i + 1) + " of " + count, (float)(i + 1) / count);
            AddFile(levelpath, comment);
        }
        EditorUtility.ClearProgressBar();
    }

    private void LoadDocument()
    {
        string[] ext = new string[2] { "LinkTwin level list", "levellist" };
        string path = EditorUtility.OpenFilePanelWithFilters("Open table", lastDocumentFolder, ext);

        if (path.Length > 0)
        {
            CloseDocument();
            string text = File.ReadAllText(path);
            LoadDocumentFromString(text);
            //CreateThumbnails();
            currentDocumentPath = path;
            lastDocumentFolder = Path.GetDirectoryName(currentDocumentPath);
            documentChanged = false;
        }
    }

    private void SaveDocumentAs()
    {
        string path = EditorUtility.SaveFilePanel("Save level list ...", lastDocumentFolder, suggestedFilename, "levellist");
        if (path.Length > 0)
        {
            SaveDocument(path);
        }
    }

    private string SaveDocumentToString()
    {
        string text = "" + currentListVersion + "\n";
        text += levels.Count + "\n";
        text += standardLevelCount + "\n";
        for (int i = 0; i < levels.Count; i++)
        {
            LevelLink ll = levels[i];
            text += ll.path + "\n" + ll.comment.Replace("\n", "\\n") + "\n";
        }
        return text;
    }

    private void SaveDocument(string path)
    {
        File.WriteAllText(path, SaveDocumentToString());
        currentDocumentPath = path;
        lastDocumentFolder = Path.GetDirectoryName(currentDocumentPath);
        documentChanged = false;
    }

    private void ShowListDetails()
    {
        if (lleWindow == null)
            lleWindow = EditorWindow.GetWindow<LevelListEditor>();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Standard level count: ", GUILayout.Width(130));
        standardLevelCount = EditorGUILayout.IntField(standardLevelCount, GUILayout.Width(26));
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < levels.Count; i++)
        {
            if (detailedView)
                ShowListItemDetailed(i);
            else
                ShowListItemSimple(i);
        }

        EditorGUILayout.EndScrollView();
    }

    private void ShowListItemDetailed(int i)
    {
        LevelLink ll = levels[i];

        const float kHeaderSize = 60;
        const float kThumbSize = 100;
        const float kInfoAreaWidth = 600;
        const float kOuterSpacing = 10;
        const float kInnerSpacing = 20;
        const float kCommentHeight = 60;
        const float kLevelButtonsAreaHeight = 80;
        const float kTotalHeight = kThumbSize + kOuterSpacing * 2;

        GUIStyle style = EditorStyles.helpBox;
        Rect r = EditorGUILayout.BeginVertical(style);

        float x = r.xMin + kOuterSpacing;
        float y = r.yMin + kOuterSpacing;

        // --- level buttons
        // --- margin between two elements is the max of the elements' facing margins
        float itemHeight = kTotalHeight + Mathf.Max(style.margin.top, style.margin.bottom) + style.padding.vertical;
        GUILayout.BeginArea(new Rect(x, y + i * itemHeight, kHeaderSize, kLevelButtonsAreaHeight));
        ShowLevelHeader(i);
        GUILayout.EndArea();
        x += kHeaderSize + kInnerSpacing;

        // --- thumbnail
        y = r.yMin + kOuterSpacing;
        Rect imageRect = new Rect(x, y, kThumbSize, kThumbSize);
        if (ll.thumbnail != null)
            EditorGUI.DrawPreviewTexture(imageRect, ll.thumbnail);
        x += kThumbSize + kInnerSpacing;

        // --- info and comment area
        y = r.yMin + kOuterSpacing;
        GUI.Label(new Rect(x, y, kInfoAreaWidth, kInnerSpacing), ll.filename + " - " + ll.path);
        y += kInnerSpacing;
        string stepsinfo = ll.minSteps > 0 ? "" + ll.minSteps + " moves" : "Solver couldn't find a fast solution, try running it by hand from LevelManager";
        GUI.Label(new Rect(x, y, kInfoAreaWidth, kInnerSpacing), stepsinfo);
        y += kInnerSpacing;
        string newcomment = ll.comment;
        newcomment = GUI.TextArea(new Rect(x, y, kInfoAreaWidth, kCommentHeight), ll.comment);
        if (newcomment != ll.comment)
        {
            ll.comment = newcomment;
            documentChanged = true;
        }
        y += kCommentHeight;
        x += kInfoAreaWidth + kInnerSpacing;

        GUILayout.Space(kTotalHeight);

        EditorGUILayout.EndVertical();
    }

    private void ShowListItemSimple(int i)
    {
        LevelLink ll = levels[i];

        const float kHeaderSize = 80;
        const float kThumbSize = 20;
        const float kInfoAreaWidth = 600;
        const float kOuterSpacing = 2;
        const float kInnerSpacing = 16;
        const float kLevelButtonsAreaHeight = 60;
        const float kTotalHeight = kThumbSize + kOuterSpacing * 2;

        GUIStyle style = EditorStyles.helpBox;
        Rect r = EditorGUILayout.BeginVertical(style);

        float x = r.xMin + kOuterSpacing + style.padding.left;
        float y = r.yMin + kOuterSpacing + style.padding.top;

        // --- level buttons
        // --- margin between two elements is the max of the elements' facing margins
        float itemHeight = kTotalHeight + Mathf.Max(style.margin.top, style.margin.bottom) + style.padding.vertical;
        GUILayout.BeginArea(new Rect(x + style.margin.left, y + i * itemHeight + style.margin.top, kHeaderSize, kLevelButtonsAreaHeight));
        ShowLevelHeaderSimple(i);
        GUILayout.EndArea();
        x += kHeaderSize + kInnerSpacing;

        // --- thumbnail
        Rect imageRect = new Rect(x, y, kThumbSize, kThumbSize);
        if (ll.thumbnail != null)
            EditorGUI.DrawPreviewTexture(imageRect, ll.thumbnail);
        x += kThumbSize + kInnerSpacing;

        // --- info and comment area
        GUI.Label(new Rect(x, y, kInfoAreaWidth, kInnerSpacing), ll.filename + " - " + ll.path);
        //y += kInnerSpacing;
        //GUI.Label(new Rect(x, y, kInfoAreaWidth, kInnerSpacing), "" + ll.minSteps + " moves");
        //y += kInnerSpacing;
        //ll.comment = GUI.TextArea(new Rect(x, y, kInfoAreaWidth, kCommentHeight), ll.comment);
        //y += kCommentHeight;
        //x += kInfoAreaWidth + kInnerSpacing;

        GUILayout.Space(kTotalHeight);

        EditorGUILayout.EndVertical();
    }

    private void ImportLevelList()
    {
        string[] ext = new string[2] { "Level List", "prefab" };
        string basepath = TableLoadSave.LoadSaveFolder().Replace('\\', '/');
        string path = EditorUtility.OpenFilePanelWithFilters("Open table", basepath, ext).Replace('\\', '/');

        if (path.Length > 0)
        {
            string relpath = path.Substring(basepath.Length, path.Length - basepath.Length - ".prefab".Length);
            LevelList prefab = Resources.Load<LevelList>(relpath);

            if (prefab != null)
            {
                CloseDocument();
                levels.AddRange(prefab.levels);
                suggestedFilename = Path.GetFileNameWithoutExtension(path);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "The file doesn't contain a LevelList component", "Ok");
            }
        }
    }

    private void CreateThumbnails()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            EditorUtility.DisplayProgressBar("Rendering thumbnails", "" + (i + 1) + " of " + levels.Count, (float)(i + 1) / levels.Count);
            levels[i].LoadLevelThumbnail();
        }

        EditorUtility.ClearProgressBar();
    }

    private static Texture2D ColorTex(Color col)
    {
        Color[] pixels = new Color[1];
        pixels[0] = col;

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private static GUIStyle GetListItemStyle(Color c)
    {
        GUIStyle style = new GUIStyle();
        style.normal.background = ColorTex(c);
        //style.border = new RectOffset(2, 2, 2, 2);
        return style;
    }

    private void ShowContentButtons()
    {
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = enableGUI;

        // --- reload all level data
        if (GUILayout.Button(reloadButtonContent, EditorStyles.miniButton, miniButtonWidthMedium))
        {
            for (int i = 0; i < levels.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Reloading levels", "Processing level " + (i + 1) + " of " + levels.Count, (float)i / (float)levels.Count);
                UpdateLevelData(i);
            }
            EditorUtility.ClearProgressBar();
        }

        GUI.enabled = enableGUI && levels.Count > 0;
        // --- clear the level list
        if (GUILayout.Button(clearAllButtonContent, EditorStyles.miniButton, miniButtonWidthMedium))
        {
            levels.Clear();
            documentChanged = true;
        }

        GUI.enabled = enableGUI;
        // --- add a level file
        if (GUILayout.Button(addButtonContent, EditorStyles.miniButtonLeft, miniButtonWidthLarge))
        {
            string[] ext = new string[2] { "LinkTwin tables", "bytes" };
            string path = EditorUtility.OpenFilePanelWithFilters("Open table", TableLoadSave.LoadSaveFolder(), ext);
            if (path != null && path != string.Empty)
            {
                AddFile(path, string.Empty);
            }
        }

        // --- add all the files in a specific folder
        if (GUILayout.Button(loadFolderButtonContent, EditorStyles.miniButtonRight, miniButtonWidthLarge))
        {
            string path = EditorUtility.OpenFolderPanel("Select levels folder", TableLoadSave.LoadSaveFolder(), "");
            if (path != null && path != string.Empty)
            {
                string[] files = Directory.GetFiles(path, "*.bytes");
                for (int i = 0; i < files.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Adding levels", "Processing level " + (i + 1) + " of " + files.Length, (float)i / (float)files.Length);
                    if (!files[i].EndsWith(".replay.bytes"))
                        AddFile(files[i], string.Empty);
                }
                EditorUtility.ClearProgressBar();
            }
        }

        GUI.enabled = enableGUI && levels.Count > 0;
        // --- export everything to a certain folder
        if (GUILayout.Button(exportButtonContent, EditorStyles.miniButton, miniButtonWidthLarge))
        {
            string path = EditorUtility.OpenFolderPanel("Select levels folder", TableLoadSave.LoadSaveFolder(), "");
            if (path != null && path != string.Empty)
            {
                for (int i = 0; i < levels.Count; i++)
                {
                    string srcPath = Path.Combine(TableLoadSave.LoadSaveFolder(), levels[i].path + ".bytes");
                    string destPath = Path.Combine(path, string.Format("Level{0:D2}.bytes", i + 1));
                    FileUtil.CopyFileOrDirectory(srcPath, destPath);
                }
            }
        }

        // --- export level statistics to csv
        if (GUILayout.Button(exportCSVButtonContent, EditorStyles.miniButton, miniButtonWidthLarge))
        {
            string path = EditorUtility.SaveFilePanel("Export CSV", Application.dataPath, "LevelStats", "csv");
            if (path != null && path != string.Empty)
            {
                string s = string.Empty;
                s += "Level number,Width,Height,Solution length,Boxes,Buttons,Push Buttons,Doors,Crumbling Tiles,Moveable Boxes,Portals,Walls,Clone distance,Exit distance,Difficulty score,dist_var,local_dist,switches,moves";
                if (levels.Count > 0)
                    for (int i = 0; i < levels.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("Reloading levels", "Processing level " + (i + 1) + " of " + levels.Count, (float)i / (float)levels.Count);
                        UpdateLevelData(i, 10, 100);
                        LevelLink link = levels[i];
                        LevelStats stats = link.levelStats;
                        s += "\n" + (i + 1)
                            + "," + stats.width
                            + "," + stats.height
                            + "," + stats.minSteps
                            + "," + stats.boxes
                            + "," + stats.buttons
                            + "," + stats.pushbuttons
                            + "," + stats.doors
                            + "," + stats.crumbling
                            + "," + stats.moveableBoxes
                            + "," + stats.portals
                            + "," + stats.walls
                            + "," + stats.cloneDistance
                            + "," + stats.exitDistance
                            + "," + link.score
                            + "," + link.levelSim.c_dist_variation
                            + "," + link.levelSim.c_local_dist
                            + "," + link.levelSim.c_switches
                            + "," + link.levelSim.c_moves;
                    }

                File.WriteAllText(path, s);

                EditorUtility.ClearProgressBar();
            }
        }

        GUI.enabled = enableGUI;

        EditorGUILayout.EndHorizontal();
    }

    private void ShowLevelHeader(int index)
    {
        GUILayout.BeginHorizontal();
        // --- item number and selection toggle
        string indexText = (index + 1).ToString();
        if (index >= standardLevelCount)
            indexText = "Bonus " + (index - standardLevelCount).ToString();
        levels[index].selected = GUILayout.Toggle(levels[index].selected, indexText);
        //GUILayout.Label();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        // --- move item up
        if (GUILayout.Button(moveUpButtonContent, EditorStyles.miniButtonLeft))
        {
            if (index > 0)
                SwapLevels(index, index - 1);
        }
        // --- move item down
        if (GUILayout.Button(moveDnButtonContent, EditorStyles.miniButtonRight))
        {
            if (index < levels.Count - 1)
                SwapLevels(index, index + 1);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        // --- open in editor
        if (GUILayout.Button(editButtonContent, EditorStyles.miniButton))
        {
            string path = levels[index].path;
            if (LevelManager.lmWindow == null)
                LevelManager.Init();
            LevelManager.lmWindow.LoadLevelRelativePath(path);
        }
        GUILayout.EndHorizontal();
    }

    private void ShowLevelHeaderSimple(int index)
    {
        GUILayout.BeginHorizontal();
        // --- item number and selection toggle
        string indexText = (index + 1).ToString();
        if (index >= standardLevelCount)
            indexText = "B" + (index - standardLevelCount).ToString();
        levels[index].selected = GUILayout.Toggle(levels[index].selected, indexText);

        // --- open in editor
        if (GUILayout.Button(editButtonContent, EditorStyles.miniButton))
        {
            string path = levels[index].path;
            if (LevelManager.lmWindow == null)
                LevelManager.Init();
            LevelManager.lmWindow.LoadLevelRelativePath(path);
        }
        GUILayout.EndHorizontal();
    }

    private void ShowMoveSelectButtons()
    {
        EditorGUILayout.BeginHorizontal();

        // --- move selection up
        if (GUILayout.Button(moveUpButtonContent, EditorStyles.miniButtonLeft, miniButtonWidthSmall))
        {
            for (int i = 1; i < levels.Count; i++)
                if (levels[i].selected && !levels[i - 1].selected)
                    SwapLevels(i, i - 1);
        }
        // --- move selection down
        if (GUILayout.Button(moveDnButtonContent, EditorStyles.miniButtonRight, miniButtonWidthSmall))
        {
            for (int i = levels.Count - 2; i >= 0; i--)
                if (levels[i].selected && !levels[i + 1].selected)
                    SwapLevels(i, i + 1);
        }

        // --- delete selection
        if (GUILayout.Button(deleteSelectionButtonContent, EditorStyles.miniButton, miniButtonWidthMedium))
        {
            for (int i = levels.Count - 1; i >= 0; i--)
                if (levels[i].selected)
                {
                    levels.RemoveAt(i);
                    documentChanged = true;
                }
        }

        // --- select all
        if (GUILayout.Button(selectAllButtonContent, EditorStyles.miniButtonLeft, miniButtonWidthMedium))
        {
            for (int i = 0; i < levels.Count; i++)
                levels[i].selected = true;
        }
        // --- select none
        if (GUILayout.Button(selectNoneButtonContent, EditorStyles.miniButtonRight, miniButtonWidthMedium))
        {
            for (int i = 0; i < levels.Count; i++)
                levels[i].selected = false;
        }

        detailedView = GUILayout.Toggle(detailedView, "Detailed view");

        EditorGUILayout.EndHorizontal();
    }

    private void SwapLevels(int index1, int index2)
    {
        LevelLink temp = levels[index2];
        levels[index2] = levels[index1];
        levels[index1] = temp;
        documentChanged = true;
    }

    public void UpdateLevelData(int index, float timelimit = 2, int maxsolutions = 1)
    {
        levels[index].LoadLevelInfo();
        levels[index].RunLevelSimulation(timelimit, maxsolutions);
        //levels[index].LoadLevelThumbnail();
    }

    public void AddFile(string path, string comment)
    {
        LevelLink link = new LevelLink();
        string clearedPath = path.Replace('\\', '/');
        if (!link.SetPath(path.Replace('\\', '/')))
            EditorUtility.DisplayDialog("File not found", "Can't open file: \"" + path + "\"", "Ok");
        link.comment = comment;
        levels.Add(link);

        UpdateLevelData(levels.IndexOf(link), 0.1f, 1);

        documentChanged = true;
    }

    public void LoadFolder(string path)
    {
        string[] files = Directory.GetFiles(path, "*.bytes");
        foreach (string file in files)
            AddFile(file, string.Empty);

        documentChanged = true;
    }

}
