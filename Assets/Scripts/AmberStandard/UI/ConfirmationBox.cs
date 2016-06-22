using UnityEngine;
using System.Collections;
using System;

public class ConfirmationBox : MessageBox {

    public Widget cancelButton;

    public Action OnCancelButtonPressedAction;

	// Use this for initialization
	protected override void Start () {
        base.Start();

        cancelButton.OnClick += OnCancelPressed;
	}

    protected override void Update()
    {
        base.Update();

        if (GameSession.BackKeyPressed() && isClosingTimer < 0 && finishedShowing)
        {
            if (OnCancelButtonPressedAction != null)
                OnCancelButtonPressedAction();
            Close();
        }
    }

    void OnCancelPressed(MonoBehaviour sender, Vector2 pos)
    {
        if (OnCancelButtonPressedAction != null)
            OnCancelButtonPressedAction();

        Close();
    }

    public static void Show(string msg, Action onOk = null, Action onCancel = null, string buttons = "ok,cancel")
    {       
        WindowA w = WindowA.Create("UI/ConfirmationBox");
        
        ConfirmationBox cb = w.GetComponent<ConfirmationBox>();

        string[] bs = buttons.Split(',');
        cb.okButton.gameObject.SetActive(false);
        cb.cancelButton.gameObject.SetActive(false);
        foreach(string s in bs)
        {
            if(s == "ok")
                cb.okButton.gameObject.SetActive(true);

            if (s == "cancel")
                cb.cancelButton.gameObject.SetActive(true);
        }

        cb.message.text = msg;

        if(onOk != null)
            cb.OnDefaultButtonPressedAction += onOk;

        if(onCancel != null)
            cb.OnCancelButtonPressedAction += onCancel;

        w.ShowModal();
    }
}
