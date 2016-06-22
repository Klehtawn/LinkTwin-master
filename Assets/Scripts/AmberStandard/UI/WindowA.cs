using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class WindowA : Widget
{
    [Range(0.0f, 1.0f)]
    public float transparency = 1.0f;

    public bool centered = false;
    public bool fitToParent = false;
    public bool alwaysOnTop = false;
    public bool alwaysOnBack = false;

    [HideInInspector]
    [NonSerialized]
    public bool isModal = false;

    private CanvasGroup canvasGroup;

    public Action<WindowA, int> OnWindowClosed;
    public Action<WindowA, int> OnWindowStartClosing;
    public Action<WindowA, float> OnWindowClosing;

    public Action<WindowA> OnWindowFinishedShowing;

    [HideInInspector]
    [NonSerialized]
    public WindowA sourcePrefab;

    public string title;

    public bool userCloseEvent = false;

    [NonSerialized]
    public bool finishedShowing = false;

    protected override void Awake()
    {
        base.Awake();

        //RectTransform rt = GetComponent<RectTransform>();
        //rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.0f, 0.0f);

        GetCanvasGroup();

        sourcePrefab = null;
    }

    protected override void Start()
    {
        base.Start();

        ShowEffect se = GetComponent<ShowEffect>();
        if(se != null)
        {
            StartCoroutine(_FinishedShowing(new WaitForSeconds(se.duration)));
        }
        else
        {
            StartCoroutine(_FinishedShowing(new WaitForEndOfFrame()));
        }

        if(isModal)
        {
            Desktop.main.modalWindowsFlow.Push(this);
        }
    }

    IEnumerator _FinishedShowing(YieldInstruction wait)
    {
        yield return wait;
        if (OnWindowFinishedShowing != null)
            OnWindowFinishedShowing(this);
        finishedShowing = true;
    }

    public bool IsClosing
    {
        get { return isClosingTimer >= 0.0f; }
    }

    bool createArgsHandled = false;
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();

        if (createArgsHandled == false)
        {
            HandleCreateArgs(createArgs);
            createArgsHandled = true;
        }

        UpdateWindowParameters();
        
        if(isClosingTimer >= 0.0f)
        {
            isClosingTimer -= Time.deltaTime;

            if (OnWindowClosing != null)
            {
                float f = 0.0f;
                if (isClosingDuration > 0.0f)
                    f = Mathf.Clamp01(isClosingTimer / isClosingDuration);
                OnWindowClosing(this, f);
            }

            if(isClosingTimer < 0.0f)
            {
                if (OnWindowClosed != null)
                    OnWindowClosed(this, closingReturnValue);

                if (isMinimized == false)
                    Destroy();
                else
                    gameObject.SetActive(false);
            }
        }
	}

    protected void UpdateWindowParameters()
    {
        canvasGroup.alpha = transparency;

        if (centered)
        {
            Center();
        }

        if (fitToParent)
        {
            FitToParent();
        }

        if (alwaysOnTop)
        {
            BringToFront();
        }

        if (alwaysOnBack)
        {
            SendToBack();
        }
    }

    public void EnableInput(bool enable)
    {
        canvasGroup.interactable = enable;
        canvasGroup.blocksRaycasts = enable;
    }

    public void DisableInput()
    {
        EnableInput(false);
    }

    private void Center()
    {
        //if(Desktop.main != null)
       //     pos = new Vector2((Desktop.main.width - size.x) * 0.5f, (Desktop.main.height - size.y) * 0.5f);
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one * 0.5f;
        rt.anchoredPosition = Vector3.zero;
    }

    private void FitToParent()
    {
        /*pos = Vector2.zero;
        if(parent == null)
        {
            if(Desktop.main != null)
                size = Desktop.main.size;
        }
        else
        {
            size = parent.size;
        }*/

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector3.zero;
        rt.anchorMax = Vector3.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    protected float isClosingTimer = -1.0f;
    protected float isClosingDuration = -1.0f;

    int closingReturnValue = -1;

    bool isMinimized = false;
    public void Minimize()
    {
        if (isMinimized) return;
        isMinimized = true;

        Desktop.main.minimizedWindows.Add(this);

        Close();
    }

    public void Restore()
    {
        if (isMinimized == false) return;

        isMinimized = false;

        Desktop.main.minimizedWindows.Remove(this);

        GetCanvasGroup();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    public bool IsMinimized()
    {
        return isMinimized;
    }

    public void Close(int returnValue = 0)
    {
        closingReturnValue = returnValue;

        float duration = 0.0f;

        CloseEffect ce = GetComponent<CloseEffect>();
        if(ce != null)
        {
            duration = ce.duration;
            Desktop.main.windowWait = ce.desktopWait ? ce.duration : 0.0f;
        }

        if (OnWindowStartClosing != null)
            OnWindowStartClosing(this, returnValue);

        isClosingDuration = duration;
        isClosingTimer = duration;

        GetCanvasGroup();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public static WindowA Create(WindowA src, string args = null)
    {
        return Desktop.main.CreateWindow(src, args);
    }

    public static WindowA Create(string srcName, string args = null)
    {
        return Desktop.main.CreateWindow(srcName, args);
    }

    public WindowA Show(bool show = true)
    {
        Desktop.main.ShowWindow(this, show);
        return this;
    }

    public WindowA Hide()
    {
        return Show(false);
    }
    public WindowA ShowModal(bool show = true)
    {
        if (show)
            Desktop.main.ShowWindowModal(this);
        else
            visible = false;

        return this;
    }

    public void SetTransparency(float t)
    {
        GetCanvasGroup();

        canvasGroup.alpha = t;
        transparency = t;
    }

    void GetCanvasGroup()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        GetCanvasGroup();
        UpdateWindowParameters();
    }

    string createArgs = null;
    public void SetCreateArgs(string args)
    {
        createArgs = args;
    }

    protected virtual void HandleCreateArgs(string args)
    {
    }
}
