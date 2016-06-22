using UnityEngine;
using System.Collections;

public class WorkshopLevelCompleteBox : MessageBox {

    public Button backButton;
    public Button uploadButton;

    public enum ReturnValues
    {
        PressedBack,
        PressedUpload
    }
	// Use this for initialization
    protected override void Start()
    {
        base.Start();

        UpdateWindowParameters();

        backButton.OnTouchUp += OnBackButtonPressed;
        uploadButton.OnTouchUp += OnUploadButtonPressed;
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
	}

    void OnBackButtonPressed(MonoBehaviour sender, Vector2 p)
    {
        Close((int)ReturnValues.PressedBack);
    }

    void OnUploadButtonPressed(MonoBehaviour sender, Vector2 p)
    {

        Close((int)ReturnValues.PressedUpload);
    }
}
