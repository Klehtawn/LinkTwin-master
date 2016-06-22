using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomPropertyDrawer(typeof(LevelLink))]
public class LevelLinkDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //return label != GUIContent.none && Screen.width < 333 ? (16f + 18f) : 16f;
        return 16f;
    }

    //static float[] col = { 0.05f, 0.2f, 0.35f, 0.1f };
    public static float[] columns = { 24f, 18f, 60f, 40f, 40f, 40f, 40f };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //label = EditorGUI.BeginProperty(position, label, property);
        //EditorGUIUtility.labelWidth = 34f;
        //contentPosition.x = position.x;
        if (GUILayout.Button(new GUIContent("\u21bb", "open file in editor"), GUILayout.Width(24f)))
        {
            string path = property.FindPropertyRelative("path").stringValue;
            LevelManager.Init();
            LevelManager.lmWindow.LoadLevelRelativePath(path);
        }

        //EditorGUI.EndProperty();

        //base.OnGUI(position, property, label);
        int oldIndentLevel = EditorGUI.indentLevel;
        label = EditorGUI.BeginProperty(position, label, property);
        EditorGUIUtility.labelWidth = 34f;
        Rect contentPosition = EditorGUI.PrefixLabel(position, label);
        //if (position.height > 16f)
        //{
        //    position.height = 16f;
        //    EditorGUI.indentLevel += 1;
        //    contentPosition = EditorGUI.IndentedRect(position);
        //    contentPosition.y += 18f;
        //}
        float w = 1.0f;// contentPosition.width;
        contentPosition.width = w * columns[1];
        EditorGUI.indentLevel = 0;

        EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("selected"), GUIContent.none);
        contentPosition.x += contentPosition.width;
        contentPosition.width = w * columns[2];

        EditorGUI.LabelField(contentPosition, property.FindPropertyRelative("filename").stringValue);
        contentPosition.x += contentPosition.width;
        contentPosition.width = w * columns[3];

        EditorGUI.LabelField(contentPosition, property.FindPropertyRelative("size").stringValue);
        contentPosition.x += contentPosition.width;
        contentPosition.width = w * columns[4];

        EditorGUI.LabelField(contentPosition, property.FindPropertyRelative("numSolutions").intValue.ToString());
        contentPosition.x += contentPosition.width;
        contentPosition.width = w * columns[5];

        EditorGUI.LabelField(contentPosition, property.FindPropertyRelative("minSteps").intValue.ToString());
        contentPosition.x += contentPosition.width;
        contentPosition.width = w * columns[6];

        EditorGUI.LabelField(contentPosition, property.FindPropertyRelative("score").floatValue.ToString("N1"));

        EditorGUI.EndProperty();
        EditorGUI.indentLevel = oldIndentLevel;

        //if (GUILayout.Button(new GUIContent(property.FindPropertyRelative("filename").stringValue, "browse for some file"), GUILayout.Width(100f)))
        //if (GUILayout.Button(new GUIContent("...", "browse for some file"), GUILayout.Width(24f)))
        //{
        //    string[] ext = new string[2] { "LinkTwin tables", "bytes" };
        //    string path = EditorUtility.OpenFilePanelWithFilters("Open table", TableLoadSave.LoadSaveFolder(), ext);
        //    if (path != null && path != string.Empty)
        //    {
        //        property.FindPropertyRelative("filename").stringValue = System.IO.Path.GetFileNameWithoutExtension(path);
        //        //property.FindPropertyRelative("title").stringValue = "";
        //        LevelList ll = property.serializedObject.targetObject as LevelList;
        //        ll.UpdateLevelData();
        //    }
        //}
    }
}
