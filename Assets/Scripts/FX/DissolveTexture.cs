using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DissolveTexture : MonoBehaviour {

    Material myMaterial;
    int dissolveFactorUniform;

    [Range(0, 1)]
    public float factor = 0.0f;
    private float prevFactor = -1.0f;
    private float currentFactor = 0.0f;

    public bool interpolate = false;
	void Start () {
        Renderer r = GetComponent<Renderer>();

        if (r != null)
        {
            myMaterial = Application.isPlaying ? r.material : r.sharedMaterial;

            dissolveFactorUniform = Shader.PropertyToID("_DissolveFactor");
            if (myMaterial.HasProperty("_DissolveFactor") == false)
            {
                myMaterial = null;
            }
        }
        else
        {
            myMaterial = null;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (myMaterial != null)
        {
            if (interpolate == false)
                currentFactor = factor;
            else
            {
                currentFactor = Mathf.Lerp(currentFactor, factor, 6.0f * Time.deltaTime);
                if (Mathf.Abs(currentFactor - factor) < 0.01f)
                    currentFactor = factor;
            }

            if (prevFactor != currentFactor)
            {
                myMaterial.SetFloat(dissolveFactorUniform, currentFactor);
                prevFactor = currentFactor;
            }
        }
	}

    public void SetFrom(float v, float from)
    {
        factor = v;
        currentFactor = from;
        prevFactor = -1.0f;
    }
}
