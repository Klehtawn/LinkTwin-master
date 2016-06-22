using UnityEngine;
using System.Collections;

public class RotateMe : MonoBehaviour
{
	public float RotationSpeed = -1.5f;
    private float currentRotationSpeed = -1.5f;
    public bool interpolate = false;

    public Vector3 axis = Vector3.forward;

	void Start()
	{
	}
	
	void Update()
	{
        if(interpolate)
        {
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, RotationSpeed, Time.deltaTime * 4.0f);
        }
        else
        {
            currentRotationSpeed = RotationSpeed;
        }

        transform.Rotate(axis * currentRotationSpeed);
	}
}
