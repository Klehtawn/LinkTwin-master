using UnityEngine;
using System.Collections;

public class BlendTextures : MonoBehaviour {

    public MeshRenderer rendererToChange;

    [Range(0.0f, 1.0f)]
    public float factor = 0.0f;

    private float prevFactor = -1.0f;
    private float targetFactor = 0.0f;

    int fadeFactorUniform;

	// Use this for initialization
	void Start ()
    {
        fadeFactorUniform = Shader.PropertyToID("_BlendTexturesFactor");
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(Mathf.Abs(factor - targetFactor) >= 0.001)
        {
            factor = Mathf.Lerp(factor, targetFactor, 12.0f * Time.deltaTime);
        }
        else
        {
            factor = targetFactor;
        }

        UploadFactor();
	}

    public void SetFactorInterpolated(float f, float delay = 0.0f)
    {
        if(delay == 0.0f)
            targetFactor = f;
        else
        {
            StopCoroutine(_SetTargetFactor(f, delay));
            StartCoroutine(_SetTargetFactor(f, delay));
        }
    }

    IEnumerator _SetTargetFactor(float f, float delay)
    {
        yield return new WaitForSeconds(delay);
        targetFactor = f;
    }

    void UploadFactor()
    {
        if (factor == prevFactor) return;
        rendererToChange.material.SetFloat(fadeFactorUniform, factor);
        prevFactor = factor;
    }
}
