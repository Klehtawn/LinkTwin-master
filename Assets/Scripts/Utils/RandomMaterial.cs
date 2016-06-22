using UnityEngine;
using System.Collections;

public class RandomMaterial : MonoBehaviour {

    public Renderer rendererToChange;
    public Material[] materials;

    private int usingMaterial = 0;

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
                usingMaterial = Random.Range(0, 100) % materials.Length;
            rendererToChange.material = materials[usingMaterial];
        }
    }
}
