using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPoolContext
{
    List<ObjectPooled> objectsPooled = new List<ObjectPooled>();

    public ObjectPoolContext(bool automaticRegister = true)
    {
        if (automaticRegister)
            ObjectPool.SetContext(this);
        else
            ObjectPool.SetContext(null);
    }

    public void Register(ObjectPooled op)
    {
        objectsPooled.Add(op);
    }
    public void Register(GameObject obj)
    {
        ObjectPooled op = obj.GetComponent<ObjectPooled>();
        if(op != null)
            objectsPooled.Add(op);
    }

    public void Cleanup()
    {
        foreach (ObjectPooled op in objectsPooled)
            op.Remove();

        objectsPooled.Clear();

        ObjectPool.SetContext(null);
    }
}
