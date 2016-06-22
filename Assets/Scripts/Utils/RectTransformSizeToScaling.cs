using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RectTransformSizeToScaling : MonoBehaviour {

    public float referenceSize = 100.0f;
    public float goZeroBelowValue = 50.0f;
    public float maxSize = 200.0f;

	void Update () {
        getRectTransform();
        getTransformToChange();

        if(myRectTransform != null && transformToChange != null)
        {
            Vector3[] corners = new Vector3[4];
            myRectTransform.GetLocalCorners(corners);
            float sz = Mathf.Max(corners[2].x - corners[1].x, Mathf.Abs(corners[2].y - corners[1].y));
            sz = Mathf.Min(sz, maxSize);
            if (sz < goZeroBelowValue)
                sz = 0.0001f;
            transformToChange.localScale = Vector3.one * sz / referenceSize;
        }
	}

    RectTransform myRectTransform = null;
    void getRectTransform()
    {
        if (myRectTransform == null)
            myRectTransform = GetComponent<RectTransform>();
    }

    Transform transformToChange = null;

    void getTransformToChange()
    {
        if(transformToChange == null)
        {
            if(transform.childCount > 0)
                transformToChange = transform.GetChild(0);
        }
    }
}
