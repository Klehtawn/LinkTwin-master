using UnityEngine;
using System.Collections;

public class BlockCreateEffect : MonoBehaviour {

    public bool firstScaleIsOnX = true;

    Vector3 initialScaling, zeroScaling;

    public float delay = 0.5f;
    public float randomness = 0.3f;
	void Start () {

        initialScaling = transform.localScale;
        zeroScaling = Vector3.one * 0.0001f;
        transform.localScale = zeroScaling;

        DoAnimation(Random.Range(0.0f, randomness) + delay);
	}
	
	public void DoAnimation(float delay)
    {
        StartCoroutine(_DoAnimation(delay));
    }

    IEnumerator _DoAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 scale1 = initialScaling;

        /*if(firstScaleIsOnX)
            scale1.z = zeroScaling.z;
        else
            scale1.x = zeroScaling.x;*/

        Vector3 scale2 = scale1;

        if(firstScaleIsOnX)
            scale2.z = initialScaling.z;
        else
            scale2.x = initialScaling.x;

        LeanTweenType anim = LeanTweenType.easeOutBack;

        float duration = Random.Range(0.2f, 0.6f) * 0.5f;

        LeanTween.scale(gameObject, scale1, duration).setEase(anim).setOnComplete(() =>
            {
                //LeanTween.scale(gameObject, scale2, duration).setEase(anim);
            });
    }
}
