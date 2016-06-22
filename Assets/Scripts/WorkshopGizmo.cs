using UnityEngine;
using System.Collections;

public class WorkshopGizmo : MonoBehaviour {

    public enum GizmoType
    {
        BigX,
        Exclamation,
        GreenDot
    }

    public GizmoType type;

    public Vector3 offset = Vector3.zero;

    [HideInInspector]
    public float lifeTime = -1.0f;

    [HideInInspector]
    public Vector3 inheritPosition = Vector3.zero;

    [HideInInspector]
    public Block block;

    [HideInInspector]
    public Camera viewCam;

    RectTransform rect;

    float destroyTimer = 0.0f;

    CanvasGroup canvasGroup;
	void Start ()
    {
        if(block != null)
            block.OnBlockDeleted += OnBlockDeleted;
        else
        {

        }
        rect = GetComponent<RectTransform>();

        destroyTimer = lifeTime;

        canvasGroup = GetComponent<CanvasGroup>();
        
        Reposition();
	}

    void OnBlockDeleted(Block b)
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if(block != null)
            block.OnBlockDeleted -= OnBlockDeleted;
    }

    void Reposition()
    {
        if (viewCam != null && block != null)
        {
            Vector3 p2 = viewCam.WorldToScreenPoint(block.origin + offset * GameSession.gridUnit);
            //p2 = Desktop.main.ScreenToDesktop(p2);
            p2.z = 0.0f;
            rect.anchoredPosition3D = p2;
        }
        else if(viewCam != null)
        {
            Vector3 p2 = viewCam.WorldToScreenPoint(inheritPosition + offset * GameSession.gridUnit);
            //p2 = Desktop.main.ScreenToDesktop(p2);
            p2.z = 0.0f;
            rect.anchoredPosition3D = p2;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(block != null)
            Reposition();

        if(lifeTime > 0.0f)
        {
            if(canvasGroup != null && destroyTimer < 1.0f)
            {
                canvasGroup.alpha = destroyTimer * destroyTimer;
            }
            destroyTimer -= Time.deltaTime;
            if (destroyTimer < 0.0f)
                Destroy(gameObject);
        }
	}
}
