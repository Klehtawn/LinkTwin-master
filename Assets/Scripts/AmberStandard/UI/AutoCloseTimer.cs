using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class AutoCloseTimer : MonoBehaviour, IPointerClickHandler {

    public float delay = 0.0f;
    public bool tapToClose = true;
    public float tapToCloseAfter = 0.0f;

    private bool cancelDelayedClose = false;
    private float timer, tapToCloseTimer;
	void Start ()
    {
        timer = delay;
        tapToCloseTimer = tapToCloseAfter;
	}

    void Update()
    {
        if (tapToCloseTimer > 0.0f)
            tapToCloseTimer -= Time.deltaTime;

        if(timer > 0.0f && cancelDelayedClose == false)
        {
            timer -= Time.deltaTime;
            if(timer < 0.0f)
            {
                Close();
            }
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (tapToClose && tapToCloseTimer <= 0.0f)
        {
            cancelDelayedClose = true;
            Close();
        }
    }

    private bool isClosing = false;
    void Close()
    {
        if (isClosing) return;

        WindowA w = GetComponent<WindowA>();
        if(w != null)
            w.Close();

        isClosing = true;
    }
}
