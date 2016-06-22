using UnityEngine;
using System.Collections;

public class FollowLocalSpace : MonoBehaviour {

    Vector3 offset = Vector3.zero;
    public Transform who;
	// Use this for initialization
	void Start () {

        if(who != null)
            offset = who.localPosition - transform.localPosition;
	}
	
	// Update is called once per frame
	void LateUpdate ()
    {
        if (who != null)
            transform.localPosition = who.localPosition - offset;
	}
}
