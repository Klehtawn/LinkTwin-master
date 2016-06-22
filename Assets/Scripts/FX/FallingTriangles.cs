using UnityEngine;
using System.Collections;

public class FallingTriangles : MonoBehaviour {

    Material mat;

    float time;
	
	void Start () {

        GetComponent<SplitTriangles>().Split();

        mat = GetComponent<Renderer>().material;
        mat.SetVector("_FallingDirection", Vector3.down);

        time = -1.0f;

        PrepareMesh(true);

        //StartEffect();
	}

    public void StartEffect(bool onlyMargins = false)
    {
        if(onlyMargins == false)
        {
            PrepareMesh(false);
        }
        else
        {
            PrepareMesh(true);
        }

        time = 0.0f;
    }

    public bool isEffectStarted
    {
        get
        {
            return time >= 0.0f;
        }
    }

    bool normalsCreated = false;

    void PrepareMesh(bool onlyMargins = false)
    {
        MeshFilter mf = GetComponent<MeshFilter>();

        int vc = mf.mesh.vertexCount;

        Color[] colors = new Color[vc];

        if (normalsCreated == false)
        {
            Vector3[] normals = new Vector3[vc];

            for (int i = 0; i < vc; i += 3)
            {
                Vector3 center = Vector3.zero;
                for (int k = 0; k < 3; k++)
                {
                    center += mf.mesh.vertices[i + k];
                }
                center /= 3.0f;
                for (int k = 0; k < 3; k++)
                {
                    normals[i + k] = center;
                }
            }
            mf.mesh.normals = normals;

            normalsCreated = true;
        }

        if (onlyMargins)
        {
            float rad = mf.mesh.bounds.extents.x * 0.7f;
            for (int i = 0; i < vc; i += 3)
            {
                float spd = Random.Range(0.4f, 3.0f);
                float rot1 = Random.Range(0.1f, 1.0f) * 1.2f;
                float rot2 = Random.Range(0.1f, 1.0f) * 1.2f;

                Vector3 d = mf.mesh.normals[i] - mf.mesh.bounds.center;
                if (d.sqrMagnitude > rad * rad)
                {

                }
                else
                {
                    spd = 0.0f;
                }

                for (int k = 0; k < 3; k++)
                {
                    colors[i + k].r = spd * 10.0f; // falling speed
                    colors[i + k].g = rot1;
                    colors[i + k].b = rot2;
                }

                i += Random.Range(0, 2) * 3;
            }
        }
        else
        {
            for (int i = 0; i < vc; i += 3)
            {
                float spd = Random.Range(0.4f, 3.0f);
                float rot1 = Random.Range(0.1f, 1.0f) * 1.2f;
                float rot2 = Random.Range(0.1f, 1.0f) * 1.2f;

                for (int k = 0; k < 3; k++)
                {
                    if(mf.mesh.colors[i + k].r > 0.0f)
                    {
                        spd *= 100000.0f;
                    }
                    colors[i + k].r = spd * 10.0f; // falling speed
                    colors[i + k].g = rot1;
                    colors[i + k].b = rot2;
                }
            }
        }

        mf.mesh.colors = colors;
    }

	void Update () {

        if (time >= 0.0f)
        {
            time += Time.deltaTime;

            float t = time / 100.0f;
            t = t * t;
            mat.SetFloat("_AnimationTime", t * 10000.0f);
        }
	}

    public void Reset()
    {
        mat.SetFloat("_AnimationTime", 0.0f);
        time = -1.0f;
    }
}
