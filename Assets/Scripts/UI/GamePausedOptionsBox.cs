using UnityEngine;
using System.Collections;

public class GamePausedOptionsBox : MessageBox {

    public SettingsOnOff musicButton;
    public SettingsOnOff sfxButton;
	
	protected override void Start () {

	    base.Start();

        //height = height * 1.6f;

        UpdateWindowParameters();
    }
	
	// Update is called once per frame
	protected override void Update () {

        base.Update();
	
	}

    protected override void OnDefaultButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Desktop.main.modalWindowsFlow.Backward();
    }
}
