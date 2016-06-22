using UnityEngine;
using System.Collections;

public class RotateMe2 : MonoBehaviour
{
    public Vector3 axis = new Vector3(0.0f, 0.0f, 1.0f);

    public float initialAngle = 1.0f;
    public float finalAngle = 0.0001f;

    public float duration = 0.5f;

    private float timer = 0.0f;

    Quaternion initialRot;
	void Start()
	{
        initialRot = transform.localRotation;
	}
	
	void Update()
	{
        timer += Time.deltaTime;
        if (timer >= duration)
            timer -= duration;

        Quaternion q1 = Quaternion.AngleAxis(initialAngle, axis) * initialRot;
        Quaternion q2 = Quaternion.AngleAxis(finalAngle, axis) * initialRot;

        transform.localRotation = Quaternion.Lerp(q1, q2, Mathf.Clamp01(timer / duration));
	}
}
