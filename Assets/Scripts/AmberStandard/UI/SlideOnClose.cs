using UnityEngine;
using System.Collections;

public class SlideOnClose : CloseEffect {

    public Vector2 direction = Vector2.right;
    public LeanTweenType tween = LeanTweenType.easeOutBack;

    public bool swipeToCloseOnModals = true;

    private bool prevFitToParent = false;

    void Start()
    {
        WindowA w = GetComponent<WindowA>();

        prevFitToParent = w.fitToParent;
        w.fitToParent = false;

        if (w != null)
        {
            w.OnWindowStartClosing += OnWindowStartClosing;
            if (w.isModal && swipeToCloseOnModals && w.userCloseEvent == false)
            {
                w.OnSwipe += OnTouchSwipe;
            }
        }
    }

    void OnWindowStartClosing(WindowA w, int returnValue)
    {
        if (isBackwards)
            direction *= -1.0f;

        Vector3 startPos = transform.position;
        Vector3 newPos = startPos + Desktop.main.DesktopToScreenRelative(new Vector3(direction.x * (w.width + Desktop.main.width) * 0.52f, direction.y * (w.height + Desktop.main.height) * 0.52f));
        LeanTween.move(gameObject, newPos, duration).setFrom(startPos).setEase(tween).setOnComplete(() => w.fitToParent = prevFitToParent);
    }

    public override void Sync(CloseEffect other)
    {
        base.Sync(other);

        SlideOnClose soc = other as SlideOnClose;
        if(soc != null)
        {
            direction = soc.direction;
            tween = soc.tween;
            swipeToCloseOnModals = soc.swipeToCloseOnModals;
        }
    }

    void OnTouchSwipe(MonoBehaviour sender, Vector2 d, float duration)
    {
        if(duration < 0.5f)
        {
            WindowA w = sender.GetComponent<WindowA>();

            if(w != null)
            {
                Vector2 p = new Vector2(d.x * direction.x, d.y * direction.y);
                if (p.magnitude > w.width * 0.5f && (p.x > 0.0f || p.y > 0.0f))
                    w.Close();
            }
        }
    }

    void OnDestroy()
    {
        WindowA w = GetComponent<WindowA>();
        if (w != null)
        {
            w.OnWindowStartClosing -= OnWindowStartClosing;
            if (w.isModal && swipeToCloseOnModals && w.userCloseEvent == false)
            {
                w.OnSwipe -= OnTouchSwipe;
            }
        }
    }
}
