using UnityEngine;
using System.Collections;
using System;

public class ShowEffect : MonoBehaviour
{
    public float duration = 0.5f;

    [HideInInspector]
    [NonSerialized]
    public bool isBackwards = false;

    void Awake()
    {
        isBackwards = false;
    }

    public virtual void Sync(ShowEffect other)
    {
        if (other == null)
            duration = 0.0f;
        else
        {
            duration = other.duration;
            isBackwards = other.isBackwards;
        }
    }

    public virtual bool Equals(ShowEffect other)
    {
        return duration == other.duration && isBackwards == other.isBackwards;
    }

    public virtual void Restore()
    {

    }

    public virtual void StartShowing()
    {

    }
}
