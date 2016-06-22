using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct InputEvent
{
    public float time;
    public TheInput.InputData data;

    public InputEvent(float t, TheInput.InputData d)
    {
        time = t;
        data = d;
    }
}

public class InputRecorder
{
    private static int VERSION = 1;

    public Queue<InputEvent> replayQueue;

    public InputRecorder()
    {
        replayQueue = new Queue<InputEvent>();
    }

    public bool BetterThan(InputRecorder other)
    {
        //if (other.replayQueue.Count == 0)
        //    return true;

        //return true;
        return replayQueue.Count < other.replayQueue.Count;
    }

    public static string GetHintPathAbsolute(string levelPath)
    {
        string filename = Path.GetFileNameWithoutExtension(levelPath) + ".replay.bytes";
        return Path.Combine(Path.GetDirectoryName(levelPath), filename);
    }

    private static string GetHintPath(string levelPath)
    {
        string fullPath = TableLoadSave.LoadSaveFolder() + levelPath;
        string filename = Path.GetFileNameWithoutExtension(fullPath) + ".replay.bytes";
        return Path.Combine(Path.GetDirectoryName(fullPath), filename);
    }

    public void Save(string levelPath)
    {
#if UNITY_EDITOR
        if (levelPath == null || levelPath.Length == 0)
            return;

        string path = GetHintPath(levelPath);

        FileStream fs = new FileStream(path, FileMode.Create);

        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(VERSION);
        bw.Write(replayQueue.Count);

        while (replayQueue.Count > 0)
        {
            InputEvent ev = replayQueue.Dequeue();
            bw.Write(ev.time);
            bw.Write((int)ev.data.type);
            bw.Write(ev.data.data.x);
            bw.Write(ev.data.data.y);
            bw.Write(ev.data.data.z);
        }

        bw.Close();
#endif
    }

    public bool Load(string levelPath)
    {
        if (levelPath == null || levelPath.Length == 0)
            return false;

        string path = GetHintPath(levelPath);

        if (!File.Exists(path))
        {
            Debug.Log("Can't open replay file: " + path);
            return false;
        }

        FileStream fs = new FileStream(path, FileMode.Open);

        if (fs == null)
            return false;

        BinaryReader br = new BinaryReader(fs);

        if (br.ReadInt32() != VERSION)
            return false;

        replayQueue.Clear();
        int count = br.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            float time = br.ReadSingle();
            int type = br.ReadInt32();
            Vector3 dir = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            TheInput.InputData data = new TheInput.InputData();
            data.type = (TheInput.InputData.InputDataType)type;
            data.data = dir;
            replayQueue.Enqueue(new InputEvent(time, data));
        }

        fs.Close();

        return true;
    }
}
