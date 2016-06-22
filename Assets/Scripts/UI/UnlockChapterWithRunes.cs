using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class UnlockChapterWithRunes : MessageBox {

    public Widget cancelButton;

    public Action OnCancelButtonPressedAction;

    public Text priceText;

    private int _price = 10;
    public int price
    {
        get
        {
            return _price;
        }

        set
        {
            if (_price != value)
            {
                _price = value;
                priceText.text = _price.ToString();
            }
        }
    }

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

    public static void Show(string msg, int price, Action onOk = null, Action onCancel = null, string buttons = "ok,cancel")
    {
        WindowA w = WindowA.Create("UI/UnlockChapterWithRunes");

        UnlockChapterWithRunes ucr = w.GetComponent<UnlockChapterWithRunes>();
        ucr.price = price;

        string[] bs = buttons.Split(',');
        ucr.okButton.gameObject.SetActive(false);
        ucr.cancelButton.gameObject.SetActive(false);
        foreach(string s in bs)
        {
            if(s == "ok")
                ucr.okButton.gameObject.SetActive(true);

            if (s == "cancel")
                ucr.cancelButton.gameObject.SetActive(true);
        }

        ucr.message.text = msg;

        if(onOk != null)
            ucr.OnDefaultButtonPressedAction += onOk;

        if(onCancel != null)
            ucr.OnCancelButtonPressedAction += onCancel;

        w.ShowModal();
    }
}
