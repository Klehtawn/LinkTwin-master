using UnityEngine;
using System.Collections;

public class RotatingBall : MonoBehaviour {

	// Use this for initialization

    Vector3 prevPos = Vector3.zero;

	void Start ()
    {
        prevPos = transform.position;
        Vector3 rot = new Vector3(UnityEngine.Random.Range(0.0f, 360.0f), 0.0f, UnityEngine.Random.Range(0.0f, 360.0f));
        transform.localRotation = Quaternion.Euler(rot);
	}
	
	// Update is called once per frame
	void Update () {

        if(enableRotation)
            RotateBall();
	}

    void RotateBall()
    {
        Vector3 dir = transform.position - prevPos;
        if (dir.sqrMagnitude < 0.0001f)
            return;

        float dist = dir.magnitude;
        dir /= dist;
        float radius = GetComponent<SphereCollider>().radius * transform.localScale.x * 0.5f;

        float angX = dist * 360.0f / (2.0f * Mathf.PI * radius);

        Vector3 up = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(dir, up)) > 0.99f)
            up = Vector3.forward;

        Quaternion q = Quaternion.LookRotation(dir, up);
        Quaternion q2 = Quaternion.AngleAxis(angX, q * Vector3.right);
        transform.localRotation = q2 * transform.localRotation;

        prevPos = transform.position;
    }

    bool enableRotation = true;
    public void EnableRotation(bool enable = true)
    {
        if (enableRotation == enable) return;
        if(enable)
            prevPos = transform.position;
        enableRotation = enable;
    }
}
