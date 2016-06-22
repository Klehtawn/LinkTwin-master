using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PayWhatYouWantWidget : Widget {

    public Widget unlockAllChaptersWidget;
    public Text message;

    public Widget bar;
    public RectTransform dot;

    public Widget removeAdsButton;

    private float payIndex = 0.0f;

    public Transform prices;

    int pricesCount;

	// Use this for initialization

    protected override void Awake()
    {
        base.Awake();
        OnDonationChanged();
    }

	protected override void Start () {
        base.Start();

        bar.OnClick += OnBarClicked;
        bar.OnTouchMoved += OnBarDragged;
        removeAdsButton.OnClick += OnRemoveAdsClicked;
        unlockAllChaptersWidget.OnClick += OnUnlockAllClicked;

        pricesCount = prices.GetComponentsInChildren<PayWhatYouWantPrice>().Length;

        FillDonations();
	}

    void OnBarDragged(MonoBehaviour sender, Vector2 start, Vector2 pos)
    {
        Widget w = sender.GetComponent<Widget>();
        UpdateBar(w, Desktop.main.DesktopToScreen(pos) + w.screenPos);
    }

    void UpdateBar(Widget w, Vector2 pos)
    {
        float f = Mathf.Clamp(Mathf.Round(4.0f * Desktop.main.ScreenToDesktopRelative(pos - w.screenPos).x / w.width), 0.0f, pricesCount - 1.0f);

        if (f != payIndex)
        {
            payIndex = f;
            OnDonationChanged();
        }
    }

    void OnBarClicked(MonoBehaviour sender, Vector2 pos)
    {
        Widget w = sender.GetComponent<Widget>();
        UpdateBar(w, pos);
    }

    void OnRemoveAdsClicked(MonoBehaviour sender, Vector2 pos)
    {
        PayWhatYouWantPrice[] allPrices = prices.GetComponentsInChildren<PayWhatYouWantPrice>();
        GameEconomy.Buy(allPrices[(int)payIndex].shopItem);
    }

    void OnUnlockAllClicked(MonoBehaviour sender, Vector2 pos)
    {
        GameEconomy.Buy(GameEconomy.GetEconomyItemById(GameEconomy.GetFullItemId("unlock.chapters")));
    }

    void OnDonationChanged()
    {
        PayWhatYouWantPrice[] allPrices = prices.GetComponentsInChildren<PayWhatYouWantPrice>();
        for(int i = 0; i < allPrices.Length; i++)
        {
            Widget w = allPrices[i].GetComponent<Widget>();
            w.active = i == (int)payIndex;
        }
    }

    void FillDonations()
    {
        List<GameEconomy.EconomyItem> items = GameEconomy.GetItemsForSaleCategorized(GameEconomy.EconomyItemType.Donation);
        PayWhatYouWantPrice[] allPrices = prices.GetComponentsInChildren<PayWhatYouWantPrice>();

        int c = Mathf.Min(items.Count, allPrices.Length);
        for(int i = 0; i < c; i++)
        {
            allPrices[i].shopItem = items[i];
        }
    }
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();

        float anch = Mathf.Clamp(payIndex * 0.25f, 0.0f, 1.0f);

        float a = Mathf.Lerp(dot.anchorMin.x, anch, 9.0f * Time.deltaTime);

        dot.anchorMin = new Vector2(a, 0.5f);
        dot.anchorMax = dot.anchorMin;
	}
}
