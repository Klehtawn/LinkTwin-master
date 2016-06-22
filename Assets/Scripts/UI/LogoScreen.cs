using UnityEngine;
using System.Collections;

public class LogoScreen : GameScreen {

    DissolveImage[] dissolveImages;
	protected override void Start () {
        base.Start();

        dissolveImages = GetComponentsInChildren<DissolveImage>();
        for (int i = 0; i < dissolveImages.Length; i++)
        {
            dissolveImages[i].SetFrom(0.0f, 1.0f);
            //dissolveImage.gameObject.SetActive(false);
        }

        //StartCoroutine(ActivateDissolve());

        OnWindowStartClosing += OnScreenStartClosing;
	}

    IEnumerator ActivateDissolve()
    {
        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < dissolveImages.Length; i++)
        {
            dissolveImages[i].gameObject.SetActive(true);
        }
    }

    void OnScreenStartClosing(WindowA sender, int ret)
    {
        for (int i = 0; i < dissolveImages.Length; i++)
        {
            //dissolveImages[i].SetFrom(1.0f, 0.0f);
        }
    }
}
