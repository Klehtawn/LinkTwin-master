using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UndoSystemWorkshop
{
    Stack<byte[]> undoStack = new Stack<byte[]>();
    Stack<byte[]> redoStack = new Stack<byte[]>();

	public bool canUndo
    {
        get
        {
            return undoStack.Count > 0;
        }
    }

    public bool canRedo
    {
        get
        {
            return redoStack.Count > 0;
        }
    }

    public byte[] Peek()
    {
        if (canUndo == false) return null;
        return undoStack.Peek();
    }

    public void LoadLast(Transform table)
    {
        if (canUndo == false) return;

        byte[] buffer = undoStack.Peek();
        LoadFromBuffer(buffer, table);
    }

    public void Undo(Transform table)
    {
        if (canUndo == false) return;

        byte[] buffer = undoStack.Pop();

        SetUndoPoint(table, true);

        LoadFromBuffer(buffer, table);

        redoStack.Push(undoStack.Pop());
    }

    public void Redo(Transform table)
    {
        if (canRedo == false) return;

        SetUndoPoint(table, true);

        LoadFromBuffer(redoStack.Pop(), table);
    }

    public void Clear()
    {
        undoStack.Clear();
        redoStack.Clear();
    }
    public void ClearRedo()
    {
        redoStack.Clear();
    }

    public void SetUndoPoint(Transform table, bool dontClear = false)
    {
        if(dontClear == false)
            redoStack.Clear();

        TableDescription td = null;
        TableLoadSave.ConvertSceneToMapDescription(ref td, table);
        undoStack.Push(TableLoadSave.SaveToMemory(td));
    }

    void LoadFromBuffer(byte[] bytes, Transform table)
    {
        TableDescription td = null;
        LevelRoot root = table.GetComponent<LevelRoot>();
        root.CreateStructure();
        TableLoadSave.LoadFromMemory(bytes, ref td, root);
        TableLoadSave.ConvertMapDescriptionToScene(td, root);
        Widget.SetLayer(root.Ground.transform.gameObject, table.gameObject.layer);
        Widget.SetLayer(root.Table.transform.gameObject, table.gameObject.layer);
    }
}
