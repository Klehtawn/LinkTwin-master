using UnityEngine;
using System.Collections;

public class RandomMaterialForExtrude : MonoBehaviour {

    public MeshExtrude meshExtrude;
    public Material[] materials;

    private int usingMaterial = 0;

	// Use this for initialization
	void Start ()
    {
        Set();
	}

    public void Set()
    {
        if (materials != null && meshExtrude != null)
        {
            if (materials.Length > 1)
                usingMaterial = Random.Range(0, 100) % materials.Length;
            meshExtrude.material = materials[usingMaterial];
        }
    }
}
