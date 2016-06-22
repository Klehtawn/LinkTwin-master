using UnityEngine;

public class LevelRoot : MonoBehaviour
{
    public TableDescription tableDescription;

    private GameObject table;
    private GridCreator ground;

    public GameObject Table
    {
        get { return table; }
    }

    public GridCreator Ground
    {
        get { return ground; }
    }

    public void DestroyStructure()
    {
        if (table == null || ground == null) return;

        Widget.DeleteAllChildren(table.transform);
        Widget.DeleteAllChildren(ground.transform);

        if (Application.isPlaying)
        {
            Destroy(table);
            Destroy(ground.gameObject);
        }
        else
        {
            DestroyImmediate(table);
            DestroyImmediate(ground.gameObject);
        }

        table = null;
        ground = null;

        blocks.Clear();
    }

    public void CreateStructure()
    {
        DestroyStructure();

        table = new GameObject("Table");
        ground = new GameObject("Ground", typeof(GridCreator)).GetComponent<GridCreator>();
        table.transform.SetParent(transform);
        ground.transform.SetParent(transform);

        blocks.Clear();
    }

    public void InitStructure()
    {
        if (table == null)
        {
            Transform t = transform.FindChild("Table");
            if (t != null)
                table = t.gameObject;
        }

        if (ground == null)
        {
            ground = transform.GetComponentInChildren<GridCreator>();
        }
    }

    public Blocks blocks = new Blocks();

    public void RefreshStructure()
    {
        blocks.Fill(transform);
    }

    #region create/destroy root helpers
    public static LevelRoot CreateRoot(string name)
    {
        LevelRoot root = new GameObject(name, typeof(LevelRoot)).GetComponent<LevelRoot>();
        root.CreateStructure();
        return root;
    }

    public static void DestroyRoot(ref LevelRoot root)
    {
        if (Application.isPlaying)
            Destroy(root.gameObject);
        else
            DestroyImmediate(root.gameObject);
    }
    #endregion
}
