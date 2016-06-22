using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SagaButton2 : Button {

    public int levelIndex = 0;

    RotateMe rotatingBorder;
	
    protected override void Start()
    {
        base.Start();

        rotatingBorder = GetComponentInChildren<RotateMe>();

        //highlight = false;

        Refresh();
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
	}

    public void Refresh()
    {
        if (caption != null)
            caption.text = (levelIndex + 1).ToString();
    }

    public bool highlight
    {
        get
        {
            if (rotatingBorder == null)
            {
                rotatingBorder = GetComponentInChildren<RotateMe>();
                if (rotatingBorder == null)
                    return false;
            }
            return rotatingBorder.enabled;
        }
        set
        {
            if(rotatingBorder == null)
                rotatingBorder = GetComponentInChildren<RotateMe>();
            if (rotatingBorder != null)
            {
                rotatingBorder.enabled = value;
            }
        }
    }
}
