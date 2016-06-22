using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridCreator : MonoBehaviour {

    public int xSteps   = 5;
    public int zSteps = 5;

    public GameObject groundBlock;

    // Use this for initialization
    void Start ()
    {
        if (groundBlock == null)
            groundBlock = Resources.Load("GroundBlock") as GameObject;

        if (GetComponent<MeshRenderer>() != null)
            GetComponent<MeshRenderer>().enabled = false;

        Vector3 min = Vector3.one * 100000.0f;
        Vector3 max = -min;
        for(int i = 0; i < transform.childCount; i++)
        {
            min = Vector3.Min(min, transform.GetChild(i).position);
            max = Vector3.Max(min, transform.GetChild(i).position);
        }

        if(transform.childCount > 0)
        {
            xSteps = (int)((max.x - min.x) / GameSession.gridUnit) + 1;
            zSteps = (int)((max.z - min.z) / GameSession.gridUnit) + 1;
        }
        else
        {
            xSteps = zSteps = 0;
        }
    }

    public void CreateBlocks(int columns, int rows, Vector3 gridoffset)
    {
        if (groundBlock == null)
            groundBlock = Resources.Load("GroundBlock") as GameObject;

        transform.localScale = Vector3.one;
        transform.position = Vector3.zero;

        xSteps = Mathf.Min(25, columns);
        zSteps = Mathf.Min(25, rows);

        while(transform.childCount > 0)
        {
            GameObject obj = transform.GetChild(0).gameObject;
            obj.transform.parent = null;
            if (Application.isPlaying == false)
                DestroyImmediate(obj);
            else
                Destroy(obj);
        }

        for(int i = 0; i < xSteps * zSteps; i++)
        {
#if UNITY_EDITOR
            GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(groundBlock) as GameObject;
#else
            GameObject obj = GameObject.Instantiate(groundBlock);
#endif
            obj.transform.SetParent(transform);
        }

        Vector3 scale = Vector3.one;// *GameSession.gridUnit;

        Vector3 offset = new Vector3(-(float)xSteps, -1.0f, -(float)zSteps) * GameSession.gridUnit * 0.5f;
        //offset.x += (float)(xSteps % 2) * GameSession.gridUnit * 0.5f;
        //offset.z += (float)(zSteps % 2) * GameSession.gridUnit * 0.5f;

        for (int z = 0; z < zSteps; z++)
        {
            for (int x = 0; x < xSteps; x++)
            {
                GameObject block = transform.GetChild(x + z * xSteps).gameObject;
                //block.GetComponent<SnapToGrid>().enabled = false;
                
                //block.name = "GroundBlock_" + x.ToString() + "x" + z.ToString();
                block.transform.localScale = scale;
                Vector3 newPos = new Vector3(GameSession.gridUnit * ((float)x + 0.5f), 0.0f, GameSession.gridUnit * ((float)z + 0.5f)) + offset + gridoffset;
                block.transform.localPosition = newPos;
            }
        }
    }

    public void GenerateBorders()
    {
    }

    void Update()
    {
        transform.position = Vector3.zero;
    }

    public static Vector3 offset(int cols, int rows)
    {
        return new Vector3(.5f * (cols % 2), 0, .5f * (rows % 2));
    }
}