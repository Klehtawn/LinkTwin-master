using UnityEngine;
using System.Collections;

public class CheckeredMaterial : MonoBehaviour {

    public Renderer rendererToChange;
    public Material[] materials;

    private int usingMaterial = 0;

    public float step = 1.0f;

	// Use this for initialization
	void Start ()
    {
        Set();
	}

    public void Set()
    {
        if (materials != null && rendererToChange != null)
        {
            if (materials.Length > 1)
            {
                usingMaterial = Mathf.Abs(Mathf.FloorToInt(transform.position.x / step));
                usingMaterial += Mathf.Abs(Mathf.FloorToInt(transform.position.z / step));
                
                usingMaterial = usingMaterial % materials.Length;
            }
            rendererToChange.material = materials[usingMaterial];
        }
    }
}
