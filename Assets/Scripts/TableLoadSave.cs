using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class TableLoadSave
{
    static public string LoadSaveFolder()
    {
#if UNITY_EDITOR
        return Application.dataPath + "/Levels/";
#else
        return "";
#endif
    }

    static public string DefaultExtesion()
    {
#if UNITY_EDITOR
        return ".bytes";
#else
        return "";
#endif
    }

    //static public void Save(string filename, TableDescription tdesc)
    //{
    //    Directory.CreateDirectory(LoadSaveFolder());
    //    string path = LoadSaveFolder() + filename;
    //    SaveAbsolutePath(path, tdesc);
    //}

    static public void SaveAbsolutePath(string filename, TableDescription tdesc, bool shrink = false)
    {
        FileStream fs = new FileStream(filename, FileMode.Create);
        SaveToStream(fs, tdesc, shrink);
        int fsz = (int)fs.Length;
        fs.Close();
        //Debug.Log("Table saved to [" + filename + "] (" + fsz + " bytes).");
        string hintPath = InputRecorder.GetHintPathAbsolute(filename);
        if (File.Exists(hintPath))
        {
            File.Delete(hintPath);
            //Debug.Log("Found old hint file, deleted");
        }
    }

    static public byte[] SaveToMemory(TableDescription tdesc)
    {
        MemoryStream ms = new MemoryStream();
        SaveToStream(ms, tdesc);

#if !UNITY_WSA
        ms.Close();
#endif

        return ms.ToArray();
    }

    public Vector2 GetBlockPosition(Block b)
    {
        Vector3 pos = Vector3.one * 0.5f + b.transform.position / (GameSession.gridUnit);
        pos.y = pos.z;
        return pos;
    }

    static void SaveToStream(Stream fs, TableDescription tdesc, bool shrink = false)
    {
        BinaryWriter bw = new BinaryWriter(fs);

        bw.Write(tdesc.descriptor);

        bw.Write((ulong)0);

        bw.Write(TableDescription.CURRENT_VERSION);
        //Debug.Log("Table version is " + tdesc.version.ToString() + "\n");
        //bw.Write(tdesc.uniqueIdentifier);

        bw.Write(tdesc.index);
        bw.Write(tdesc.difficulty);
        bw.Write(tdesc.stage);
        if (shrink)
        {
            bw.Write("");
            bw.Write("");
        }
        else
        {
            bw.Write(tdesc.name);
            bw.Write(tdesc.author);
        }

        // designer manual values
        //bw.Write((byte)0);
        //bw.Write((byte)0);
        //bw.Write((byte)0);
        //bw.Write((byte)0);
        //bw.Write((byte)0);
        //bw.Write((byte)0);

        // first the ground
        bw.Write(tdesc.groundWidth);
        bw.Write(tdesc.groundHeight);
        bw.Write(tdesc.groundCenterX);
        bw.Write(tdesc.groundCenterZ);

        // empty blocks
        bw.Write(tdesc.emptyGroundBlocksCount);

        if (tdesc.emptyGroundBlocksCount > 0 && tdesc.emptyGroundBlocksXZ.Length > 0)
            bw.Write(tdesc.emptyGroundBlocksXZ);

        // other blocks
        bw.Write(tdesc.numBlocks);

        for (int i = 0; i < tdesc.blocks.Length; i++)
        {
            Block b = tdesc.blocks[i];
            string msg = "Block #" + (i + 1).ToString() + ": ";
            // block id
            byte bt = (byte)b.blockType; 
            bw.Write(bt);

#if UNITY_EDITOR
            if (shrink)
            {
                b.sourcePrefab = "";
            }
            else
            {
                UnityEngine.Object src = UnityEditor.PrefabUtility.GetPrefabParent(b.gameObject);
                if (src != null)
                    b.sourcePrefab = src.name;
            }
#endif

            msg += "(type:" + b.blockType.ToString() + ", prefab:" + b.sourcePrefab + ") ";

            bw.Write(b.sourcePrefab);

            BlockModifier[] modifiers = b.GetComponents<BlockModifier>();
            bw.Write((byte)modifiers.Length);
            foreach (BlockModifier bm in modifiers)
            {
                if (shrink)
                    bm.name = bm.name.Substring(0, 1);
                string typeStr = bm.GetType().ToString();
                bw.Write(typeStr);
                bm.PrepareWritingSignificantData(ref bw);
                bm.WriteSignificantInfo(ref bw);
                bm.FinishWritingSignificantData(ref bw);

                msg += bm.GetType().ToString() + ", ";
            }

            msg += "\n";
            //Debug.Log(msg);
        }

        // solution
        bw.Write(tdesc.numSolutionSteps);
        for (int i = 0; i < tdesc.numSolutionSteps; i++)
        {
            bw.Write(tdesc.solutionSteps[i]);
        }

        // compute crc and write it
    }

    static public void Load(string filename, ref TableDescription tdesc, LevelRoot root)
    {
        string path = LoadSaveFolder() + filename;
        LoadAbsolutePath(path, ref tdesc, root);
    }

    static public void LoadFromMemory(byte[] bytes, ref TableDescription tdesc, LevelRoot root)
    {
        Stream stream = new MemoryStream(bytes);
        BinaryReader br = new BinaryReader(stream);

        if (ReadSignature(ref br) == false)
        {
            tdesc = null;
            Debug.LogError("Invalid map signature");
            return;
        }

        tdesc = new TableDescription();

        tdesc.crc = br.ReadUInt64();

        tdesc.version = br.ReadUInt16();

        if (tdesc.version < 3)
            tdesc.uniqueIdentifier = br.ReadChars(32);

        if (tdesc.version == 1)
            LoadVersion1(br, ref tdesc);
        else
        if (tdesc.version == 2)
            LoadVersion2(br, ref tdesc, root);
        else
        if (tdesc.version == 3)
            LoadVersion3(br, ref tdesc, root);
        else
        if (tdesc.version == 4)
            LoadVersion4(br, ref tdesc, root);
        // put other versions here
        else
        {
            tdesc = null;
            Debug.LogError("Invalid map (invalid version)");
            return;
        }
    }

    static public void LoadAbsolutePath(string filename, ref TableDescription tdesc, LevelRoot root)
    {
        //Debug.Log("Opening table from [" + filename + "] ...\n");

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            FileStream fs = new FileStream(filename + DefaultExtesion(), FileMode.Open);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            LoadFromMemory(bytes, ref tdesc, root);
            fs.Close();
        }
        else
#endif
        {
            //TextAsset asset = Resources.Load(filename) as TextAsset;
            //byte[] bytes = asset.bytes;
            byte[] bytes = ContentManager.LoadBinaryFile(filename);
            LoadFromMemory(bytes, ref tdesc, root);
        }

        // check crc
        //Debug.Log("Table loaded.\n");
    }

    static void LoadVersion1(BinaryReader br, ref TableDescription tdesc)
    {
        tdesc.index = br.ReadUInt16();
        tdesc.difficulty = br.ReadByte();
        tdesc.stage = br.ReadByte();
        tdesc.name = br.ReadString();
        tdesc.author = br.ReadString();

        // ground
        tdesc.groundWidth = br.ReadByte();
        tdesc.groundHeight = br.ReadByte();
        tdesc.groundCenterX = br.ReadSingle();
        tdesc.groundCenterZ = br.ReadSingle();

        // empty blocks
        tdesc.emptyGroundBlocksCount = br.ReadUInt16();
        tdesc.emptyGroundBlocksXZ = br.ReadBytes(tdesc.emptyGroundBlocksCount * 2);

        // other blocks
        tdesc.numBlocks = br.ReadUInt16();
        tdesc.blocks = new Block[tdesc.numBlocks];

        ResetPrefabsList();

        for (int i = 0; i < tdesc.numBlocks; i++)
        {
            Block.BlockType bt = (Block.BlockType)br.ReadByte();
            GameObject obj = InstantiatePrefabByType(bt);
            if (obj == null) continue;

            tdesc.blocks[i] = obj.GetComponent<Block>();
                    
            int numModifiers = (int)br.ReadByte();
            for (int k = 0; k < numModifiers; k++)
            {
                string modif = br.ReadString();
                BlockModifier bm = obj.GetComponent(modif) as BlockModifier;
                if (bm != null)
                {
                    bm.PrepareReadingSignificantInfo(ref br);
                    bm.ReadSignificantInfo(ref br);
                    bm.FinishReadingSignificantInfo(ref br);
                }
            }

            if(obj.name.Length < 1)
                obj.name = bt.ToString();
        }

        for (int i = 0; i < tdesc.numBlocks; i++)
        {
            BlockModifier[] bms = tdesc.blocks[i].GetComponents<BlockModifier>();
            foreach (BlockModifier bm in bms)
                bm.LinkSignificantValues(tdesc.blocks);
        }
    }

    static void LoadVersion2(BinaryReader br, ref TableDescription tdesc, LevelRoot root)
    {
        tdesc.index = br.ReadUInt16();
        tdesc.difficulty = br.ReadByte();
        tdesc.stage = br.ReadByte();
        tdesc.name = br.ReadString();
        tdesc.author = br.ReadString();

        // ground
        tdesc.groundWidth = br.ReadByte();
        tdesc.groundHeight = br.ReadByte();
        tdesc.groundCenterX = br.ReadSingle();
        tdesc.groundCenterZ = br.ReadSingle();

        // empty blocks
        tdesc.emptyGroundBlocksCount = br.ReadUInt16();
        tdesc.emptyGroundBlocksXZ = br.ReadBytes(tdesc.emptyGroundBlocksCount * 2);

        // other blocks
        tdesc.numBlocks = br.ReadUInt16();
        tdesc.blocks = new Block[tdesc.numBlocks];

        ResetPrefabsList();

        for (int i = 0; i < tdesc.numBlocks; i++)
        {
            Block.BlockType bt = (Block.BlockType)br.ReadByte();
            string prefabName = br.ReadString();

            GameObject obj = null;

            if (prefabName != null && prefabName.Length > 0)
                obj = InstantiatePrefabByName(prefabName);
            else
                obj = InstantiatePrefabByType(bt);
            if (obj == null) continue;

            obj.transform.parent = root.Table.transform;

            tdesc.blocks[i] = obj.GetComponent<Block>();
            tdesc.blocks[i].sourcePrefab = prefabName;

            int numModifiers = (int)br.ReadByte();
            for (int k = 0; k < numModifiers; k++)
            {
                string modif = br.ReadString();
                BlockModifier bm = obj.GetComponent(modif) as BlockModifier;
                if (bm != null)
                {
                    bm.PrepareReadingSignificantInfo(ref br);
                    bm.ReadSignificantInfo(ref br);
                    bm.FinishReadingSignificantInfo(ref br);
                }
            }

            if (obj.name.Length < 1)
                obj.name = bt.ToString();

            if (bt == Block.BlockType.Trap)
                obj.SetActive(false);
        }

        for (int i = 0; i < tdesc.numBlocks; i++)
        {
            BlockModifier[] bms = tdesc.blocks[i].GetComponents<BlockModifier>();
            foreach (BlockModifier bm in bms)
                bm.LinkSignificantValues(tdesc.blocks);
        }
    }

    static void LoadVersion3(BinaryReader br, ref TableDescription tdesc, LevelRoot root)
    {
        tdesc.index = br.ReadUInt16();
        tdesc.difficulty = br.ReadByte();
        tdesc.stage = br.ReadByte();
        tdesc.name = br.ReadString();
        tdesc.author = br.ReadString();

        // designer manual values
        br.ReadByte();
        br.ReadByte();
        br.ReadByte();
        br.ReadByte();
        br.ReadByte();
        br.ReadByte();

        // ground
        tdesc.groundWidth = br.ReadByte();
        tdesc.groundHeight = br.ReadByte();
        tdesc.groundCenterX = br.ReadSingle();
        tdesc.groundCenterZ = br.ReadSingle();

        // empty blocks
        tdesc.emptyGroundBlocksCount = br.ReadUInt16();
        tdesc.emptyGroundBlocksXZ = br.ReadBytes(tdesc.emptyGroundBlocksCount * 2);

        // other blocks
        tdesc.numBlocks = br.ReadUInt16();
        tdesc.blocks = new Block[tdesc.numBlocks];

        ResetPrefabsList();

        for (int i = 0; i < tdesc.numBlocks; i++)
        {
            Block.BlockType bt = (Block.BlockType)br.ReadByte();
            string prefabName = br.ReadString();

            GameObject obj = null;

            if (prefabName != null && prefabName.Length > 0)
                obj = InstantiatePrefabByName(prefabName);
            else
                obj = InstantiatePrefabByType(bt);
            if (obj == null) continue;

            obj.transform.parent = root.Table.transform;

            tdesc.blocks[i] = obj.GetComponent<Block>();
            tdesc.blocks[i].sourcePrefab = prefabName;

            int numModifiers = (int)br.ReadByte();
            for (int k = 0; k < numModifiers; k++)
            {
                string modif = br.ReadString();
                BlockModifier bm = obj.GetComponent(modif) as BlockModifier;
                if (bm != null)
                {
                    bm.PrepareReadingSignificantInfo(ref br);
                    bm.ReadSignificantInfo(ref br);
                    bm.FinishReadingSignificantInfo(ref br);
                }
            }

            if (obj.name.Length < 1)
                obj.name = bt.ToString();
        }

        for (int i = 0; i < tdesc.numBlocks; i++)
        {
            BlockModifier[] bms = tdesc.blocks[i].GetComponents<BlockModifier>();
            foreach (BlockModifier bm in bms)
                bm.LinkSignificantValues(tdesc.blocks);
        }
    }

    static void LoadVersion4(BinaryReader br, ref TableDescription tdesc, LevelRoot root)
    {
        tdesc.index = br.ReadUInt16();
        tdesc.difficulty = br.ReadByte();
        tdesc.stage = br.ReadByte();
        tdesc.name = br.ReadString();
        tdesc.author = br.ReadString();

        // designer manual values
        //br.ReadByte();
        //br.ReadByte();
        //br.ReadByte();
        //br.ReadByte();
        //br.ReadByte();
        //br.ReadByte();

        // ground
        tdesc.groundWidth = br.ReadByte();
        tdesc.groundHeight = br.ReadByte();
        tdesc.groundCenterX = br.ReadSingle();
        tdesc.groundCenterZ = br.ReadSingle();

        // empty blocks
        tdesc.emptyGroundBlocksCount = br.ReadUInt16();
        tdesc.emptyGroundBlocksXZ = br.ReadBytes(tdesc.emptyGroundBlocksCount * 2);

        // other blocks
        tdesc.numBlocks = br.ReadUInt16();
        tdesc.blocks = new Block[tdesc.numBlocks];

        ResetPrefabsList();

        for (int i = 0; i < tdesc.numBlocks; i++)
        {
            Block.BlockType bt = (Block.BlockType)br.ReadByte();
            string prefabName = br.ReadString();

            GameObject obj = null;

            if (prefabName != null && prefabName.Length > 0)
                obj = InstantiatePrefabByName(prefabName);
            else
                obj = InstantiatePrefabByType(bt);
            if (obj == null) continue;

            obj.transform.parent = root.Table.transform;

            tdesc.blocks[i] = obj.GetComponent<Block>();
            tdesc.blocks[i].sourcePrefab = prefabName;

            int numModifiers = (int)br.ReadByte();
            for (int k = 0; k < numModifiers; k++)
            {
                string modif = br.ReadString();
                BlockModifier bm = obj.GetComponent(modif) as BlockModifier;
                if (bm != null)
                {
                    bm.PrepareReadingSignificantInfo(ref br);
                    bm.ReadSignificantInfo(ref br);
                    bm.FinishReadingSignificantInfo(ref br);
                }
            }

            if (obj.name.Length < 1)
                obj.name = bt.ToString();
        }

        for (int i = 0; i < tdesc.numBlocks; i++)
        {
            BlockModifier[] bms = tdesc.blocks[i].GetComponents<BlockModifier>();
            foreach (BlockModifier bm in bms)
                bm.LinkSignificantValues(tdesc.blocks);
        }

        // solution
        tdesc.numSolutionSteps = br.ReadUInt16();
        if (tdesc.numSolutionSteps > 0)
        {
            tdesc.solutionSteps = new byte[tdesc.numSolutionSteps];
            for (int i = 0; i < tdesc.numSolutionSteps; i++)
                tdesc.solutionSteps[i] = br.ReadByte();
        }
    }

    static public void ConvertSceneToMapDescription(ref TableDescription tdesc, Transform root = null)
    {
        tdesc = new TableDescription();
        tdesc.version = TableDescription.CURRENT_VERSION;

        Blocks blocks = new Blocks();
        blocks.Fill(root);

        Vector3 sz = blocks.GetGroundSize();
        Vector3 center = blocks.GetGroundCenter();

        tdesc.groundWidth = (byte)(sz.x / GameSession.gridUnit);
        tdesc.groundHeight = (byte)(sz.z / GameSession.gridUnit);
        tdesc.groundCenterX = center.x;
        tdesc.groundCenterZ = center.z;

        List<byte> emptyBlocks = new List<byte>();

        Vector3 min = blocks.GetGroundMin();

        for(byte z = 0; z < tdesc.groundHeight; z++)
        {
            for(byte x = 0; x < tdesc.groundWidth; x++)
            {
                Vector3 pos = new Vector3((float)x, 0.0f, (float)z) * GameSession.gridUnit + min;
                pos.y = 0.0f;

                if (blocks.GroundAt(pos) == null) // empty
                {
                    emptyBlocks.Add(x);
                    emptyBlocks.Add(z);
                }
            }
        }

        //Debug.Log("Ground is " + tdesc.groundWidth + "x" + tdesc.groundHeight + "with " + (emptyBlocks.Count / 2).ToString() + " empty blocks.\n");

        tdesc.emptyGroundBlocksCount = (ushort)(emptyBlocks.Count / 2);
        if(tdesc.emptyGroundBlocksCount > 0)
        {
            tdesc.emptyGroundBlocksXZ = new byte[tdesc.emptyGroundBlocksCount * 2];
            for (int i = 0; i < tdesc.emptyGroundBlocksXZ.Length; i++)
                tdesc.emptyGroundBlocksXZ[i] = emptyBlocks[i];
        }

        tdesc.numBlocks = (ushort)blocks.items.Count;
        tdesc.blocks = new Block[tdesc.numBlocks];
        for (int i = 0; i < tdesc.blocks.Length; i++)
        {
            tdesc.blocks[i] = blocks.items[i];
            tdesc.blocks[i].blockIdentifier = (byte)(i + 1);
        }

        //Debug.Log(tdesc.numBlocks.ToString() + " blocks added.\n");

        //Debug.Log("Scene converted.\n");
    }

    static public Transform ConvertMapDescriptionToScene(TableDescription tdesc, LevelRoot root)
    {
        UnityEngine.Random.seed = 1234;

        //Debug.Log("Converting table to scene ...\n");

        GridCreator gc = root.Ground;

        //Debug.Log("Creating ground " + tdesc.groundWidth + "x" + tdesc.groundHeight + "\n");

        gc.CreateBlocks(tdesc.groundWidth, tdesc.groundHeight, new Vector3(tdesc.groundCenterX, 0.0f, tdesc.groundCenterZ));
        //gc.transform.position = new Vector3(tdesc.groundCenterX, 0.0f, tdesc.groundCenterZ);

        List<GameObject> toDelete = new List<GameObject>();
        for(int i = 0; i < tdesc.emptyGroundBlocksCount; i++)
        {
            int index = tdesc.emptyGroundBlocksXZ[2 * i] + tdesc.emptyGroundBlocksXZ[2 * i + 1] * tdesc.groundWidth;
            GameObject obj = gc.transform.GetChild(index).gameObject;

            toDelete.Add(obj);
        }

        foreach(GameObject obj in toDelete)
        {
            obj.transform.parent = null;
            if (Application.isPlaying)
                GameObject.Destroy(obj);
            else
                GameObject.DestroyImmediate(obj);
        }
        //Debug.Log("Deleted " + toDelete.Count + " ground blocks.\n");

        for (int i = 0; i < tdesc.numBlocks; i++ )
        {
            GameObject obj = tdesc.blocks[i].gameObject;
            obj.transform.SetParent(root.Table.transform);
        }

        for (int i = 0; i < tdesc.numBlocks; i++)
        {
            BlockModifier[] bms = tdesc.blocks[i].GetComponents<BlockModifier>();
            foreach (BlockModifier bm in bms)
                bm.LinkSignificantValues(tdesc.blocks);
        }

        //Debug.Log(tdesc.numBlocks.ToString() + " blocks added.\n");

        //Debug.Log("Converting done.\n");

        return root.Table.transform;
    }

    static bool ReadSignature(ref BinaryReader br)
    {
        char[] desc = br.ReadChars(8);
        return desc[0] == 'L' && desc[1] == 'I' && desc[2] == 'N' && desc[3] == 'K' &&
            desc[4] == 'T' && desc[5] == 'W' && desc[6] == 'I' && desc[7] == 'N';
    }

    static GameObject[] prefabsList;

    static void ResetPrefabsList()
    {
        prefabsList = null;
    }

    static GameObject _InstantiatePrefab(GameObject src)
    {
/*#if UNITY_EDITOR
        return UnityEditor.PrefabUtility.InstantiatePrefab(src) as GameObject;
#else*/
        return GameObject.Instantiate<GameObject>(src);
//#endif
    }

    public static GameObject InstantiatePrefabByType(Block.BlockType t)
    {
        if(prefabsList == null)
            prefabsList = Resources.LoadAll<GameObject>("Blocks");

        foreach(GameObject obj in prefabsList)
        {
            Block b = obj.GetComponent<Block>();
            if(b && b.blockType == t)
            {
                return _InstantiatePrefab(obj);
            }
        }

        Debug.LogError("Cannot instantiate prefab for " + t.ToString());
        return null;
    }

    public static GameObject InstantiatePrefabByName(string name)
    {
        if (prefabsList == null)
            prefabsList = Resources.LoadAll<GameObject>("Blocks");

        foreach (GameObject obj in prefabsList)
        {
            Block b = obj.GetComponent<Block>();
            if (b && obj.name == name)
            {
                return _InstantiatePrefab(obj);
            }
        }

        Debug.LogError("Cannot instantiate prefab for \"" + name + "\"");
        return null;
    }

    public static string AppendExtension(string path)
    {
        string ext = DefaultExtesion();
        if (ext == null || ext.Length < 1) return path;

        if (path.IndexOf(ext) > 0)
            return path;

        return path + ext;
    }

    static List<byte> getGroundBlocks(TableDescription tdesc)
    {
        List<byte> groundblocks = new List<byte>();

        for (byte y = 0; y < tdesc.groundHeight; y++)
        {
            for (byte x = 0; x < tdesc.groundWidth; x++)
            {
                bool exists = true;
                for (int k = 0; k < tdesc.emptyGroundBlocksCount * 2; k += 2)
                {
                    if (tdesc.emptyGroundBlocksXZ[k] == x && tdesc.emptyGroundBlocksXZ[k + 1] == y)
                    {
                        exists = false;
                        break;
                    }
                }

                if (exists)
                {
                    groundblocks.Add(x);
                    groundblocks.Add(y);
                }
            }
        }

        return groundblocks;
    }

    static Dictionary<GameObject, IconRepresentation> blockIcons = new Dictionary<GameObject, IconRepresentation>();

    public static Texture2D MakeThumbnail(TableDescription tdesc, int thumbnailSize = 64, bool transparent = true)
    {
        PixelBlock pb = new PixelBlock(thumbnailSize, thumbnailSize);
        if (transparent)
            BufferBlit.Fill(pb, new Color(1.0f, 1.0f, 1.0f, 0.0f));
        else
            //BufferBlit.Fill(pb, new Color(0.835f, 0.373f, 0.227f, 1.0f));
            BufferBlit.Fill(pb, new Color(0.5f, 0.5f, 0.5f, 1.0f));

        int groundSz = Math.Max(tdesc.groundWidth, tdesc.groundHeight);

        float tileSize = (float)thumbnailSize / (float)groundSz;
        float blockSize = tileSize * 0.95f;
        float groundTileSize = tileSize * 0.95f;
        
        float ofsx = (float)(-tdesc.groundWidth * tileSize + thumbnailSize) * 0.5f;
        float ofsy = (float)(-tdesc.groundHeight * tileSize + thumbnailSize) * 0.5f;

        // write ground
        List<byte> ground = getGroundBlocks(tdesc);

        GameObject groundPrefab = Resources.Load<GameObject>("GroundBlock");
        if(blockIcons.ContainsKey(groundPrefab) == false)
            blockIcons.Add(groundPrefab, GameObject.Instantiate<GameObject>(groundPrefab).GetComponent<IconRepresentation>());

        IconRepresentation groundIcon = blockIcons[groundPrefab];

        for (int i = 0; i < ground.Count; i += 2)
        {
            float x = ofsx + (float)ground[i] * tileSize;
            float y = ofsy + (float)ground[i + 1] * tileSize;

            float iconOfs = ((tileSize - groundTileSize) * 0.5f);

            BufferBlit.Blit(pb, Mathf.RoundToInt(x + iconOfs), Mathf.RoundToInt(y + iconOfs), groundIcon.GetSmallIcon((int)groundTileSize), BufferBlit.BlendOp.Alpha);
        }

        Vector3 groundMin = new Vector3(Mathf.Ceil(tdesc.groundWidth * 0.5f), 0.0f, Mathf.Ceil(tdesc.groundHeight * 0.5f)) * GameSession.gridUnit;
        Vector3 groundCenter = new Vector3(tdesc.groundCenterX, 0.0f, tdesc.groundCenterZ);

        // write the other blocks
        for(int i = 0; i < tdesc.blocks.Length; i++)
        {
            Block b = tdesc.blocks[i];

            if (
                b.blockType != Block.BlockType.Finish && 
                b.blockType != Block.BlockType.Spawn && 
                b.blockType != Block.BlockType.Default &&
                b.blockType != Block.BlockType.MovableBox
                )
                continue;

            Vector3 p = SnapToGrid.Snap(b.position) - new Vector3(0.5f, 0.0f, 0.5f) * GameSession.gridUnit + groundMin - groundCenter;

            float blockX = Mathf.Floor(p.x / GameSession.gridUnit);
            float blockZ = Mathf.Floor(p.z / GameSession.gridUnit);

            blockX = ofsx + blockX * tileSize;
            blockZ = ofsy + blockZ * tileSize;

            float iconOfs = ((tileSize - blockSize) * 0.5f);
            IconRepresentation ir = b.GetComponent<IconRepresentation>();
            BufferBlit.Blit(pb, Mathf.RoundToInt(blockX + iconOfs), Mathf.RoundToInt(blockZ + iconOfs), ir.GetSmallIcon((int)blockSize), BufferBlit.BlendOp.Alpha);
        }

        Texture2D tex = new Texture2D(thumbnailSize, thumbnailSize, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(pb.bufferRGBA);
        tex.Apply();

        return tex;
    }

    public static void Cleanup()
    {
        //delete dictionary
        foreach (IconRepresentation key in blockIcons.Values)
        {
            key.transform.SetParent(null);
            if (!Application.isPlaying)
                GameObject.DestroyImmediate(key.gameObject);
            else
                GameObject.Destroy(key.gameObject);
        }

        blockIcons.Clear();
    }
}
