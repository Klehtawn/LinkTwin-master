using UnityEngine;
using System.Collections;

public class ButtonPressScaleEffect : ButtonPressEffect
{
    public float scaling = 0.9f;
    public float pressDuration = 0.5f;
    public float releaseDuration = 0.5f;
    protected override void OnButtonPress()
    {
        LeanTween.scale(gameObject, Vector3.one * scaling, pressDuration).setEase(LeanTweenType.easeOutBack);
    }
    protected override void OnButtonReleased()
    {
        LeanTween.scale(gameObject, Vector3.one, releaseDuration).setEase(LeanTweenType.easeOutElastic);
    }

    public override void CopyFrom(ButtonPressEffect other)
    {
        ButtonPressScaleEffect bpe = other as ButtonPressScaleEffect;
        scaling = bpe.scaling;
        pressDuration = bpe.pressDuration;
        releaseDuration = bpe.releaseDuration;
    }
}
