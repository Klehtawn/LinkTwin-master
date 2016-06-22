using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WindowsFlow
{
    List<WindowA> flowPrefabs = new List<WindowA>(); // prefabs
    WindowA lastWindowInstance = null;

    public bool isModal = false;

    public WindowsFlow(bool isModal = false)
    {
        this.isModal = isModal;
    }

    private WindowA Last()
    {
        if (flowPrefabs.Count < 1) return null;
        return flowPrefabs[flowPrefabs.Count - 1];
    }

    public void Backward(int returnValue = 0, bool showLast = true)
    {
        if (flowPrefabs.Count < 2) return;

        Pop();

        if (lastWindowInstance.GetComponent<CloseEffect>() != null)
            lastWindowInstance.GetComponent<CloseEffect>().isBackwards = true;

        lastWindowInstance.Close(returnValue);

        if (showLast && Last() != null)
        {
            WindowA newWindow = WindowA.Create(Last());
            if (newWindow.GetComponent<ShowEffect>() != null)
                newWindow.GetComponent<ShowEffect>().isBackwards = true;
            if (isModal)
                newWindow.ShowModal();
            else
                newWindow.Show();
            lastWindowInstance = newWindow;
        }
    }

    public void Push(WindowA w)
    {
        if (w.sourcePrefab == null)
        {
#if UNITY_EDITOR
            GameObject go = UnityEditor.PrefabUtility.GetPrefabParent(w.gameObject) as GameObject;
            if (go != null)
                w.sourcePrefab = go.GetComponent<WindowA>();
#else
            return;
#endif
        }
        if (Last() == w.sourcePrefab) return;
        flowPrefabs.Add(w.sourcePrefab);
        lastWindowInstance = w;
    }

    private WindowA Pop()
    {
        if (flowPrefabs.Count == 0) return null;

        WindowA l = Last();
        flowPrefabs.RemoveAt(flowPrefabs.Count - 1);

        return l;
    }

    public void Clear()
    {
        flowPrefabs.Clear();
    }

    public void RemoveLast()
    {
        Pop();
        return;
    }

    public bool IsWindowOnTop(WindowA w)
    {
        return lastWindowInstance == w;
    }

    public bool Empty()
    {
        return lastWindowInstance == null;
    }
}
