using UnityEngine;
using System.Collections;

public class EconomyBarModal : WindowA {

	// Use this for initialization
	protected override void Start () {
        base.Start();
	}

    static WindowA bar = null;
    public static void Show(WindowA parent)
    {
        if (bar != null) return;

        bar = WindowA.Create("UI/EconomyBarModal").Show();
        bar.transform.SetParent(parent.transform.parent);
        parent.OnWindowStartClosing += OnParentStartClosing;
    }

    static void OnParentStartClosing(WindowA sender, int ret)
    {
        bar.Close();
        bar = null;
    }
}
