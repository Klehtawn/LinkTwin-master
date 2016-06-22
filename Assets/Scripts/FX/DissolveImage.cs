using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class DissolveImage : MonoBehaviour {

    Material myMaterial;
    int dissolveFactorUniform;

    [Range(0, 1)]
    public float factor = 0.0f;

    private float prevFactor = -1.0f;
    private float currentFactor = 0.0f;

    public bool interpolate = true;

    public float interpolateSpeed = 6.0f;

	void Start () {
        GetMyMaterial();
	}

    void GetMyMaterial()
    {
        if (myMaterial != null) return;

        Image img = GetComponent<Image>();

        if (img != null)
        {
            myMaterial = new Material(img.material);
            img.material = myMaterial;

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
                currentFactor = Mathf.Lerp(currentFactor, factor, interpolateSpeed * Time.deltaTime);
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

        GetMyMaterial();

        if(myMaterial != null)
            myMaterial.SetFloat(dissolveFactorUniform, currentFactor);
    }
}
