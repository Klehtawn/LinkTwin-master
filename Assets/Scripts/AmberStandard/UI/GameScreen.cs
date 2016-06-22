using UnityEngine;
using System.Collections;

public class GameScreen : WindowA {

    public bool supportsWindowFlow = true;

    public GameObject[] autoLoad;

    protected override void Awake()
    {
        base.Awake();
        fitToParent = true;
    }


    protected override void Start()
    {
        base.Start();
        if(supportsWindowFlow)
            Desktop.main.windowsFlow.Push(this);

        if(autoLoad != null && autoLoad.Length > 0)
        {
            foreach(GameObject o in autoLoad)
            {
                GameObject obj = GameObject.Instantiate<GameObject>(o);
                obj.name = o.name;
                obj.transform.SetParent(transform);

                RectTransform dstRect = obj.GetComponent<RectTransform>();
                RectTransform srcRect = o.GetComponent<RectTransform>();

                dstRect.anchoredPosition = srcRect.anchoredPosition;
                dstRect.sizeDelta = srcRect.sizeDelta;
                obj.transform.localScale = Vector3.one;
            }
        }
    }
}
