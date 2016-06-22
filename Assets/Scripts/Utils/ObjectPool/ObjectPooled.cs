using UnityEngine;
using System.Collections;

public class ObjectPooled : MonoBehaviour {

    private float _destroyAfter = 0.0f;
    private float destroyTimer = 0.0f;

    public int usage = 0;
    public float destroyAfter
    {
        get { return _destroyAfter; }
        set { _destroyAfter = value; destroyTimer = value; }
    }

    public ObjectPool pool;

    Transform originalParent = null;

	// Use this for initialization
	void Awake ()
    {
        originalParent = transform.parent;
	}
	
	// Update is called once per frame
	void Update () {
	
        if(destroyAfter > 0.0f && destroyTimer > 0.0f)
        {
            destroyTimer -= Time.deltaTime;
            if(destroyTimer < 0.0f)
            {
                Remove();
            }
        }
	}

    public void Remove()
    {
        gameObject.transform.SetParent(originalParent);
        pool.Destroy(gameObject, 0.0f);
    }
}
