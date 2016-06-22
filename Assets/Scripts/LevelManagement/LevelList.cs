using UnityEngine;
using System.Collections.Generic;

public class LevelList : MonoBehaviour
{
    public List<LevelLink> levels = new List<LevelLink>();

    public int Count
    {
        get { return levels.Count; }
    }

    public void UpdateLevelData(int index)
    {
        levels[index].LoadLevelInfo();
        levels[index].RunLevelSimulation(5, 20);
    }

    public void Clear()
    {
        levels = new List<LevelLink>();
    }

    public void AddFile(string path)
    {
        LevelLink link = new LevelLink();
        link.SetPath(path.Replace('\\', '/'));
        levels.Add(link);

        UpdateLevelData(levels.IndexOf(link));
    }

    public void LoadFolder(string path)
    {
#if UNITY_EDITOR
        string[] files = System.IO.Directory.GetFiles(path, "*.bytes");
        foreach (string file in files)
            AddFile(file);
#endif
    }
}
