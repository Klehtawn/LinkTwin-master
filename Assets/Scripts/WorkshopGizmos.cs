using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WorkshopGizmos : MonoBehaviour {

    public RectTransform redX;
    public RectTransform exclamation;
    public RectTransform dot;

    [HideInInspector]
    public Camera tableCamera;

	void Start ()
    {
        redX.gameObject.SetActive(false);
        exclamation.gameObject.SetActive(false);
        dot.gameObject.SetActive(false);
	}

    List<WorkshopGizmo> gizmos = new List<WorkshopGizmo>();

    public void RemoveGizmo(Block b, WorkshopGizmo.GizmoType type)
    {
        for (int i = 0; i < gizmos.Count; i++)
        {
            if (gizmos[i].block == b && gizmos[i].type == type)
            {
                Destroy(gizmos[i].gameObject);
                gizmos.RemoveAt(i);
                i--;
            }
        }
    }

    public bool HasGizmo(Block b, WorkshopGizmo.GizmoType type)
    {
        for (int i = 0; i < gizmos.Count; i++)
        {
            if (gizmos[i].block == b && gizmos[i].type == type)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveGizmos(Block b)
    {
        for (int i = 0; i < gizmos.Count; i++)
        {
            if (gizmos[i].block == b)
            {
                Destroy(gizmos[i].gameObject);
                gizmos.RemoveAt(i);
                i--;
            }
        }
    }

    public WorkshopGizmo Find(Block b, WorkshopGizmo.GizmoType type)
    {
        for (int i = 0; i < gizmos.Count; i++)
        {
            if (gizmos[i].block == b && gizmos[i].type == type)
            {
                return gizmos[i];
            }
        }

        return null;
    }
	

    public WorkshopGizmo ShowGizmo(Block b, WorkshopGizmo.GizmoType type, bool show = true)
    {
        if(show == false)
        {
            RemoveGizmo(b, type);
            return null;
        }

        //WorkshopGizmo g = Find(b, type);
        //if (g != null) return g;

        GameObject o = null;
        
        if(type == WorkshopGizmo.GizmoType.BigX)
            o = GameObject.Instantiate<GameObject>(redX.gameObject);
        else
        if (type == WorkshopGizmo.GizmoType.Exclamation)
            o = GameObject.Instantiate<GameObject>(exclamation.gameObject);
        else
        if (type == WorkshopGizmo.GizmoType.GreenDot)
            o = GameObject.Instantiate<GameObject>(dot.gameObject);

        o.transform.SetParent(transform);
        o.SetActive(true);

        WorkshopGizmo gizmo = o.GetComponent<WorkshopGizmo>();

        gizmo.block = b;
        gizmo.viewCam = tableCamera;

        gizmos.Add(gizmo);

        RectTransform rt = o.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = Vector2.zero;

        return gizmo;
    }

    public void ShowExclamation(Block b, bool show = true)
    {
        if (HasGizmo(b, WorkshopGizmo.GizmoType.Exclamation) && show) return;

        ShowGizmo(b, WorkshopGizmo.GizmoType.Exclamation, show);
    }

    public WorkshopGizmo ShowBigX(Vector3 position, bool show = true, float duration = -1.0f)
    {
        WorkshopGizmo g = ShowGizmo(null, WorkshopGizmo.GizmoType.BigX, show);
        g.lifeTime = duration;
        g.inheritPosition = position + Vector3.up * GameSession.gridUnit;

        return g;
    }

    public void RemoveExclamation(Block b)
    {
        RemoveGizmo(b, WorkshopGizmo.GizmoType.Exclamation);
    }

    
    public void Clear()
    {
    }
}