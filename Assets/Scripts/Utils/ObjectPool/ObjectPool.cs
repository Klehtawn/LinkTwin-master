using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool
{
    private GameObject prefab;
    private Transform root;
    private List<GameObject> allObjects;
    private Stack<GameObject> availableObjects;

    public bool autoIncreaseSize = true;

    public string name;

    static GameObject poolsRoot = null;

    static void CreatePoolsRoot()
    {
        if (poolsRoot != null) return;

        poolsRoot = new GameObject();
        poolsRoot.name = "The Pools";
    }

    public ObjectPool(GameObject prefab, Transform root, int capacity)
    {
        this.root = root;
        this.prefab = prefab;
        allObjects = new List<GameObject>();
        availableObjects = new Stack<GameObject>();

        IncreaseSize(capacity);
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = (GameObject)GameObject.Instantiate(prefab);
#if UNITY_EDITOR
        obj.name = prefab.name + "_Pooled_" + allObjects.Count;
#endif
        obj.SetActive(false);
        obj.transform.SetParent(root);
        obj.AddComponent<ObjectPooled>();
        return obj;
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;

        if (availableObjects.Count == 0 && autoIncreaseSize == false)
            return null;

        if (availableObjects.Count == 0 && autoIncreaseSize)
        {
            IncreaseSize();
            return Spawn(position, rotation);
        }

        obj = availableObjects.Pop();
        Transform trans = obj.transform;
        trans.position = position;
        trans.rotation = rotation;
        obj.SetActive(true);
        //obj.SendMessage("Start");

        obj.GetComponent<ObjectPooled>().destroyAfter = 0.0f;
        obj.GetComponent<ObjectPooled>().pool = this;
        obj.GetComponent<ObjectPooled>().usage++;

        if(objectPoolContext != null)
        {
            objectPoolContext.Register(obj);
        }

        return obj;
    }

    private void IncreaseSize(int capacity = -1)
    {
        int c = capacity > 0 ? capacity : allObjects.Count;
        for (int i = 0; i < c; i++)
        {
            GameObject obj = CreateNewObject();
            allObjects.Add(obj);
            availableObjects.Push(obj);
        }
    }

    public GameObject Spawn()
    {
        return Spawn(Vector3.zero, Quaternion.identity);
    }

    public void Destroy(GameObject obj, float delay = 0.0f)
    {
        if (delay <= 0.0f)
        {
            obj.SetActive(false);
            availableObjects.Push(obj);
        }
        else
        {
            obj.GetComponent<ObjectPooled>().destroyAfter = delay;
        }
    }

    public void Clear()
    {
        foreach (GameObject obj in availableObjects)
            GameObject.Destroy(obj);
        availableObjects.Clear();
        foreach (GameObject obj in allObjects)
            GameObject.Destroy(obj);
        allObjects.Clear();
    }

    public void Reset()
    {
        availableObjects.Clear();
        foreach (GameObject obj in allObjects)
        {
            obj.SetActive(false);
            availableObjects.Push(obj);
        }
    }

    private static Dictionary<GameObject, ObjectPool> pools = new Dictionary<GameObject, ObjectPool>();


    public static ObjectPool CreatePool(GameObject prefab, GameObject root, int capacity, string name = null)
    {
        if (pools.ContainsKey(prefab))
            RemovePool(prefab);

        if (root == null)
        {
            root = new GameObject();
            root.name = "ObjectPool_" + prefab.ToString();

            CreatePoolsRoot();
            root.transform.SetParent(poolsRoot.transform);
        }

        ObjectPool pool = new ObjectPool(prefab, root.transform, capacity);
        pools.Add(prefab, pool);
        pool.name = name;
        return pool;
    }

    public static ObjectPool CreatePool(GameObject prefab, int capacity, string name = null)
    {
        return CreatePool(prefab, null, capacity, name);
    }

    public static void RemovePool(GameObject prefab)
    {
        if (pools.ContainsKey(prefab))
        {
            ObjectPool pool = pools[prefab];
            pool.Clear();
            pools.Remove(prefab);
        }
    }

    public static void Cleanup()
    {
        foreach (var item in pools)
            item.Value.Clear();
        pools.Clear();
    }

    public static void ResetAll()
    {
        foreach (var item in pools)
            item.Value.Reset();
    }

    public static ObjectPool GetPool(GameObject prefab)
    {
        if (pools.ContainsKey(prefab))
            return pools[prefab];
        else
            return null;
    }

    public static int preferredPoolSize = 16;

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        ObjectPool pool = GetPool(prefab);
        if (pool == null)
            pool = CreatePool(prefab, preferredPoolSize);
        return pool.Spawn(position, rotation);
    }

    public static GameObject Spawn(GameObject prefab)
    {
        ObjectPool pool = GetPool(prefab);
        if (pool == null)
            pool = CreatePool(prefab, preferredPoolSize);
        return pool.Spawn();
    }

    public static void Destroy(GameObject prefab, GameObject obj)
    {
        GetPool(prefab).Destroy(obj);
    }

    static ObjectPoolContext objectPoolContext = null;
    public static void SetContext(ObjectPoolContext ctx)
    {
        objectPoolContext = ctx;
    }
}
