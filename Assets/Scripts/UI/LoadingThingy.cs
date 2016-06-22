using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingThingy : WindowA {

    public Image indicator;
    public Text message;
    public Button cancelButton;

    public Sprite[] frames;

    public float framesPerSecond = 25.0f;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();

        indicator.sprite = frames[0];
        cancelButton.transform.position = new Vector3(Screen.width - 150, Screen.height - 150, 0);
        cancelButton.OnClick += OnCancelPressed;
    }


    void OnCancelPressed(MonoBehaviour sender, Vector2 pos)
    {
        Close();
    }

    float timer = 0.0f;
    int frame = 0;
	protected override void Update ()
    {
        base.Update();
        if (frames.Length == 0) return;

        timer += Time.deltaTime;

        if(timer >= 1.0f / framesPerSecond)
        {
            timer -= 1.0f / framesPerSecond;
            frame = (frame + 1) % frames.Length;
            indicator.sprite = frames[frame];
        }
	}
}
