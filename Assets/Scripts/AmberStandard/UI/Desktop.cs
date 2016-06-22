using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class Desktop : MonoBehaviour {

    private Transform background;
    private Camera backgroundCamera;

    private Canvas canvas;

    private Transform root;
    private Transform rootModals;

    private Theme prevTheme = null;
    public Theme theme;

    public GameScreen index;

    [HideInInspector]
    public WindowsFlow windowsFlow = new WindowsFlow();

    [HideInInspector]
    public WindowsFlow modalWindowsFlow = new WindowsFlow(true);

    private static Desktop _instance = null;

    [HideInInspector]
    [NonSerialized]
    public SoundSchemePlayback sounds;

    public Transform topBanner = null;
    public Transform bottomBanner = null;

    private int topBannerHeight = 0;
    private int bottomBannerHeight = 0;

    public RectTransform overlays;

    public bool ignoreNextStart = false;
    public bool ignoreNextPause = false;

    [NonSerialized]
    public PostProcessVanilla postProcess;

    [NonSerialized]
    public float windowWait = 0.0f;

    public List<WindowA> minimizedWindows = new List<WindowA>();
    public static Desktop main
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<Desktop>();
            }
            return _instance;
        }
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        UpdateTheme();

        if (Application.isPlaying)
        {
            if (root.childCount == 0 && rootModals.childCount == 0 && index != null)
            {
                WindowA.Create(index).Show();
            }
        }

        if(false)
        //if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.worldCamera = Camera.main;
            canvas.worldCamera.orthographicSize = width;
            canvas.worldCamera.transform.rotation = Quaternion.identity;
            canvas.worldCamera.transform.position = Vector3.back * 200.0f;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }
    }

	void Awake ()
    {
        canvas = transform.Find("MainCanvas").GetComponent<Canvas>();

        background = transform.Find("BackgroundCanvas/Background");
        backgroundCamera = transform.Find("BackgroundCamera").GetComponent<Camera>();
        backgroundCamera.cullingMask = 1 << LayerMask.NameToLayer("Background");

        root = canvas.transform.Find("Root");
        rootModals = canvas.transform.Find("RootModals");

        postProcess = GetComponent<PostProcessVanilla>();

        // compute client area
        if (topBanner != null && topBanner.gameObject.activeSelf)
        {
            topBannerHeight = (int)topBanner.GetComponent<RectTransform>().rect.height;
        }

        if (bottomBanner != null && bottomBanner.gameObject.activeSelf)
        {
            bottomBannerHeight = (int)bottomBanner.GetComponent<RectTransform>().rect.height;
        }

        root.GetComponent<RectTransform>().offsetMax = new Vector2(0.0f, -topBannerHeight);
        root.GetComponent<RectTransform>().offsetMin = new Vector2(0.0f, bottomBannerHeight);

        rootModals.GetComponent<RectTransform>().offsetMax = new Vector2(0.0f, -topBannerHeight);
        rootModals.GetComponent<RectTransform>().offsetMin = new Vector2(0.0f, bottomBannerHeight);

        windowsFlow.Clear();

        Camera.main.clearFlags = CameraClearFlags.Nothing;
        Camera.main.cullingMask &=  ~(1 << LayerMask.NameToLayer("Background"));
        Camera.main.orthographic = true;

        if (cameraFit)
            FixCameraSizeAndPosition();

        sounds = SoundSchemePlayback.Initialize(transform);

        GameSession.IncrementSessionIndex();
    }

    void FixCameraSizeAndPosition()
    {
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = Screen.height * 0.5f;
        Camera.main.transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, -200.0f);
    }
	
	// Update is called once per frame

    //private Text displayRes = null;

    public bool cameraFit = false;

	void Update ()
    {
        if(cameraFit)
            FixCameraSizeAndPosition();

        /*if(displayRes == null)
        {
            displayRes = canvas.transform.Find("Display").GetComponent<Text>();
        }

        displayRes.text = width.ToString() + "x" + height.ToString() + "(" + canvas.scaleFactor + ")";*/

        if (prevTheme != theme || (theme != null && theme.hasChanged))
            UpdateTheme();
        //if (modalWindows.Length == 2)
        {
            if (closeModalBackgroundTicks > 0)
            {
                closeModalBackgroundTicks--;
                if (closeModalBackgroundTicks == 0)
                {
                    //Debug.Log("BLACK CLOSING: (" + modalsBackground + ")");
                    modalsBackground.Close();
                    modalsBackground = null;
                    //Debug.Log("BLACK CLOSED END (" + modalsBackground + ")");
                }
            }
        }

        /*if (topmostModalWindow == modalsBackground && modalsBackground != null)
        {
            modalsBackground.Close();
            modalsBackground = null;
        }*/

        ThemedElement.UpdateAppearance(transform, GetComponent<ThemedElement>());
	}

    void UpdateTheme()
    {
        theme.hasChanged = false;

        ThemedElement[] allElems = GetComponentsInChildren<ThemedElement>();
        foreach(ThemedElement te in allElems)
        {
            if(te != null)
                te.UpdateTheme(true);
        }

        SetBackground(null);
        backgroundCamera.backgroundColor = theme.generalAppearance.desktop;

        prevTheme = theme;
    }

    GameObject currentBackground = null;
    GameObject currentBackgroundSrc = null;

    public GameObject GetBackgroundSource()
    {
        return currentBackgroundSrc;
    }

    public void SetBackground(RectTransform newBkg, float fadeDuration = 0.0f)
    {
        if(newBkg == null)
        {
            SetBackground(theme.desktopBackground, fadeDuration);
            return;
        }

        if (newBkg != null && newBkg.gameObject == currentBackgroundSrc) return;

        if (fadeDuration == 0.0f)
        {
            Widget.DeleteAllChildren(background);
            currentBackground = null;
        }
        else
        {
            FadeAndDestroy.Destroy(currentBackground, fadeDuration);
        }

        if (newBkg != null)
        {
            currentBackgroundSrc = newBkg.gameObject;
            currentBackground = ThemedElement.CreateReference(currentBackgroundSrc, true);
            if (currentBackground != null)
            {
                RectTransform rt = currentBackground.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.transform.SetParent(background);
                    rt.transform.localScale = Vector3.one;
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;
                    rt.anchoredPosition3D = Vector3.zero;
                }
            }

            FadeOnCreate.Fade(currentBackground, fadeDuration);
        }
    }

    public void SetTheme(Theme t, float fadeDuration)
    {
        if(fadeDuration == 0.0f)
        {
            theme = t;
        }
        else
        {

        }
    }

    public WindowA topmostWindow
    {
        get
        {
            if (root.childCount == 0) return null;
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform c = root.GetChild(i);
                WindowA w = c.GetComponent<WindowA>();
                if (w != null)
                    return w;
            }

            return null;
        }
    }

    public WindowA topmostModalWindow
    {
        get
        {
            if (rootModals.childCount == 0) return null;
            for (int i = rootModals.childCount - 1; i >= 0; i--)
            {
                Transform c = rootModals.GetChild(i);
                WindowA w = c.GetComponent<WindowA>();
                if (w != null && !w.IsClosing)
                    return w;
            }

            return null;
        }
    }

    public WindowA CreateWindow(WindowA prefab, string args = null)
    {
        if (prefab == null) return null;

        WindowA wnd = GameObject.Instantiate<WindowA>(prefab);
        wnd.parent = null;

        wnd.transform.localScale = Vector3.one;
        wnd.pos = prefab.GetComponent<RectTransform>().anchoredPosition;
        wnd.size = prefab.GetComponent<RectTransform>().sizeDelta;

        wnd.sourcePrefab = prefab;

        wnd.SetCreateArgs(args);

        wnd.visible = false;

        wnd.OnWidgetDestroyed += OnWindowDestroyed;

        return wnd;
    }

    void OnWindowDestroyed(Widget w)
    {
        if (minimizedWindows.Contains(w as WindowA))
            minimizedWindows.Remove(w as WindowA);
    }

    public WindowA CreateWindow(string resName, string args = null)
    {
        GameObject wndRes = Resources.Load<GameObject>(resName);
        if (wndRes == null) return null;
        return CreateWindow(wndRes.GetComponent<WindowA>(), args);
    }

    public void ShowWindow(WindowA wnd, bool show = true)
    {
        if (wnd == null) return;

        if(windowWait > 0.0f)
        {
            float delay = windowWait;
            windowWait = 0.0f;
            ShowWindowDelayed(wnd, delay, show);
            return;
        }

        if (wnd.isModal)
            wnd.parent = null;

        wnd.visible = show;
        wnd.isModal = false;

        wnd.Restore();
    }

    int modalsShown = 0;

    public void ShowWindowModal(WindowA wnd)
    {
        if (modalsBackground == null)
        {
            //Debug.Log("SHOW BLACK");
            ShowTransparentBlack();
            if (modalsBackground != null)
            {
                ShowEffect sfx = modalsBackground.GetComponent<ShowEffect>();
                if (sfx != null)
                {
                    sfx.Sync(wnd.GetComponent<ShowEffect>());
                }
            }
            //Debug.Log("SHOW BLACK DONE");
        }

        wnd.transform.SetParent(rootModals);
        wnd.visible = true;
        wnd.isModal = true;

        wnd.OnWindowStartClosing += OnWindowStartedClosing;

        closeModalBackgroundTicks = -1;

        modalsShown++;

        //Debug.Log("show modal: " + modalsShown + " frame " + Time.frameCount);
        //Debug.Log("SHOW MODAL!!!!");

        windowWait = 0.0f;
    }

    int closeModalBackgroundTicks = -1;
    void OnWindowStartedClosing(WindowA who, int returnValue)
    {
        if(who.isModal)
        {
            modalsShown--;
            //Debug.Log("close modal: " + modalsShown + " frame " + Time.frameCount);
        }

        if (who.isModal && modalsBackground != null && who != modalsBackground && modalsShown <= 1)
        {
            //Debug.Log("starting modal close tick");
            closeModalBackgroundTicks = 5;
            CloseEffect cfx = modalsBackground.GetComponent<CloseEffect>();
            if (cfx != null)
            {
                cfx.Sync(who.GetComponent<CloseEffect>());
            }
        }

        if (minimizedWindows.Contains(who))
            minimizedWindows.Remove(who);
    }

    WindowA modalsBackground = null;
    public void ShowTransparentBlack()
    {
        if (modalsBackground != null)
            return;

        modalsBackground = CreateWindow(theme.modalsTransparentBlack);
        if(modalsBackground != null)
            ShowWindowModal(modalsBackground);
        modalsBackground.OnTouchUp += OnBackgroundTouch;
    }

    void OnBackgroundTouch(MonoBehaviour sender, Vector2 pos)
    {
        // close modal
        WindowA topmost = topmostModalWindow;
        if (topmost != null && topmost.userCloseEvent == false)
            topmost.Close();
    }

    public bool IsWindowOnTop(WindowA w)
    {
        if (modalWindows.Length == 0)
            return windowsFlow.IsWindowOnTop(w);
        else
            return modalWindowsFlow.IsWindowOnTop(w);
    }

    public int width
    {
        get
        {
            return (int)size.x;
        }
    }

    public int height
    {
        get
        {
            return (int)size.y;
        }
    }

    public int totalWidth
    {
        get
        {
            return width;
        }
    }

    public int totalHeight
    {
        get
        {
            return height + bottomBannerHeight + topBannerHeight;
        }
    }

    public float aspect
    {
        get
        {
            return (float)width / (float)height;
        }
    }

    public Vector2 size
    {
        get
        {
            Vector2 sz = canvas.GetComponent<RectTransform>().rect.size;

            sz.x = Mathf.Ceil(sz.x);
            sz.y = Mathf.Ceil(sz.y - (float)bottomBannerHeight - (float)topBannerHeight);
            return sz;
        }
    }

    public WindowA[] windows
    {
        get
        {
            return root.GetComponentsInChildren<WindowA>();
        }
    }

    public WindowA[] modalWindows
    {
        get
        {
            return rootModals.GetComponentsInChildren<WindowA>();
        }
    }

    public void AddWidget(Widget w)
    {
        w.transform.SetParent(root);
    }

    public void HideWindowsBeneath(WindowA w)
    {
        if(w.isModal)
        {
            foreach(WindowA wnd in modalWindows)
            {
                if(wnd == w)
                {
                    break;
                }
                wnd.visible = false;
            }
        }
    }

    public void ShowWindowDelayed(WindowA w, float delay, bool show = true)
    {
        StartCoroutine(_ShowWindowDelayed(w, show, delay));
    }

    IEnumerator _ShowWindowDelayed(WindowA w, bool show, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowWindow(w, show);
    }

    public Vector2 ScreenToDesktop(Vector2 sp)
    {
        Vector2 p = sp;
        p.x = (float)totalWidth * sp.x / (float)Screen.width;
        p.y = (float)totalHeight * sp.y / (float)Screen.height - bottomBannerHeight;
        return p;
    }

    public Vector2 ScreenToDesktopRelative(Vector2 sp)
    {
        Vector2 p = sp;
        p.x = (float)totalWidth * sp.x / (float)Screen.width;
        p.y = (float)totalHeight * sp.y / (float)Screen.height;
        return p;
    }

    public Vector2 DesktopToScreen(Vector2 dp)
    {
        dp.x = (float)Screen.width * dp.x / (float)totalWidth;
        dp.y = (float)Screen.height * (dp.y + bottomBannerHeight) / (float)totalHeight;
        return dp;
    }

    public Vector2 DesktopToScreenRelative(Vector2 dp)
    {
        dp.x = (float)Screen.width * dp.x / (float)totalWidth;
        dp.y = (float)Screen.height * dp.y / (float)totalHeight;
        return dp;
    }

    public Vector3 DesktopToScreen(Vector3 dp)
    {
        dp.x = (float)Screen.width * dp.x / (float)totalWidth;
        dp.y = (float)Screen.height * (dp.y + bottomBannerHeight) / (float)totalHeight;
        return dp;
    }

    public Vector3 DesktopToScreenRelative(Vector3 dp)
    {
        dp.x = (float)Screen.width * dp.x / (float)totalWidth;
        dp.y = (float)Screen.height * dp.y / (float)totalHeight;
        return dp;
    }

    public Vector2 DesktopToViewport(Vector2 dp)
    {
        dp.x = dp.x / width;
        dp.y = dp.y / height;
        return dp;
    }

    public Vector2 ViewportToDesktop(Vector2 dp)
    {
        dp.x = dp.x * width;
        dp.y = dp.y * height;
        return dp;
    }

    public void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            if (!ignoreNextPause)
                GameSession.EndSession();
            ignoreNextPause = false;
        }
        else
        {
            if (!ignoreNextStart)
                GameSession.StartSession();
            ignoreNextStart = false;
        }
    }
}
