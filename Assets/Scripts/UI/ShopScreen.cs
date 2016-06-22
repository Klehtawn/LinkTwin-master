using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ShopScreen : GameScreen {

    private RectTransform itemsContainer;
    private TabbedWidget tabs;

    public ShopItemBase itemSingleLine;
    public ShopItemBase itemTwoLines;
    public ShopItemBase itemMultipleObjects;

    public Widget backButton;

    int fillFrames = 3;

    public Sprite donationsIcon;
    public GameObject donationsWidget;
	protected override void Start () {
        base.Start();

        title = "Shop";

        //itemsContainer = transform.Find("Contents/Items").GetComponent<RectTransform>();
        itemsContainer = null;

        tabs = GetComponentInChildren<TabbedWidget>();

        GameEconomy.Initialize();

        GameEconomy.OnItemBought += OnItemBought;
        //GameEconomy.OnCurrencyBought += OnCurrencyBought;

        backButton.OnClick += OnBackButtonClicked;

        RegisterEvents();
        OnWindowFinishedShowing += OnWindowShown;

        GameEconomy.RetrieveProductList();
	}

    void OnDestroy()
    {
        GameEconomy.OnItemBought -= OnItemBought;
        //GameEconomy.OnCurrencyBought -= OnCurrencyBought;
    }
	
	protected override void Update () {
        base.Update();

        if(fillFrames > 0)
        {
            fillFrames--;
            if(fillFrames == 0)
                Fill();
        }

        if (GameSession.BackKeyPressed())
            OnBackButtonClicked(this, Vector2.zero);
    }

    void Fill()
    {
        if (itemsContainer != null)
            Widget.DeleteAllChildren(itemsContainer);

        AddTab(GameEconomy.EconomyItemType.Solutions);
        AddTab(GameEconomy.EconomyItemType.Bundle);
        AddTab(GameEconomy.EconomyItemType.Runes);
        AddTab(GameEconomy.EconomyItemType.GenericPowerup, true, "POWERUPS");

        tabs.AddTab("", donationsIcon, GameObject.Instantiate<GameObject>(donationsWidget));

        if(tabToActivate >= 0)
            tabs.ActivateTab(tabToActivate);
    }

    void OnWindowShown(WindowA sender)
    {
        PlayerPrefs.SetInt("ShopWindowShown", PlayerPrefs.GetInt("ShowWindowShown", 0) + 1);
    }

    void AddTab(GameEconomy.EconomyItemType type, bool isBaseType = false, string title = null)
    {
        GameObject srcPrefab = Resources.Load<GameObject>("UI/ShopCategory");

        List<GameEconomy.EconomyItem> items = GameEconomy.GetItemsForSale(type);
        if(isBaseType)
            items = GameEconomy.GetItemsForSaleCategorized(type);

        if(items.Count > 0)
        {
            GameObject obj = GameObject.Instantiate<GameObject>(srcPrefab);

            tabs.AddTab("", GameEconomy.GetSpriteForItem(type), obj);

            obj.transform.localScale = Vector3.one;
            obj.name = type.ToString();

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.0f, 0.0f);
            rt.anchorMax = new Vector2(1.0f, 1.0f);
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            ShopCategory sc = obj.GetComponent<ShopCategory>();

            if (type == GameEconomy.EconomyItemType.Solutions)
            {
                sc.numColumns = 1;
                sc.itemPrefab = itemSingleLine;
                sc.showTotalCurrency = false;
            }

            if (type == GameEconomy.EconomyItemType.Runes)
            {
                sc.numColumns = 1;
                sc.itemPrefab = itemSingleLine;
                sc.showTotalCurrency = true;
            }

            if (type == GameEconomy.EconomyItemType.PowerupFreezeCharacter)
            {
                sc.numColumns = 2;
                sc.itemPrefab = itemTwoLines;
                sc.showTotalCurrency = true;
                sc.extendItemSize = false;
            }

            if (type == GameEconomy.EconomyItemType.Bundle)
            {
                sc.numColumns = 2;
                sc.itemPrefab = itemMultipleObjects;
                sc.showTotalCurrency = false;
                sc.extendItemSize = false;
            }

            sc.Fill(type, isBaseType);
            sc.OnItemClicked += OnShopItemClicked;
            sc.dynamicHeightUpdate = false;

            if(title != null)
                sc.title.GetComponent<LocalizedText>().text = title;
        }
    }
    
    private GameEconomy.EconomyItem itemToBuy = null;
    void OnShopItemClicked(GameEconomy.EconomyItem ei)
    {
        if (ei.softCurrency)
            GameEconomy.ShowBuyItemWithSoftCurrencyBox(ei);
        else
            GameEconomy.Buy(ei);
    }

    void OnCurrencyBought(int ammount)
    {
        OnShopItemClicked(itemToBuy);
    }

    public bool closeOnBuy = false;

    private int tabToActivate = -1;
    private bool isOverlay = false;
    protected override void HandleCreateArgs(string args)
    {
        isOverlay = false;

        if(args != null)
        {
            string[] _args = args.Split(',');

            foreach (string a in _args)
            {
                if (a == "solutions")
                {
                    tabToActivate = 0;
                }

                if (a == "currency")
                {
                    tabToActivate = 2;
                }

                if (a == "powerups")
                {
                    tabToActivate = 3;
                }

                if(a == "overlay")
                {
                    isOverlay = true;
                }
            }

            closeOnBuy = false;
        }
    }

    void OnItemBought(GameEconomy.EconomyItem i)
    {
        if(closeOnBuy)
        {
            Close();
            Desktop.main.windowsFlow.Backward();
        }
    }

    static bool eventsRegistered = false;
    public static void RegisterEvents()
    {
        if (eventsRegistered) return;

        eventsRegistered = true;

        GameEconomy.OnNotEnoughCurrency += OnNotEnoughCurrency;
    }

    static void OnNotEnoughCurrency()
    {
        ShopScreen ss = Desktop.main.topmostWindow.GetComponent<ShopScreen>();
        if (ss != null)
        {
            ConfirmationBox.Show(Locale.GetString("NOT_ENOUGH_CURRENCY"), () =>
                {
                    ss.tabs.ActivateTab(2);
                }, null, "ok");
        }
    }

    void OnBackButtonClicked(MonoBehaviour sender, Vector2 pos)
    {
        if (isOverlay == false)
            Desktop.main.windowsFlow.Backward();
        else
        {
            Desktop.main.windowsFlow.Backward(0, false);
            if(Desktop.main.minimizedWindows.Count > 0)
            {
                Desktop.main.minimizedWindows[Desktop.main.minimizedWindows.Count - 1].Show();
            }
        }
    }
}