using UnityEngine;
using System.Collections;

public class RandomRotation : MonoBehaviour {

    public float angle = 90.0f;
    public Vector3 axis = Vector3.up;
	// Use this for initialization
	void Start ()
    {
        int i = Random.Range(0, Mathf.CeilToInt(360.0f / angle));
        transform.localRotation = Quaternion.AngleAxis((float)i * angle, axis);
	}
}
