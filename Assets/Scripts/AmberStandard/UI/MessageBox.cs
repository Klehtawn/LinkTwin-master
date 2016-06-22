using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class MessageBox : WindowA {

    public Widget okButton;
    public Text message;

    public Action OnDefaultButtonPressedAction;

    protected override void Awake()
    {
        base.Awake();
    }

	protected override void Start ()
    {
        base.Start();

        centered = true;

        //float a = aspect;

        //width = Desktop.main.width * 0.65f;
        //height = width / a;

        if(okButton != null)
            okButton.OnClick += OnDefaultButtonPressed;

        UpdateWindowParameters();
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
	}

    protected virtual void OnDefaultButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        if (OnDefaultButtonPressedAction != null)
            OnDefaultButtonPressedAction();
        Close();
    }
}
