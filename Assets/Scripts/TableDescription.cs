using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class TableDescription
{
    // description
    public char[] descriptor = new char[8] { 'L', 'I', 'N', 'K', 'T', 'W', 'I', 'N'};

    public ulong crc;

    public const ushort CURRENT_VERSION = 4;

    public ushort version = 1;

    public char[] uniqueIdentifier = new char[32];

    public ushort index = 0;
    public byte difficulty = 0;
    public byte stage = 0;
    public string name = "UserGeneratedMap";

#if UNITY_EDITOR
    public string author = Environment.UserName;
#else
    public string author = "Unnamed";
#endif

    // data
    // ground
    public byte groundWidth = 0;
    public byte groundHeight = 0;
    public float groundCenterX = 0.0f;
    public float groundCenterZ = 0.0f;

    // emtpy blocks
    public ushort emptyGroundBlocksCount = 0;
    public byte[] emptyGroundBlocksXZ;

    // other blocks
    public ushort numBlocks = 0;
    public Block[] blocks;

    // solution
    public ushort numSolutionSteps = 0;
    public byte[] solutionSteps;
}
