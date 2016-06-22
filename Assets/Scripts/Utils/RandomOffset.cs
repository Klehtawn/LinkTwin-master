using UnityEngine;
using System.Collections;

public class RandomOffset : MonoBehaviour {

    public Vector3 min = Vector3.zero;
    public Vector3 max = Vector3.down;
	// Use this for initialization
	void Start ()
    {
        Vector3 dir = max - min;
        float d = dir.magnitude;
        if(d > 0.0f)
        {
            transform.localPosition = min + dir * Random.Range(0.0f, d);
        }
	}
	
}
