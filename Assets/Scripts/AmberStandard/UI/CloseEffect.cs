using UnityEngine;
using System.Collections;
using System;

public class CloseEffect : MonoBehaviour
{
    public float duration = 0.5f;

    [HideInInspector]
    [NonSerialized]
    public bool isBackwards = false;

    public bool desktopWait = false;

    void Awake()
    {
        isBackwards = false;
    }

    public virtual void Sync(CloseEffect other)
    {
        if (other == null)
            duration = 0.0f;
        else
        {
            duration = other.duration;
            isBackwards = other.isBackwards;
            desktopWait = other.desktopWait;
        }
    }
}
