using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SagaButton : SimpleWidget {

    public int levelIndex = 0;

    public Text caption;

	// Use this for initialization
    protected override void Start()
    {
        base.Start();

        if(caption != null)
            caption.text = levelIndex.ToString();
	}
	
	// Update is called once per frame
    protected override void Update()
    {

        base.Update();
	}
}
