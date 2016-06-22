using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;

[CustomEditor(typeof(LevelList))]
public class LevelListInspector : Editor
{
    private static GUIContent moveUpButtonContent = new GUIContent("\u25b2", "move up");
    private static GUIContent moveDnButtonContent = new GUIContent("\u25bc", "move down");

    private static GUIContent selectAllButtonContent = new GUIContent("All", "Select all");
    private static GUIContent selectNoneButtonContent = new GUIContent("None", "Select none");

    private static GUIContent reloadButtonContent = new GUIContent("Reload", "Reload level info");
    private static GUIContent addButtonContent = new GUIContent("Add file...", "Add level");
    private static GUIContent exportButtonContent = new GUIContent("Export...", "Export the levels to a folder in an orderly fashion");
    private static GUIContent exportCSVButtonContent = new GUIContent("CSV...", "Export the level stats to a csv file");

    private static GUIContent clearAllButtonContent = new GUIContent("Clear", "Delete all level references");
    private static GUIContent loadFolderButtonContent = new GUIContent("Add folder...", "Load all levels in a specific folder");

    private static GUILayoutOption miniButtonWidthSmall = GUILayout.Width(20f);
    private static GUILayoutOption miniButtonWidthMedium = GUILayout.Width(50f);
    private static GUILayoutOption miniButtonWidthLarge = GUILayout.Width(80f);

    public GUIStyle listItemStyle1 = new GUIStyle();//GetListItemStyle(new Color(1.0f, 1.0f, 1.0f, 0.2f));
    public GUIStyle listItemStyle2 = GetListItemStyle(new Color(0.5f, 0.5f, 0.5f, 0.15f));
    public GUIStyle listItemStyle3 = GetListItemStyle(new Color(0.6f, 0.6f, 0.6f, 0.25f));

