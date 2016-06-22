using UnityEngine;
using System.Collections;

public class AutomaticScroll : MonoBehaviour {

    public float horizontalSpeed = 0.0f;
    public float verticalSpeed = 0.0f;

    RectTransform myRectTransform;
	void Start () {
        myRectTransform = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
	    if(horizontalSpeed != 0.0f || verticalSpeed != 0.0f)
        {
            Vector2 p = myRectTransform.anchoredPosition;
            p.x += Time.deltaTime * horizontalSpeed;
            p.y += Time.deltaTime * verticalSpeed;
            myRectTransform.anchoredPosition = p;
        }
	}
}
