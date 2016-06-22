using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class SerializerSimple : MonoBehaviour
{
    public enum DataTypes
    {
        Byte,
        SByte,
        Int,
        Float,
        Short,
        Bool,
        String
    }

    int significantValuesCount = 0;
    int significantValuesStartPosition;

    public void PrepareWritingSignificantData(ref BinaryWriter bw)
    {
        significantValuesStartPosition = (int)bw.BaseStream.Position;
        bw.Write((int)0);
        significantValuesCount = 0;
    }

    public void FinishWritingSignificantData(ref BinaryWriter bw)
    {
        int currentBlockPos = (int)bw.BaseStream.Position;
        bw.Seek(significantValuesStartPosition, System.IO.SeekOrigin.Begin);
        bw.Write(significantValuesCount);
        bw.Seek(currentBlockPos, System.IO.SeekOrigin.Begin);
    }

    protected void WriteSignificantValue(ref BinaryWriter bw, string name, int value)
    {
        bw.Write(name);
        bw.Write((byte)DataTypes.Int);
        bw.Write(value);
        significantValuesCount++;
    }

    protected void WriteSignificantValue(ref BinaryWriter bw, string name, bool value)
    {
        bw.Write(name);
        bw.Write((byte)DataTypes.Bool);
        bw.Write((byte)(value ? 1 : 0));
        significantValuesCount++;
    }

    protected void WriteSignificantValue(ref BinaryWriter bw, string name, float value)
    {
        bw.Write(name);
        bw.Write((byte)DataTypes.Float);
        bw.Write(value);
        significantValuesCount++;
    }

    protected void WriteSignificantValue(ref BinaryWriter bw, string name, string value)
    {
        bw.Write(name);
        bw.Write((byte)DataTypes.String);
        bw.Write(value);
        significantValuesCount++;
    }

    protected void WriteSignificantValue(ref BinaryWriter bw, string name, short value)
    {
        bw.Write(name);
        bw.Write((byte)DataTypes.Short);
        bw.Write(value);
        significantValuesCount++;
    }

    protected void WriteSignificantValue(ref BinaryWriter bw, string name, byte value)
    {
        bw.Write(name);
        bw.Write((byte)DataTypes.Byte);
        bw.Write(value);
        significantValuesCount++;
    }

    protected void WriteSignificantValue(ref BinaryWriter bw, string name, sbyte value)
    {
        bw.Write(name);
        bw.Write((byte)DataTypes.SByte);
        bw.Write(value);
        significantValuesCount++;
    }

    protected int ReadSignificantValueInt32(ref BinaryReader br, string name)
    {
        CheckDataTypes(name, DataTypes.Int);

        foreach (SignificantValue sv in readSignificantValues)
        {
            if (sv.name == name && sv.type == DataTypes.Int)
            {
                br.BaseStream.Seek(sv.offset, SeekOrigin.Begin);
                return br.ReadInt32();
            }
        }
        return 0;
    }

    protected float ReadSignificantValueFloat(ref BinaryReader br, string name)
    {
        CheckDataTypes(name, DataTypes.Float);

        foreach (SignificantValue sv in readSignificantValues)
        {
            if (sv.name == name && sv.type == DataTypes.Float)
            {
                br.BaseStream.Seek(sv.offset, SeekOrigin.Begin);
                return br.ReadSingle();
            }
        }
        return 0.0f;
    }

    protected bool ReadSignificantValueBool(ref BinaryReader br, string name)
    {
        CheckDataTypes(name, DataTypes.Bool);

        foreach (SignificantValue sv in readSignificantValues)
        {
            if (sv.name == name && sv.type == DataTypes.Bool)
            {
                br.BaseStream.Seek(sv.offset, SeekOrigin.Begin);
                return br.ReadByte() != 0;
            }
        }
        return false;
    }

    protected string ReadSignificantValueString(ref BinaryReader br, string name)
    {
        CheckDataTypes(name, DataTypes.String);

        foreach (SignificantValue sv in readSignificantValues)
        {
            if (sv.name == name && sv.type == DataTypes.String)
            {
                br.BaseStream.Seek(sv.offset, SeekOrigin.Begin);
                return br.ReadString();
            }
        }
        return "";
    }
    protected sbyte ReadSignificantValueSByte(ref BinaryReader br, string name)
    {
        CheckDataTypes(name, DataTypes.SByte);

        foreach (SignificantValue sv in readSignificantValues)
        {
            if (sv.name == name && sv.type == DataTypes.SByte)
            {
                br.BaseStream.Seek(sv.offset, SeekOrigin.Begin);
                return br.ReadSByte();
            }
        }
        return 0;
    }

    protected byte ReadSignificantValueByte(ref BinaryReader br, string name, byte failValue = 0)
    {
        CheckDataTypes(name, DataTypes.Byte);

        foreach (SignificantValue sv in readSignificantValues)
        {
            if (sv.name == name && sv.type == DataTypes.Byte)
            {
                br.BaseStream.Seek(sv.offset, SeekOrigin.Begin);
                return br.ReadByte();
            }
        }
        return failValue;
    }

    void CheckDataTypes(string varName, DataTypes type)
    {
        foreach (SignificantValue sv in readSignificantValues)
        {
            if (sv.name == varName && sv.type != type)
            {
                Debug.LogError("Type mismatch for [" + varName + "]: expected " + sv.type.ToString() + ", reading " + type.ToString() + "\n");
                break;
            }
        }
    }

    public virtual void WriteSignificantInfo(ref BinaryWriter bw)
    {
    }

    struct SignificantValue
    {
        public string name;
        public DataTypes type;
        public int offset;
    }

    List<SignificantValue> readSignificantValues = new List<SignificantValue>();

    int significantInfoBlockEnd;
    public virtual void ReadSignificantInfo(ref BinaryReader br)
    {
    }

    public virtual void PrepareReadingSignificantInfo(ref BinaryReader br)
    {
        readSignificantValues.Clear();
        int numValues = br.ReadInt32();
        for (int i = 0; i < numValues; i++)
        {
            SignificantValue sv = new SignificantValue();
            sv.name = br.ReadString();
            sv.type = (DataTypes)br.ReadByte();
            sv.offset = (int)br.BaseStream.Position;
            ReadTypedValue(ref br, sv.type);

            readSignificantValues.Add(sv);
        }

        significantInfoBlockEnd = (int)br.BaseStream.Position;
    }

    public void FinishReadingSignificantInfo(ref BinaryReader br)
    {
        br.BaseStream.Seek((int)significantInfoBlockEnd, SeekOrigin.Begin);
    }

    public virtual void LinkSignificantValues(Block[] blocks)
    {

    }

    void ReadTypedValue(ref BinaryReader br, DataTypes t)
    {
        if (t == DataTypes.Byte || t == DataTypes.Bool)
            br.ReadByte();
        else
            if (t == DataTypes.Int)
                br.ReadInt32();
            else
                if (t == DataTypes.Short)
                    br.ReadInt16();
                else
                    if (t == DataTypes.Float)
                        br.ReadSingle();
                    else
                        if (t == DataTypes.String)
                            br.ReadString();
                        else
                            if (t == DataTypes.Byte)
                                br.ReadByte();
                            else
                                if (t == DataTypes.SByte)
                                    br.ReadSByte();
                                else
                                    Debug.Assert(true, "!!!!");
    }
}
