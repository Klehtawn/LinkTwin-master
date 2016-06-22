using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class LevelLink
{
    public string author;
    public string title;
    public float score;
    public string filename;
    public string path;
    public bool selected;
    public string size;
    public int numSolutions;
    public int minSteps;
    public string comment = string.Empty;
    public Texture2D thumbnail;
    //public byte unusedTiles = 0;
    //public byte fakeBoxes = 0;
    //public byte fakeMoveableBoxes = 0;
    //public byte fakePortals = 0;
    //public byte wrongStarts = 0;

    public LevelStats levelStats;
    public LevelSimulation.Solver levelSim;

    public void LoadFromCurrent(LevelRoot root)
    {
        TableDescription td = null;
        TableLoadSave.ConvertSceneToMapDescription(ref td);
        CalculateLevelStatistics(td, root);
    }

    public void LoadLevelInfo()
    {
        TableDescription td = null;

        LevelRoot root = LevelRoot.CreateRoot("LevelRootTemp");

        try
        {
            TableLoadSave.Load(path, ref td, root);

            TableLoadSave.ConvertMapDescriptionToScene(td, root);

            CalculateLevelStatistics(td, root);

            title = td.name;
            author = td.author;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            LevelRoot.DestroyRoot(ref root);
        }
    }

    public void LoadLevelThumbnail()
    {
        TableDescription td = null;
        LevelRoot root = LevelRoot.CreateRoot("LevelRootTemp");
        thumbnail = null;

        try
        {
            TableLoadSave.Load(path, ref td, root);
            thumbnail = TableLoadSave.MakeThumbnail(td, 128, false);
            TableLoadSave.Cleanup();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            LevelRoot.DestroyRoot(ref root);
        }
    }

    public void CalculateLevelStatistics(TableDescription td, LevelRoot root)
    {
        Debug.Assert(root != null);

        List<string> errors = new List<string>();
        levelStats = new LevelStats();

        //Blocks blocks = new Blocks();
        //blocks.Fill(root != null ? root.gameObject : null);
        //Vector3 size = root.blocks.GetGroundSize() / GameSession.gridUnit;

        //levelStats.width = (int)size.x;
        //levelStats.height = (int)size.z;

        List<Block> spawns = new List<Block>();
        List<Block> exits = new List<Block>();

        levelSim = new LevelSimulation.Solver();
        levelSim.Init(root);

        //foreach (Block b in td.Table.GetComponentsInChildren<Block>())
        foreach (Block b in root.blocks.items)
        {
            switch (b.blockType)
            {
                case Block.BlockType.Default:
                    levelStats.boxes++;
                    break;
                case Block.BlockType.Portal:
                    levelStats.portals++;
                    break;
                case Block.BlockType.Piston:
                    levelStats.pistons++;
                    break;
                case Block.BlockType.Spawn:
                    spawns.Add(b);
                    levelStats.clones++;
                    break;
                case Block.BlockType.Finish:
                    exits.Add(b);
                    levelStats.exits++;
                    break;
                case Block.BlockType.Wall:
                    levelStats.walls++;
                    break;
                case Block.BlockType.MovableBox:
                    levelStats.moveableBoxes++;
                    break;
                case Block.BlockType.Button:
                    levelStats.buttons++;
                    break;
                case Block.BlockType.PushButton:
                    levelStats.pushbuttons++;
                    break;
                case Block.BlockType.Door:
                    levelStats.doors++;
                    break;
                case Block.BlockType.Crumbling:
                    levelStats.crumbling++;
                    break;
                case Block.BlockType.Tutorial:
                    // nothing here, just don't whine about it
                    break;
                default:
                    errors.Add("uncounted block type: " + b.blockType + ", bug Stef about it");
                    Debug.LogWarning("uncounted block type: " + b.blockType);
                    break;
            }
        }

        if (spawns.Count > 1)
        {
            Vector3 d = spawns[0].transform.position - spawns[1].transform.position;
            float dx = Mathf.Abs(d.x / GameSession.gridUnit);
            float dy = Mathf.Abs(d.z / GameSession.gridUnit);
            levelStats.cloneDistance = (int)(dx + dy);
        }

        if (exits.Count > 1)
        {
            Vector3 d = exits[0].transform.position - exits[1].transform.position;
            float dx = Mathf.Abs(d.x / GameSession.gridUnit);
            float dy = Mathf.Abs(d.z / GameSession.gridUnit);
            levelStats.exitDistance = (int)(dx + dy);
        }

        //score =
        //    1.0f * levelStats.boxes +
        //    1.0f * levelStats.buttons +
        //    1.0f * levelStats.pushbuttons +
        //    0.3f * levelStats.doors +
        //    1.5f * levelStats.moveableBoxes +
        //    0.8f * levelStats.movingPlatforms +
        //    2.0f * levelStats.portals +
        //    1.3f * levelStats.pistons +
        //    0.3f * levelStats.cloneDistance +
        //    0.3f * levelStats.exitDistance +
        //    // manual values
        //    0.3f * levelStats.minSteps;
        //0.5f * levelStats.unusedTiles +
        //2.0f * levelStats.fakeBoxes +
        //1.5f * levelStats.fakeMoveableBoxes +
        //3.0f * levelStats.fakePortals +
        //1.0f * levelStats.wrongStarts;

        if (levelStats.exits != levelStats.clones)
        {
            errors.Add(string.Format("mismatched number of spawners/exits ({0}/{1})", levelStats.clones, levelStats.exits));
        }

        if (errors.Count > 0)
        {
            levelStats.error = "Errors detected:";
            foreach (string error in errors)
                levelStats.error += "\n - " + error;
        }
    }

    public void RunLevelSimulation(float maxtime, int maxsolutions)
    {
        levelSim.RunSolver(maxtime, maxsolutions);
        this.size = levelSim.GetBoardSizeString();
        levelStats.width = (int)levelSim.GetBoardSize().x;
        levelStats.height = (int)levelSim.GetBoardSize().y;

        levelStats.solutions = levelSim.Solutions;
        levelStats.numSolutions = levelSim.Solutions.Count;
        if (levelSim.Solutions.Count > 0)
        {
            levelStats.minSteps = (byte)levelSim.Solutions[0].NumSteps;
            minSteps = levelStats.minSteps;
        }
        else
        {
            levelStats.minSteps = 0;
            minSteps = 0;
        }

        numSolutions = levelStats.numSolutions;

        if (levelSim.Solutions == null || levelSim.Solutions.Count == 0)
        {
            score = -1;
        }
        else
        {
            score = levelSim.GetFirstSolutionScore();
        }
    }

    public bool SetPath(string fullpath)
    {
        filename = Path.GetFileNameWithoutExtension(fullpath);

        string pathnoext = Path.Combine(Path.GetDirectoryName(fullpath), filename).Replace('\\', '/');

        string rootPath = Path.Combine(Application.dataPath, "Levels").Replace('\\', '/');

        if (!pathnoext.StartsWith(rootPath))
        {
            //Debug.Log("path doesn't begin with root asset folder, assuming relative: " + pathnoext);
            path = fullpath;
            if (!File.Exists(Path.Combine(rootPath, path + ".bytes")))
                return false;
        }
        else
        {
            path = pathnoext.Substring(rootPath.Length + 1);
            if (!File.Exists(pathnoext + ".bytes"))
                return false;
        }

        return true;
    }
}
