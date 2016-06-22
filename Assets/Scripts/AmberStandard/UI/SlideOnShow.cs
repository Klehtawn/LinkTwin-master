using UnityEngine;
using System.Collections;

public class SlideOnShow : ShowEffect {

    public Vector2 direction = Vector2.right;
    public LeanTweenType tween = LeanTweenType.easeOutBack;

    private bool prevFitToParent = false;
	// Use this for initialization
	void Start ()
    {
        WindowA w = GetComponent<WindowA>();
        w.centered = false;
        prevFitToParent = w.fitToParent;
        w.fitToParent = false;

        if(w != null)
        {
            if(isBackwards)
                direction *= -1.0f;

            Vector3 newPos = transform.position;
            Vector3 startPos = newPos - Desktop.main.DesktopToScreenRelative(new Vector3(direction.x * (Desktop.main.width + w.width) * 0.51f, direction.y * (Desktop.main.height + w.height) * 0.51f));
            LeanTween.move(gameObject, newPos, duration).setFrom(startPos).setEase(tween).setOnComplete(() => w.fitToParent = prevFitToParent);
            gameObject.transform.position = startPos;
        }
	}

    public override void Sync(ShowEffect other)
    {
        base.Sync(other);

        SlideOnShow sos = other as SlideOnShow;
        if (sos != null)
        {
            direction = sos.direction;
            tween = sos.tween;
        }
    }
}
