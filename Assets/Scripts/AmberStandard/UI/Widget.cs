using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;


//[ExecuteInEditMode]
public class Widget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static Widget capture = null;

    protected bool isStarted = false;

    RectTransform rectT;

    private int mustCallOnClick = -1;

    protected virtual void Start()
    {
        visible = true;
    }

    protected virtual void Awake()
    {
        GetRectTransform();
        //rectT.anchorMin = rectT.anchorMax = rectT.pivot = new Vector2(0.0f, 0.0f);
    }

    protected virtual void Update()
    {
        isStarted = true;
        if (_isActive == false)
            return;

        // failsafe

#if !UNITY_EDITOR
        if(false)
#else
        if (Input.touchCount == 0 && Input.GetMouseButton(0) == false)
#endif
        {
            pointerDown = false;
        }
        
        if (pointerDown)
        {
            //if (Input.touchCount > 0 || Input.GetMouseButton(0))
            {
                Vector2 currentPos = Vector2.zero;
#if UNITY_EDITOR
                currentPos = Input.mousePosition;
#else
                currentPos = Input.GetTouch(0).position;                    
#endif

                if (OnTouchMoved != null)
                {
                    OnTouchMoved(this, Desktop.main.ScreenToDesktopRelative(pointerDownPos - screenPos), Desktop.main.ScreenToDesktopRelative(currentPos - screenPos));
                }

                if(canDoLongPress)
                { 
                    Vector2 d = currentPos - pointerDownPos;
                    if (d.sqrMagnitude > 5.0f * 5.0f)
                        canDoLongPress = false;
                }

                if(canDoLongPress == true && (Time.time - pointerDownMoment) > LongPressDuration && calledLongPress == false)
                {
                    if (OnLongPress != null)
                        OnLongPress(this, Desktop.main.ScreenToDesktopRelative(currentPos - screenPos));
                    calledLongPress = true;
                }
            }
        }

        if (mustCallOnClick > 0)
            mustCallOnClick--;

        if(mustCallOnClick == 0)
        {
            if (parentIsScrolling() == false)
            {
                if (OnClick != null)
                    // OnTap(this, Desktop.main.ScreenToDesktop(ped.position - screenPos));
                    OnClick(this, pointerUpPos);
                else
                {
                    Transform p = transform.parent;
                    while (p != null)
                    {
                        Widget wp = p.GetComponent<Widget>();
                        if (wp != null && wp.OnClick != null)
                        {
                            wp.OnClick(wp, pointerUpPos);
                            break;
                        }

                        p = p.parent;
                    }
                }
            }

            mustCallOnClick = -1;
        }
    }

    public float width
    {
        get
        {
            return size.x;
        }
        set
        {
            size = new Vector2(value, height);
        }
    }

    public float height
    {
        get
        {
            return size.y;
        }
        set
        {
            size = new Vector2(width, value);
        }
    }

    public float aspect
    {
        get
        {
            if (height == 0.0f)
                return 1.0f;

            return width / height;
        }
    }

    public Vector2 size
    {
        get
        {
            GetRectTransform();
            return rectT.rect.size;
        }
        set
        {
            GetRectTransform();
            rectT.sizeDelta = value;
        }
    }

    public Vector2 pos
    {
        get
        {
            GetRectTransform();
            return rectT.anchoredPosition;
        }
        set
        {
            GetRectTransform();
            rectT.anchoredPosition = value;
        }
    }

    public Vector2 screenPos
    {
        get
        {
            GetRectTransform();
            Vector3[] corners = new Vector3[4];
            rectT.GetWorldCorners(corners);
            return corners[0];
        }
    }

    public Vector2 screenCenter
    {
        get
        {
            GetRectTransform();
            Vector3[] corners = new Vector3[4];
            rectT.GetWorldCorners(corners);
            return (corners[0] + corners[2]) * 0.5f;
        }
    }

    public Widget parent
    {
        get
        {
            if (transform.parent == null)
                return null;
            return transform.parent.GetComponent<Widget>();
        }
        set
        {
            if (value == null)
                Desktop.main.AddWidget(this);
            else
                transform.SetParent(value.transform);
        }
    }

    public bool visible
    {
        get
        {
            return gameObject.activeSelf;
        }
        set
        {
            if (gameObject.activeSelf != value)
            {
                gameObject.SetActive(value);
                if (OnWidgetShown != null)
                    OnWidgetShown(this);

                if(visible == true)
                {
                    ShowEffect se = GetComponent<ShowEffect>();
                    if (se != null)
                        se.StartShowing();
                }
            }
        }
    }

    bool _isActive = true;
    public bool active
    {
        get
        {
            return _isActive;
        }

        set
        {
            if (_isActive != value)
            {
                CanvasGroup cg = GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.blocksRaycasts = value;
                    cg.interactable = value;
                }
                _isActive = value;
                if (OnWidgetActivated != null)
                    OnWidgetActivated(this);
            }
        }
    }

    public Action<Widget> OnWidgetActivated;

    public Action<Widget> OnWidgetShown;

    public void BringToFront()
    {
        if (transform.parent == null) return;

        transform.SetAsLastSibling();
    }
    public void SendToBack()
    {
        if (transform.parent == null) return;
        transform.SetAsFirstSibling();
    }

    public void Destroy()
    {
        transform.SetParent(null);
        Destroy(gameObject);
    }

    public Action<Widget> OnWidgetDestroyed;
    void OnDestroy()
    {
        if (OnWidgetDestroyed != null)
        {
            OnWidgetDestroyed(this);
        }
    }

    public Action<MonoBehaviour, Vector2> OnTouchDown; // local coords
    public Action<MonoBehaviour, Vector2> OnTouchUp; // local coords
    public Action<MonoBehaviour, Vector2, Vector2> OnTouchMoved; // local coords : startPos, currentPos
    public Action<MonoBehaviour, Vector2> OnMouseEnter; // local coords
    public Action<MonoBehaviour, Vector2> OnMouseExit; // local coords

    public static float LongPressDuration = 1.5f;
    public Action<MonoBehaviour, Vector2> OnLongPress; // local coords

    public Action<MonoBehaviour, Vector2, float> OnSwipe; // delta, duration
    public Action<MonoBehaviour, Vector2> OnClick;

    bool pointerDown = false;
    bool calledLongPress = false;
    bool canDoLongPress = true;
    Vector2 pointerDownPos = Vector2.zero;
    Vector2 pointerUpPos = Vector2.zero;
    float pointerDownMoment = 0.0f;

    ScrollRect parentWithScrollRect;
    Vector2 scrollRectContentPos;

    public void OnPointerDown(PointerEventData ped)
    {
        //Debug.Log("PointerDown @" + ped.position.x + "x" + ped.position.y);

        capture = this;

        if (OnTouchDown != null && _isActive)
            OnTouchDown(this, Desktop.main.ScreenToDesktopRelative(ped.position - screenPos));

        pointerDown = true;
        pointerDownPos = ped.position;
        pointerDownMoment = Time.time;
        calledLongPress = false;
        canDoLongPress = true;

        parentWithScrollRect = transform.GetComponentInParent<ScrollRect>();
        if(parentWithScrollRect != null)
        {
            scrollRectContentPos = parentWithScrollRect.content.anchoredPosition;
        }

        mustCallOnClick = -1;
    }

    bool parentIsScrolling()
    {
        if (parentWithScrollRect == null) return false;
        return Vector2.Distance(scrollRectContentPos, parentWithScrollRect.content.anchoredPosition) > 0.01f;
    }

    public void OnPointerUp(PointerEventData ped)
    {
        capture = null;
        //Debug.Log("PointerUp @" + ped.position.x + "x" + ped.position.y);

        pointerUpPos = ped.position;
        
        if (OnTouchUp != null && _isActive)
            OnTouchUp(this, Desktop.main.ScreenToDesktopRelative(ped.position - screenPos));

        Vector2 d = ped.position - pointerDownPos;
        const float swipeDelta = 20.0f;
        //Debug.Log("deltas: " + Mathf.Abs(d.x) + ", " + Mathf.Abs(d.y));

        float screenThreshold = (float)Screen.width / 20.0f;

        float pressDuration = Time.time - pointerDownMoment;

        if(Mathf.Abs(d.x) > swipeDelta || Mathf.Abs(d.y) > swipeDelta)
        {
            if (OnSwipe != null)
                OnSwipe(this, Desktop.main.ScreenToDesktopRelative(d), pressDuration);
            else
            {
                Transform p = transform.parent;
                while(p != null)
                {
                    Widget wp = p.GetComponent<Widget>();
                    if (wp != null && wp.OnSwipe != null)
                    {
                        wp.OnSwipe(wp, Desktop.main.ScreenToDesktopRelative(d), pressDuration);
                        break;
                    }

                    p = p.parent;
                }
            }
        }
        else if (Mathf.Abs(d.x) < screenThreshold && Mathf.Abs(d.y) < screenThreshold)
        {
            mustCallOnClick = 5;
        }

        pointerDown = false;
    }

    public void OnPointerEnter(PointerEventData ped)
    {
        if (_isActive == false) return;

        if (OnMouseEnter != null)
            OnMouseEnter(this, Desktop.main.ScreenToDesktopRelative(ped.position - screenPos));
    }

    public void OnPointerExit(PointerEventData ped)
    {
        if (_isActive == false) return;

        if (OnMouseExit != null)
            OnMouseExit(this, Desktop.main.ScreenToDesktopRelative(ped.position - screenPos));
    }

    public static void DeleteAllChildren(Transform t)
    {
        while (t.transform.childCount > 0)
        {
            Transform c = t.transform.GetChild(t.transform.childCount - 1);
            c.SetParent(null);
            if (Application.isPlaying)
                GameObject.Destroy(c.gameObject);
            else
                GameObject.DestroyImmediate(c.gameObject);
        }
    }

    public void DeleteAllChildren()
    {
        DeleteAllChildren(transform);
    }

    public static void SetLayer(GameObject obj, int layer)
    {
        obj.layer = layer;

        for (int i = 0; i < obj.transform.childCount; i++)
            SetLayer(obj.transform.GetChild(i).gameObject, layer);
    }

    protected virtual void OnValidate()
    {
        GetRectTransform();
    }

    RectTransform GetRectTransform()
    {
        if (rectT == null)
            rectT = GetComponent<RectTransform>();

        return rectT;
    }

    public void FitToChildren(Vector2 borders)
    {
        if (transform.childCount == 0) return;

        Vector3 min = Vector3.one * 10000.0f;
        Vector3 max = -min;

        Vector3[] corners = new Vector3[4];
        
        for(int i = 0; i < transform.childCount; i++)
        {
            RectTransform rt = transform.GetChild(i).GetComponent<RectTransform>();
            rt.GetLocalCorners(corners);

            foreach(Vector3 c in corners)
            {
                Vector3 p = c + rt.anchoredPosition3D;
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
        }

        Vector3 sz = max - min;
        size = new Vector2(sz.x + borders.x * 2.0f, sz.y + borders.y * 2.0f);
        //pos = new Vector2(min.x - borders.x, min.y - borders.y);
    }

    public void Activate(bool activate = true, float delay = 0.0f)
    {
        StartCoroutine(_Activate(activate, delay));
    }

    IEnumerator _Activate(bool activate, float delay)
    {
        yield return new WaitForSeconds(delay);
        active = activate;
    }
}