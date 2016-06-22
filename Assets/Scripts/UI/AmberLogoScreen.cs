using UnityEngine;
using System.Collections;

public class AmberLogoScreen : GameScreen {

    DissolveImage[] dissolveImages;
	protected override void Start () {
        base.Start();

        dissolveImages = GetComponentsInChildren<DissolveImage>();

        for (int i = 0; i < dissolveImages.Length; i++)
        {
            dissolveImages[i].SetFrom(0.0f, 1.0f);
            dissolveImages[i].gameObject.SetActive(false);
        }

        StartCoroutine(ActivateDissolve());

        OnWindowStartClosing += OnScreenStartClosing;

        Transform bkg = transform.Find("Blackness");
        myTween.Instance.SpriteAlphaFade(bkg.gameObject, 0.0f, 1.5f, 0.5f);
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
        return;
        for (int i = 0; i < dissolveImages.Length; i++)
        {
            dissolveImages[i].SetFrom(1.0f, 0.0f);
        }
    }
}