    Vector2 scrollPos = Vector2.zero;

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            GUI.enabled = false;
            //EditorGUILayout.HelpBox("Level list editor not available in play mode", MessageType.None);
            //return;
        }

        serializedObject.Update();

        ShowList(serializedObject.FindProperty("levels"));
        serializedObject.ApplyModifiedProperties();
        GUI.enabled = true;
    }

    private void ShowContentButtons()
    {
        EditorGUILayout.BeginHorizontal();

        // --- reload all level data
        if (GUILayout.Button(reloadButtonContent, EditorStyles.miniButton, miniButtonWidthMedium))
        {
            LevelList ll = serializedObject.targetObject as LevelList;
            if (ll.levels != null && ll.levels.Count > 0)
                for (int i = 0; i < ll.levels.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Reloading levels", "Processing level " + (i + 1) + " of " + ll.levels.Count, (float)i / (float)ll.levels.Count);
                    ll.UpdateLevelData(i);
                }
            EditorUtility.ClearProgressBar();
        }

        // --- clear the level list
        if (GUILayout.Button(clearAllButtonContent, EditorStyles.miniButton, miniButtonWidthMedium))
        {
            LevelList ll = serializedObject.targetObject as LevelList;
            ll.Clear();
        }

        // --- add a level file
        if (GUILayout.Button(addButtonContent, EditorStyles.miniButtonLeft, miniButtonWidthLarge))
        {
            string[] ext = new string[2] { "LinkTwin tables", "bytes" };
            string path = EditorUtility.OpenFilePanelWithFilters("Open table", TableLoadSave.LoadSaveFolder(), ext);
            if (path != null && path != string.Empty)
            {
                LevelList ll = serializedObject.targetObject as LevelList;
                ll.AddFile(path);
            }
        }

        // --- add all the files in a specific folder
        if (GUILayout.Button(loadFolderButtonContent, EditorStyles.miniButtonRight, miniButtonWidthLarge))
        {
            string path = EditorUtility.OpenFolderPanel("Select levels folder", TableLoadSave.LoadSaveFolder(), "");
            if (path != null && path != string.Empty)
            {
                LevelList ll = serializedObject.targetObject as LevelList;
                string[] files = System.IO.Directory.GetFiles(path, "*.bytes");
                for (int i = 0; i < files.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Adding levels", "Processing level " + (i + 1) + " of " + files.Length, (float)i / (float)files.Length);
                    if (!files[i].EndsWith(".replay.bytes"))
                        ll.AddFile(files[i]);
                }
                EditorUtility.ClearProgressBar();
            }
        }

        // --- export everything to a certain folder
        if (GUILayout.Button(exportButtonContent, EditorStyles.miniButton, miniButtonWidthLarge))
        {
            string path = EditorUtility.OpenFolderPanel("Select levels folder", TableLoadSave.LoadSaveFolder(), "");
            if (path != null && path != string.Empty)
            {
                LevelList ll = serializedObject.targetObject as LevelList;
                for (int i = 0; i < ll.Count; i++)
                {
                    LevelLink link = ll.levels[i];
                    string srcPath = System.IO.Path.Combine(TableLoadSave.LoadSaveFolder(), link.path + ".bytes");
                    string destPath = System.IO.Path.Combine(path, string.Format("Level{0:D2}.bytes", i + 1));
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
                LevelList ll = serializedObject.targetObject as LevelList;
                if (ll.levels != null && ll.levels.Count > 0)
                    for (int i = 0; i < ll.levels.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("Reloading levels", "Processing level " + (i + 1) + " of " + ll.levels.Count, (float)i / (float)ll.levels.Count);
                        ll.UpdateLevelData(i);
                        LevelLink link = ll.levels[i];
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

        EditorGUILayout.EndHorizontal();
    }

    private void ShowMoveSelectButtons()
    {
        EditorGUILayout.BeginHorizontal();

        // --- move selection up
        if (GUILayout.Button(moveUpButtonContent, EditorStyles.miniButtonLeft, miniButtonWidthSmall))
        {
            SerializedProperty list = serializedObject.FindProperty("levels");
            for (int i = 1; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).FindPropertyRelative("selected").boolValue == true)
                {
                    if (list.GetArrayElementAtIndex(i - 1).FindPropertyRelative("selected").boolValue == false)
                        list.MoveArrayElement(i, i - 1);
                }
            }
        }
        // --- move selection down
        if (GUILayout.Button(moveDnButtonContent, EditorStyles.miniButtonRight, miniButtonWidthSmall))
        {
            SerializedProperty list = serializedObject.FindProperty("levels");
            for (int i = list.arraySize - 2; i >= 0; i--)
            {
                if (list.GetArrayElementAtIndex(i).FindPropertyRelative("selected").boolValue == true)
                {
                    if (list.GetArrayElementAtIndex(i + 1).FindPropertyRelative("selected").boolValue == false)
                        list.MoveArrayElement(i, i + 1);
                }
            }
        }

        // --- select all
        if (GUILayout.Button(selectAllButtonContent, EditorStyles.miniButtonLeft, miniButtonWidthMedium))
        {
            SerializedProperty list = serializedObject.FindProperty("levels");
            for (int i = 0; i < list.arraySize; i++)
                list.GetArrayElementAtIndex(i).FindPropertyRelative("selected").boolValue = true;
        }
        // --- select none
        if (GUILayout.Button(selectNoneButtonContent, EditorStyles.miniButtonRight, miniButtonWidthMedium))
        {
            SerializedProperty list = serializedObject.FindProperty("levels");
            for (int i = 0; i < list.arraySize; i++)
                list.GetArrayElementAtIndex(i).FindPropertyRelative("selected").boolValue = false;
        }

        EditorGUILayout.EndHorizontal();
    }

    protected GUIStyle GetStyle(int x, int y)
    {
        if (x % 2 == 0)
            return y % 2 == 0 ? listItemStyle1 : listItemStyle2;
        else
            return y % 2 == 0 ? listItemStyle2 : listItemStyle3;
    }

    protected void ShowList(SerializedProperty list)
    {
        if (!list.isArray)
        {
            EditorGUILayout.HelpBox(list.name + " is neither an array nor a list!", MessageType.Error);
            return;
        }

        ShowContentButtons();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(list);
        EditorGUILayout.LabelField("(" + list.arraySize + " elements)");
        EditorGUILayout.EndHorizontal();
        if (list.isExpanded && list.arraySize > 0)
        {
            float[] columns = { 24f, 18f, 60f, 40f, 40f, 40f, 40f };

            ShowMoveSelectButtons();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Idx", GUILayout.Width(6 + columns[0]));
            EditorGUILayout.LabelField("Sel", GUILayout.Width(columns[1]));
            EditorGUILayout.LabelField("Name", GUILayout.Width(columns[2]));
            EditorGUILayout.LabelField("Size", GUILayout.Width(columns[3]));
            EditorGUILayout.LabelField("Sol", GUILayout.Width(columns[4]));
            EditorGUILayout.LabelField("Moves", GUILayout.Width(columns[5]));
            EditorGUILayout.LabelField("Score", GUILayout.Width(columns[6]));
            EditorGUILayout.EndHorizontal();


            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox);
            for (int i = 0; i < list.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal(i % 2 == 0 ? listItemStyle1 : listItemStyle2);
                //EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(columns[0]));
                //item.FindPropertyRelative("selected").boolValue = EditorGUILayout.Toggle(item.FindPropertyRelative("selected").boolValue, GUILayout.Width(columns[1]));
                //EditorGUILayout.LabelField(item.FindPropertyRelative("filename").stringValue, GUILayout.Width(columns[2]));
                //EditorGUILayout.LabelField(item.FindPropertyRelative("size").stringValue, GUILayout.Width(columns[3]));
                //EditorGUILayout.LabelField(item.FindPropertyRelative("numSolutions").intValue.ToString(), GUILayout.Width(columns[4]));
                //EditorGUILayout.LabelField(item.FindPropertyRelative("minSteps").intValue.ToString(), GUILayout.Width(columns[5]));
                //EditorGUILayout.LabelField(item.FindPropertyRelative("score").floatValue.ToString("N1"), GUILayout.Width(columns[6]));
                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent("" + (i + 1)));

                //if (GUILayout.Button(new GUIContent("\u21bb", "open file in editor"), EditorStyles.miniButton, GUILayout.Width(24f)))
                //{
                //    string path = item.FindPropertyRelative("path").stringValue;
                //    if (LevelManager.lmWindow == null)
                //        LevelManager.Init();
                //    LevelManager.lmWindow.LoadLevelRelativePath(path);
                //}
                //if (GUILayout.Button(new GUIContent("rld", "refresh file"), EditorStyles.miniButton, GUILayout.Width(24f)))
                //{
                //    ll.UpdateLevelData(i);
                //    string path = item.FindPropertyRelative("path").stringValue;
                //}

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        if (list.arraySize == 0)
        {
            EditorGUILayout.HelpBox("list is empty", MessageType.None);
            return;
        }
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
}
