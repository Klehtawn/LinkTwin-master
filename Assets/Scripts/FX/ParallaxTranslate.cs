using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ParallaxTranslate : MonoBehaviour {

    public Vector3 translateSpeed = Vector3.zero;

    public Vector3 clamp = Vector3.one;

    public float interpolateFactor = 6.0f;

    private Vector3 initialPosition;

    private Vector3 offsetPosition = Vector3.zero;
    private Text debugText;
    private string debugTextStr;

	// Use this for initialization
	void Start () {
        initialPosition = transform.localPosition;
        debugText = GetComponentInChildren<Text>();
        if (debugText != null)
            debugTextStr = debugText.text;
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 acc = Vector3.zero;

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Alpha1))
            acc.x -= 1.0f;
        if (Input.GetKey(KeyCode.Alpha2))
            acc.x += 1.0f;

        if (Input.GetKey(KeyCode.Alpha3))
            acc.y -= 1.0f;
        if (Input.GetKey(KeyCode.Alpha4))
            acc.y += 1.0f;
#else
        acc = Input.acceleration;
        if (acc.sqrMagnitude > 1.0f)
            acc.Normalize();
        acc *= 5.0f;
#endif

        acc *= 3.0f;

        acc.x *= translateSpeed.x;
        acc.y *= translateSpeed.y;
        acc.z *= translateSpeed.z;

        offsetPosition += acc * Time.deltaTime;

        if (Mathf.Abs(offsetPosition.x) > clamp.x)
            offsetPosition.x = clamp.x * Mathf.Sign(offsetPosition.x);

        if (Mathf.Abs(offsetPosition.y) > clamp.y)
            offsetPosition.y = clamp.y * Mathf.Sign(offsetPosition.y);

        if (Mathf.Abs(offsetPosition.z) > clamp.z)
            offsetPosition.z = clamp.z * Mathf.Sign(offsetPosition.z);

        //offsetPosition *= 3.0f;

        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + offsetPosition, interpolateFactor * Time.deltaTime);

        if(debugText != null)
        {
            debugText.text = debugTextStr + "\n\n" + offsetPosition.ToString();
        }
	}
}