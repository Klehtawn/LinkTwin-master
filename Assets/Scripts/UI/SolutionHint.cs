using UnityEngine;
using UnityEngine.UI;

public class SolutionHint : Widget
{
    public Image arrowImage;
    public float inflateScale = 1.5f;
    public float inflateTime = 0.1f;
    public LeanTweenType inflateType = LeanTweenType.linear;
    public float deflateTime = 0.5f;
    public LeanTweenType deflateType = LeanTweenType.easeOutElastic;

    public void SetDirection(Vector3 dir)
    {
        Vector3 from = Vector3.down;
        Vector3 to = new Vector3(dir.x, dir.z, 0.0f);
        Quaternion rot = Quaternion.FromToRotation(from, to);
        arrowImage.rectTransform.rotation = rot;

        LeanTween.scale(arrowImage.gameObject, Vector3.one * inflateScale, inflateTime).setEase(inflateType);
        LeanTween.scale(arrowImage.gameObject, Vector3.one, deflateTime).setDelay(inflateTime).setEase(deflateType);
    }
}
