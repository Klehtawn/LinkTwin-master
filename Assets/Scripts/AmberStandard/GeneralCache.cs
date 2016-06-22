using UnityEngine;
using System.Collections;
using System;

public class GeneralCache : MonoBehaviour {

    [Serializable]
    public class ObjectPoolInfo
    {
        public GameObject objectToPool;
        public int defaultSize;
        public string resourceName;
    }

    public ObjectPoolInfo[] objectPools;
    public float delay = 1.0f;

	void Start ()
    {
        StartCoroutine(CreatePools(delay));
	}

    void Update()
    {

    }

    IEnumerator CreatePools(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach(ObjectPoolInfo opi in objectPools)
        {
            ObjectPool.CreatePool(opi.objectToPool, opi.defaultSize, opi.resourceName);
        }
    }
}
