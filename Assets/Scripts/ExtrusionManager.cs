using UnityEngine;
using System.Collections;

public class ExtrusionManager : MonoBehaviour {

    Material extrusionMaterial = null;
    ExtrudedSprite extrudedSprite = null;

    Vector3 mapCenter = Vector3.zero;

#if !UNITY_EDITOR
    Vector3 prevPosition = Vector3.one * 100000.0f;
    Vector3 prevAngles = Vector3.one * 100000.0f;
#endif

    void Start () {

        mapCenter.z = 20000;

        extrudedSprite = GetComponentInChildren<ExtrudedSprite>();
        if (extrudedSprite != null)
        {
            extrusionMaterial = extrudedSprite.GetComponent<Renderer>().material;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (extrusionMaterial != null && TheGame.Instance != null)
        {
            if(mapCenter.z > 10000.0f)
                mapCenter = TheGame.Instance.blocks.GetGroundCenter();

#if !UNITY_EDITOR
            Vector3 movement = transform.position - prevPosition;
            Vector3 rot = transform.rotation.eulerAngles - prevAngles;
            if (movement.sqrMagnitude > 0.01f || rot.sqrMagnitude > 0.01f)
#endif
            {
                Vector3 dir = transform.position - mapCenter;
                float limit = GameSession.gridUnit * 3.0f;
                if (Mathf.Abs(dir.x) > limit)
                    dir.x = Mathf.Sign(dir.x) * limit;
                if (Mathf.Abs(dir.z) > limit)
                    dir.z = Mathf.Sign(dir.z) * limit;
                dir.y = -200.0f;
                dir.Normalize();

                dir = Vector3.forward;

                extrusionMaterial.SetVector("_ExtrusionDir", dir);
                extrusionMaterial.SetFloat("_Extrusion", extrudedSprite.extrusion);
                //extrusionMaterial.SetVector("_MapCenter", mapCenter);

                Quaternion invRot = Quaternion.Inverse(transform.rotation);
                dir = invRot * dir; dir.Normalize();

                Vector3 localPos = extrudedSprite.transform.parent.transform.localPosition;

                Vector3 finalPos = dir * extrudedSprite.extrusion;
                finalPos.y = localPos.y;
                extrudedSprite.transform.parent.localPosition = finalPos;

#if !UNITY_EDITOR
                prevPosition = transform.position;
                prevAngles = transform.rotation.eulerAngles;
#endif
            }
        }
        else
        {
            if (extrudedSprite != null)
                extrudedSprite.gameObject.SetActive(false);
        }
	}
}
