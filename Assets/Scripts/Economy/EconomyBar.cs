using UnityEngine;
using System.Collections;

public class EconomyBar : Widget {

    public Widget currencyIcon;
    public Widget solutionsIcon;
    public Widget shopButton;

	// Use this for initialization
	protected override void Start () {
        base.Start();

        currencyIcon.OnClick += OnCurrencyIconClicked;
        solutionsIcon.OnClick += OnSolutionsIconClicked;
        shopButton.OnClick += OnShopButtonClicked;

        if (isOnShopScreen())
            shopButton.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();

	}

    void OnCurrencyIconClicked(MonoBehaviour sender, Vector2 pos)
    {
        WindowA par = GetComponentInParent<WindowA>();
        par.Close();
        WindowA.Create("UI/ShopScreen", "currency").Show();
    }

    void OnSolutionsIconClicked(MonoBehaviour sender, Vector2 pos)
    {
        ShowShopScreen();
    }

    void OnShopButtonClicked(MonoBehaviour sender, Vector2 pos)
    {
        ShowShopScreen();
    }

    void ShowShopScreen()
    {
        if (isOnShopScreen()) return;

        WindowA par = GetComponentInParent<WindowA>();
        par.Close();
        WindowA.Create("UI/ShopScreen").Show();
    }

    bool isOnShopScreen()
    {
        return GetComponentInParent<ShopScreen>();
    }
}
