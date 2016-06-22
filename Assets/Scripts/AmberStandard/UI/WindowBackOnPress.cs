using UnityEngine;
using System.Collections;

public class WindowBackOnPress : MonoBehaviour {

    void Start()
    {
        Widget w = GetComponent<Widget>();
        if (w != null)
            w.OnTouchUp += OnButtonPress;
    }

    void Update()
    {
        if (GameSession.BackKeyPressed())
            Desktop.main.windowsFlow.Backward();

        transform.SetAsLastSibling();
    }

    void OnButtonPress(MonoBehaviour sender, Vector2 p)
    {
        Desktop.main.windowsFlow.Backward();
    }
}
