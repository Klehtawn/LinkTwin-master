using UnityEngine;
using System.Collections;

public class WorkshopDevButtons : Widget {

    public Widget openFileButton;
    public Widget saveFileButton;
    public Widget newFileButton;

    WorkshopScreen workshop;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();

        openFileButton.OnTouchUp += OnOpenFilePressed;
        saveFileButton.OnTouchUp += OnSaveFilePressed;
        newFileButton.OnTouchUp += OnNewFilePressed;

#if UNITY_EDITOR
        gameObject.SetActive(true);
#else
        gameObject.SetActive(false);
#endif

        workshop = GetComponentInParent<WorkshopScreen>();
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();

        newFileButton.active = workshop.levelSource != WorkshopScreen.LevelSource.FromPlaying;
        openFileButton.active = newFileButton.active;
	}

    void OnOpenFilePressed(MonoBehaviour sender, Vector2 p)
    {
#if UNITY_EDITOR
        string[] ext = new string[2] { "LinkTwin tables", "bytes" };
        string path = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Open table", TableLoadSave.LoadSaveFolder(), ext);
        if (path != null && path != string.Empty)
        {
            workshop.LoadLevelFromPath(path);
        }
#endif
    }

    void OnSaveFilePressed(MonoBehaviour sender, Vector2 p)
    {
#if UNITY_EDITOR

        string path = null;
        if (workshop.levelSource == WorkshopScreen.LevelSource.FromPlaying && GameSession.customLevelBytes == null)
            path = workshop.LoadedLevelPath;
        else
            path = UnityEditor.EditorUtility.SaveFilePanel("Save table ...", TableLoadSave.LoadSaveFolder(), "UntitledTable", "bytes");

        path = TableLoadSave.AppendExtension(path);

        if (path.Length > 0)
        {
            TableDescription td = null;
            Transform table = workshop.GetGameTable();
            TableLoadSave.ConvertSceneToMapDescription(ref td, table);
            TableLoadSave.SaveAbsolutePath(path, td);
        }
#endif
    }

    void OnNewFilePressed(MonoBehaviour sender, Vector2 p)
    {
        workshop.ResetTable();
    }
}
