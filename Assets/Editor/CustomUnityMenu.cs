#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Diagnostics;

public class CustomUnityMenu
{
    [MenuItem("LinkTwin/Level Manager")]
    private static void StartLevelManager()
    {
        LevelManager.Init();
    }

    [MenuItem("LinkTwin/Level List Editor")]
    private static void StartLevelListEditor()
    {
        LevelListEditor.Init();
    }

    [MenuItem("LinkTwin/New Level List")]
    private static void CreateLevelList()
    {
        new GameObject("LevelList", typeof(LevelList));
    }

    [MenuItem("LinkTwin/Auto-arrange buttons")]
    private static void AutoArrangeButtons()
    {
        if (Selection.activeGameObject != null)
        {
            Transform rootTransform = Selection.activeGameObject.transform;
            RectTransform rt = Selection.activeGameObject.GetComponent<RectTransform>();
            SagaButtonPosition[] positions = rootTransform.GetComponentsInChildren<SagaButtonPosition>();
            UnityEngine.Debug.Log("Found " + positions.Length + " buttons");
            int x = 0;
            float y = 0;
            int dir = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                SagaButtonPosition button = positions[i];
                button.gameObject.name = "Level" + i;
                RectTransform buttonrt = button.GetComponent<RectTransform>();
                if (Random.Range(0, 100) < 25 || dir == 0)
                {
                    int r = Random.Range(0, 100);
                    if (r < 45)
                        dir = -1;
                    else if (r < 45)
                        dir = 1;
                    else
                        dir = 0;
                }
                int oldx = x;
                if (x + dir > 2 || x + dir < -2)
                    dir = -dir;
                x += dir;
                if (oldx == x)
                    y += 1.0f;
                else
                    y += 0.85f;
                buttonrt.anchoredPosition = new Vector2((float)x * 85.0f, -(rt.sizeDelta.y / 2.0f) + y * 85.0f);
            }
        }
    }

    [MenuItem("LinkTwin/Export localizations")]
    private static void ReloadLocaleMenuItem()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            EditorUtility.DisplayDialog("Error", "The excel exporter can only run on Windows", "Ok");
            return;
        }

        string basePath = Path.Combine(Application.dataPath, "Localization");
        string exporterPath = Path.Combine(basePath, "ExcelExporter.exe");
        string spreadsheetPath = Path.Combine(basePath, "texts.xlsx");
        string targetPath = Path.Combine(Application.dataPath, "Resources/Texts");
        string args = string.Format("\"{0}\" \"{1}\"", spreadsheetPath, targetPath).Replace("/", "\\");
        UnityEngine.Debug.Log(args);
        Process proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exporterPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        proc.Start();
        proc.WaitForExit();
        UnityEngine.Debug.Log(proc.StandardOutput.ReadToEnd());
        UnityEngine.Debug.Log(proc.StandardError.ReadToEnd());

        AssetDatabase.Refresh();

        string locale = Locale.DetectLocale();
        Locale.LoadLocale(locale);
    }
    /*
    [MenuItem("LinkTwin/Generate Borders")]
    private static void GenerateBorders()
    {
        GridCreator gc = GameObject.FindObjectOfType<GridCreator>();
        if(gc != null)
        {
            gc.GenerateBorders();
        }
    }*/

            //[MenuItem("LinkTwin/Export Table (deprecated)")]
            //private static void ExportTable()
            //{
            //    TableDescription td = null;

            //    string path = EditorUtility.SaveFilePanel("Save table ...", TableLoadSave.LoadSaveFolder(), "UntitledTable", "bytes");
            //    if (path.Length > 0)
            //    {
            //        TableLoadSave.ConvertSceneToMapDescription(ref td);

            //        td.author = GenerateGridValuesWindow.author;
            //        td.index = (ushort)GenerateGridValuesWindow.levelIndex;
            //        td.name = GenerateGridValuesWindow.title;

            //        TableLoadSave.SaveAbsolutePath(path, td);
            //    }
            //}

            //[MenuItem("LinkTwin/Import Table (deprecated)")]
            //private static void ImportTable()
            //{
            //    TableDescription td = null;

            //    string[] ext = new string[2] { "LinkTwin tables", "bytes" };
            //    string path = EditorUtility.OpenFilePanelWithFilters("Open table", TableLoadSave.LoadSaveFolder(), ext);

            //    if (path.Length > 0)
            //    {
            //        TableLoadSave.LoadAbsolutePath(path, ref td);

            //        CleanupScene();

            //        SetupSceneDefaults();

            //        TableLoadSave.ConvertMapDescriptionToScene(td);

            //        PostLoadSceneDefaults();

            //        GenerateGridValuesWindow.author = td.author;
            //        GenerateGridValuesWindow.levelIndex = td.index;
            //        GenerateGridValuesWindow.title = td.name;
            //    }
            //}

    [MenuItem("LinkTwin/Refresh Theme")]
    private static void RefreshTheme()
    {
        if (Desktop.main != null)
            Desktop.main.theme.hasChanged = true;
    }

    [MenuItem("LinkTwin/Delete PlayerPrefs")]
    private static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("LinkTwin/Update Amber Libs")]
    private static void UpdateAmberLibs()
    {
        BuildTools.PostProcessor.UpdateAmberLibs();
    }
}

#endif