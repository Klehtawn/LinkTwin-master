using UnityEngine;
using System.Collections;

[SelectionBase]
public class Spawner : Block {

    public GameObject playerToSpawn;

    public Transform editorObject, ingameObject;

	// Use this for initialization

    public override void Awake()
    {
        base.Awake();
        blockType = BlockType.Spawn;
        ignoreOnGamePlay = true;
    }

	public override void Start () {
        base.Start();
        editorObject.gameObject.SetActive(false);
        //ingameObject.gameObject.SetActive(true);
	}

    // Update is called once per frame
    public override void Update ()
    {
        base.Update();
	}
}
