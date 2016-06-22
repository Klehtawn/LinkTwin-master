using UnityEngine;
using System.Collections;

public class DelayedActivate : MonoBehaviour {

    public float delay = 0.0f;

    public GameObject which;
	
	void Start ()
    {
        if (delay > 0.0f)
        {
            StartCoroutine(_Activate());
            which.SetActive(false);
        }
	}
	
	IEnumerator _Activate()
    {
        yield return new WaitForSeconds(delay);
        which.SetActive(true);
    }
}
