using UnityEngine;
using System.Collections;

[SelectionBase]
[ExecuteInEditMode]
public class Wall : Block {

    Transform wallLeft;
    Transform wallRight;
    Transform wallTop;
    Transform wallBottom;

    public static float yOffset = 0.0f;
	public override void Start () {

        base.Start();

        wallLeft = transform.Find("Left");
        wallRight = transform.Find("Right");
        wallTop = transform.Find("Top");
        wallBottom = transform.Find("Bottom");

        wallLeft.gameObject.SetActive(true);
        wallRight.gameObject.SetActive(true);
        wallTop.gameObject.SetActive(true);
        wallBottom.gameObject.SetActive(true);

        CheckNeighbours();
	}
	
	public override void Update ()
    {
        base.Update();

        if (hasMoved)
            Refresh();
    }

    public override void Refresh()
    {
        CheckNeighbours();
    }

    void CheckCompleteness()
    {
        /*if (wallLeft.gameObject.activeSelf == false && wallRight.gameObject.activeSelf == false && wallTop.gameObject.activeSelf == false && wallBottom.gameObject.activeSelf == false)
        {
            //gameObject.SetActive(false);
        }*/
    }

    void CheckNeighbours()
    {
        if (root != null)
        {
            Block n = GetNeighbour(Vector3.left);
            wallLeft.gameObject.SetActive(n != null && n.blockType != BlockType.Wall);

            n = GetNeighbour(Vector3.right);
            wallRight.gameObject.SetActive(n != null && n.blockType != BlockType.Wall);

            n = GetNeighbour(Vector3.forward);
            wallTop.gameObject.SetActive(n != null && n.blockType != BlockType.Wall);

            n = GetNeighbour(Vector3.back);
            wallBottom.gameObject.SetActive(n != null && n.blockType != BlockType.Wall);

            CheckCompleteness();
        }
    }

    public override void OnGamePreStart()
    {
        base.OnGamePreStart();

        getRoot();

        wallLeft = transform.Find("Left");
        wallRight = transform.Find("Right");
        wallTop = transform.Find("Top");
        wallBottom = transform.Find("Bottom");

        wallLeft.gameObject.SetActive(true);
        wallRight.gameObject.SetActive(true);
        wallTop.gameObject.SetActive(true);
        wallBottom.gameObject.SetActive(true);

        CheckNeighbours();
    }
}
