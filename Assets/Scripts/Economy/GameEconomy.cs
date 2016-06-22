using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;

public class GameEconomy
{
    public enum EconomyItemType
    {
        Invalid, // do not use
        Solutions,
        Runes,
        Levels,
        Characters,
        PowerupFreezeCharacter,
        PowerupPlaceATile,
        Bundle,
        GenericPowerup,
        Donation,
        Donation1,
        Donation2,
        Donation3,
        Donation4,
        Donation5,
        UnlockAll,
        ChapterUnlock
    }

    public class EconomyItem
    {
        public int ammount = 1;
        public EconomyItemType category;
        public EconomyItemType type;
        public string name = "";
        public string iconName = "";
        public float price;
        public string priceStr = "";
        public bool softCurrency;
        public string displayName = "";
        public string args = "";
        public string currencyCode = "";
        // and others

        public string shopId = "";
        public EconomyItemType[] bundleItems;
        public int[] bundleAmmounts;
    }

    public class CurrencyItem
    {
        public int ammount = 0;
        public string sProductId = "";

        public float sPrice = 0.0f;
        public string sProductName;
        public string sProductDesc;
        public string sProductFormattedPrice;
    }

    static List<ShopIcon> shopIcons = null;

    static InAppPurchase iap = null;
    static BuyCurrencyBox bcb = null;

    static Queue<DateTime> videoAdTimes = new Queue<DateTime>();
    const string kVideoAdTimesKey = "VideoAdTimes";

    static bool bInitialized = false;

    public static void Initialize()
    {
        if (bInitialized) return;

        LoadItemsForSale();
        //LoadCurrencyForSale();
        //RetrieveProductList();
        shopIcons = null;
        GameObject o = Resources.Load<GameObject>("shopicons");

        if (shopIcons == null)
            shopIcons = new List<ShopIcon>();
        shopIcons.Clear();
        if (o != null)
        {
            for (int i = 0; i < o.transform.childCount; i++ )
            {
                ShopIcon si = o.transform.GetChild(i).GetComponent<ShopIcon>();
                if(si != null)
                    shopIcons.Add(si);
            }
        }

        bInitialized = true;
    }

    public static void RetrieveProductList()
    {
        if (iap == null)
        {
#if UNITY_IOS && !UNITY_EDITOR
            iap = new InAppPurchaseIOS();
#elif UNITY_ANDROID && !UNITY_EDITOR
            //iap = new InAppPurchaseAndroid();
            iap = new InAppPurchaseSamsung();
#elif (UNITY_METRO || UNITY_WP8) && !UNITY_EDITOR
            iap = new InAppPurchaseWindows();
#else
            iap = new InAppPurchase();
#endif
        }
        Debug.Log("GameEconomy RetrieveProductList");
        iap.RetrieveProductList();
    }

    public static void Sync()
    {

    }

    public static void SaveLocal()
    {
        PlayerPrefs.SetInt("Currency", currency);
        foreach (EconomyItemType type in Enum.GetValues(typeof(EconomyItemType)))
            if (boughtItems.ContainsKey(type))
                PlayerPrefs.SetInt("BoughtItem" + type, boughtItems[type].ammount);
    }

    private static bool localLoaded = false;
    public static void LoadLocal()
    {
        if (localLoaded) return;

        currency = PlayerPrefs.GetInt("Currency", 100);
        foreach (EconomyItemType type in Enum.GetValues(typeof(EconomyItemType)))
        {
            string key = "BoughtItem" + type;
            if (PlayerPrefs.HasKey(key))
            {
                EconomyItem item = new EconomyItem();
                item.ammount = PlayerPrefs.GetInt(key);
                item.type = type;
                boughtItems.Add(type, item);
            }
        }

        localLoaded = true;

        LoadVideoAdTimes();
    }

    static int _currency = 100;
    static public int currency
    {
        get
        {
            return _currency;
        }
        set
        {
            _currency = value;
        }
    }

    static Dictionary<EconomyItemType, EconomyItem> boughtItems = new Dictionary<EconomyItemType, EconomyItem>();

    public static EconomyItem bought(EconomyItemType type)
    {
        if (boughtItems.ContainsKey(type))
            return boughtItems[type];
        return null;
    }

    public static int boughtAmmount(EconomyItemType type)
    {
        if (boughtItems.ContainsKey(type))
            return boughtItems[type].ammount;
        return 0;
    }

    static public Sprite GetSpriteForItem(GameEconomy.EconomyItem it)
    {
        return GetSpriteForItem(it.type, it.iconName);
    }

    static public Sprite GetSpriteForItem(GameEconomy.EconomyItemType t, string iconName = null)
    {
        if(shopIcons == null)
        {
            return null;
        }
        
        Sprite defaultSprite = null;
        foreach(ShopIcon si in shopIcons)
        {
            if(si.type == EconomyItemType.Invalid)
            {
                defaultSprite = si.sprite;
            }

            if(iconName != null && iconName.Length > 0 && si.type == t)
            {
                return si.variant(iconName);
            }
            else
            if (si.type == t)
                return si.sprite;
        }

        return defaultSprite;
    }

    static List<EconomyItem> forSaleItems = new List<EconomyItem>();

    private static string GetAttribute(XmlNode node, string name)
    {
        XmlNode attr = node.Attributes.GetNamedItem(name);
        if (attr != null)
            return attr.InnerText;
        return null;
    }

    static void LoadItemsForSale()
    {
        forSaleItems.Clear();

        XmlDocument doc = new XmlDocument();
        string text = ContentManager.LoadTextFile("Shop/forsale");
        if (text == null) return;
        doc.LoadXml(text);

        // second node, should be "Items"
        XmlNode root = doc.ChildNodes[1];
        for(int i = 0; i < root.ChildNodes.Count; i++)
        {
            XmlNode child = root.ChildNodes[i];

            if (child.Name == "Currency") continue;

            EconomyItem ei = new EconomyItem();

            string typeStr = GetAttribute(child, "type");

            try
            {
                ei.type = (EconomyItemType)Enum.Parse(typeof(EconomyItemType), typeStr, true);
            }
            catch(ArgumentException)
            {
                Debug.LogError("Invalid sale item! [" + typeStr + "]");
                continue;
            }

            try
            {
                ei.category = (EconomyItemType)Enum.Parse(typeof(EconomyItemType), child.Name, true);
            }
            catch (ArgumentException)
            {
                Debug.LogError("Invalid base type! [" + child.Name + "]");
                ei.category = EconomyItemType.Invalid;

                //continue;
            }

            ei.ammount = int.Parse(GetAttribute(child, "ammount"));

            ei.shopId = GetFullItemId(GetAttribute(child, "shopId"));

            ei.name = GetAttribute(child, "name");
            if (ei.name == null)
                ei.name = typeStr;
            ei.iconName = GetAttribute(child, "icon");

            string priceStr = GetAttribute(child, "price");
            ei.softCurrency = priceStr.StartsWith("soft:");
            if (ei.softCurrency)
                priceStr = priceStr.Remove(0, 5);
            
            ei.price = float.Parse(priceStr);

            if (ei.softCurrency == false)
                ei.priceStr = ei.price + " USD";
            else
                ei.priceStr = ei.price.ToString();

            ei.args = GetAttribute(child, "args");

            string displayName = GetAttribute(child, "displayname");
            if (displayName != null)
                ei.displayName = Locale.GetString(displayName);
            else
                ei.displayName = ei.name;

            string bundleItemsStr = GetAttribute(child, "bundle");
            if (bundleItemsStr != null)
            {
                string[] bundles = bundleItemsStr.Split(',');
                int bundlesCount = bundles.Length / 2;
                if(bundlesCount > 0)
                {
                    ei.bundleItems = new EconomyItemType[bundlesCount];
                    ei.bundleAmmounts = new int[bundlesCount];

                    for(int k = 0; k < bundlesCount; k++)
                    {
                        bundles[k * 2] = bundles[k * 2].TrimStart(' ');
                        bundles[k * 2 + 1] = bundles[k * 2 + 1].TrimStart(' ');
                        try
                        {
                            ei.bundleItems[k] = (EconomyItemType)Enum.Parse(typeof(EconomyItemType), bundles[k * 2 + 1], true);
                        }
                        catch (ArgumentException)
                        {
                            Debug.LogError("Invalid sale item! [" + bundles[k * 2 + 1] + "]");
                            continue;
                        }
                        ei.bundleAmmounts[k] = int.Parse(bundles[k * 2]);
                    }
                }
            }

            forSaleItems.Add(ei);
        }
    }

    static List<CurrencyItem> forSaleCurrency = new List<CurrencyItem>();

    static void LoadCurrencyForSale()
    {
        forSaleCurrency.Clear();

        XmlDocument doc = new XmlDocument();
        string text = ContentManager.LoadTextFile("Shop/forsale");
        if (text == null) return;
        doc.LoadXml(text);


        // second node, should be "Items"
        XmlNode root = doc.ChildNodes[1];
        for (int i = 0; i < root.ChildNodes.Count; i++)
        {
            XmlNode child = root.ChildNodes[i];

            if (child.Name != "Currency") continue;

            CurrencyItem ci = new CurrencyItem();
            ci.sPrice = int.Parse(GetAttribute(child, "price"));
            ci.ammount = int.Parse(GetAttribute(child, "ammount"));
            ci.sProductFormattedPrice = "$" + ci.sPrice.ToString();
            ci.sProductName = ci.ammount.ToString() + "x";
#if UNITY_IOS
            ci.sProductId = GetAttribute(child, "iOSId");
#elif UNITY_ANDROID
            ci.sProductId = GetAttribute(child, "androidId");
            Debug.Log("ci.sProductId=" + ci.sProductId);
#elif UNITY_METRO || UNITY_WP8
            ci.sProductId = GetAttribute(child, "metroId");
#endif
            forSaleCurrency.Add(ci);
        }
    }

    static public List<EconomyItem> GetItemsForSale(EconomyItemType t)
    {
        List<EconomyItem> res = new List<EconomyItem>();

        foreach(EconomyItem ei in forSaleItems)
        {
            if (ei.type == t)
                res.Add(ei);
        }

        return res;
    }

    static public List<EconomyItem> GetItemsForSaleCategorized(EconomyItemType category)
    {
        List<EconomyItem> res = new List<EconomyItem>();

        foreach (EconomyItem ei in forSaleItems)
        {
            if (ei.category == category)
                res.Add(ei);
        }

        return res;
    }


    static public Action<EconomyItem> OnItemBought;

    static EconomyItem boughtItem = null;
    public static void Buy(EconomyItem ei)
    {
        if (ei.softCurrency == false)
        {
            iap.PurchaseProduct(ei.shopId);
#if UNITY_EDITOR
            if (iap.GetType() == typeof(InAppPurchase))
                OnItemPurchaseComplete(ei);
#endif
        }
        else
        {
            if (ConsumeCurrency((int)ei.price))
            {
                currency -= (int)ei.price;
                FinalizeItemBought(ei);
            }
        }
    }

    public static void FinalizeItemBought(EconomyItem ei)
    {
        if(ei.type == EconomyItemType.Runes)
        {
            currency += ei.ammount;
        }

        if (boughtItems.ContainsKey(ei.type) == false)
        {
            EconomyItem item = new EconomyItem();
            item.ammount = 0;
            item.type = ei.type;
            boughtItems.Add(ei.type, item);
        }

        boughtItems[ei.type].ammount += ei.ammount;

        boughtItem = ei;

        SaveLocal();

        //DesktopUtils.ShowLocalizedMessageBox("ITEM_PURCHASED", OnEconomyItemPurchased);
        if (ei.softCurrency)
        {

        }
        else
        {
        }

        DesktopUtils.ShowLocalizedMessageBox("ITEM_PURCHASED", OnEconomyItemPurchased);
    }

    public static void OnItemPurchaseComplete(EconomyItem ei)
    {
        ReportIapResult(ei, "Success", "");
        FinalizeItemBought(ei);
    }

    public static void OnItemPurchaseCanceled(EconomyItem ei, string errorInfo)
    {
        ReportIapResult(ei, "Cancel", errorInfo);
    }

    public static void OnItemPurchaseError(EconomyItem ei, string errorInfo)
    {
        ReportIapResult(ei, "Failed", errorInfo);
    }

    private static void ReportIapResult(EconomyItem ei, string result, string errorInfo)
    {
        GameSparksManager.Instance.RecordAnalytics("IAP_RESULT",
            "Connection_Type", Application.internetReachability.ToString(),
            "Max_Chapter", GameSession.GetMaxUnlockedChapter(),
            "Max_Level", GameSession.GetMaxUnlockedLevel(),
            "Last_Map", GameSession.lastPlayedLevel,
            "Object_ID", ei.shopId,
            "Money_Currency", ei.currencyCode,
            "Money_IAP", ei.price,
            "PStore_Visits", PlayerPrefs.GetInt("ShowWindowShown", 0),
            "Result", result,
            "Error", errorInfo
            );
    }

    public static EconomyItem GetEconomyItemById(string id)
    {
        foreach (EconomyItem ei in forSaleItems)
            if (ei.shopId == id)
                return ei;
        return null;
    }

    static void OnEconomyItemPurchased()
    {
        if (OnItemBought != null)
            OnItemBought(boughtItem);

        boughtItem = null;
    }

    static public Action<int> OnCurrencyBought;

    public static void BuyCurrency(CurrencyItem ci)
    {
#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS && !UNITY_METRO && !UNITY_WP8)
        OnCurrencyItemPurchased(ci.sProductId, true);
#else
        iap.PurchaseProduct(ci.sProductId);
#endif
    }

    public static void OnCurrencyItemPurchased(string productId, bool showMessage = false)
    {
        int ammount = GetCurrencyAmmount(productId);
        _currency += ammount;

        SaveLocal();

        if (OnCurrencyBought != null)
            OnCurrencyBought(ammount);
        else if(showMessage)
            DesktopUtils.ShowLocalizedMessageBox("ITEM_PURCHASED");
    }

    public static bool CanBuy(EconomyItem ei)
    {
        return ei.price <= currency;
    }

    public static List<CurrencyItem> GetCurrencyItems()
    {
        return forSaleCurrency;
    }

    public static void ShowBuyItemWithSoftCurrencyBox(EconomyItem ei, string msg = null)
    {
        WindowA w = WindowA.Create("UI/BuyItemBox");
        BuyItemBox bib = w.GetComponent<BuyItemBox>();
        bib.item = ei;
        w.ShowModal();
    }

    public static string[] GetProductIds()
    {
        string[] products = new string[forSaleCurrency.Count];
        int idx = 0;
        foreach (CurrencyItem ci in forSaleCurrency)
        {
            products[idx++] = ci.sProductId;
        }
        return products;
    }

    public static string GetFullItemId(string id)
    {
        if (id == null) return null;
#if UNITY_EDITOR
        return "com.amberstudio.linktwin." + id;
#elif UNITY_ANDROID
        return "com.amberstudio.linktwin." + id + ".sapps";
#else
        return "com.amberstudio.linktwin." + id;
#endif
    }

    public static void UpdateShopItem(string id, string formattedPrice, string currencyCode, float price)
    {
        foreach (EconomyItem ei in forSaleItems)
        {
            if (ei.shopId == id)
            {
                ei.priceStr = formattedPrice;
                ei.price = price;
                ei.currencyCode = currencyCode;
                Debug.Log("Updated info for " + id + " price str: " + ei.priceStr + " price: " + price);
                break;
            }
        }
    }

    public static void onProductReceived(string productId, string title, string description, string formattedPrice,
        float price)
    {
        Debug.Log("GameEconomy onProductReceived " + productId);
        foreach (CurrencyItem ci in forSaleCurrency)
        {
            if (ci.sProductId == productId)
            {
                ci.sProductName = title;
                ci.sProductDesc = description;
                ci.sProductFormattedPrice = formattedPrice;
                ci.sPrice = price;

                if (bcb != null && bcb.isActiveAndEnabled)
                {
                    ShopCurrencyItem[] allElems = bcb.GetComponentsInChildren<ShopCurrencyItem>();
                    foreach (ShopCurrencyItem sc in allElems)
                    {
                        if (sc.item.sProductId == productId)
                        {
                            sc.text.text = title;
                            sc.ammountText.text = formattedPrice;
                            break;
                        }

                    }
                }
                break;
            }
        }
    }

    public static int GetCurrencyAmmount(string productId)
    {
        foreach (CurrencyItem ci in forSaleCurrency)
        {
            if (ci.sProductId == productId)
            {
                return ci.ammount;
            }
        }
        return 0;
    }

    public static int GetPriceForChapter(int i)
    {
        Initialize();

        List<EconomyItem> list = GetItemsForSale(EconomyItemType.ChapterUnlock);
        string strToSearch = "chapter" + (i + 1).ToString();

        foreach(EconomyItem ei in list)
        {
            if (ei.args.Equals(strToSearch))
                return (int)ei.price;
        }

        return 0;
    }


    public static Action OnNotEnoughCurrency;
    public static bool ConsumeCurrency(int amm)
    {
        if(amm <= currency)
        {
            currency -= amm;
            return true;
        }

        if (OnNotEnoughCurrency != null)
            OnNotEnoughCurrency();

        return false;
    }

    public static void Consume(EconomyItemType eit, int ammount)
    {
        EconomyItem ei = boughtItems[eit];
        ei.ammount -= ammount;
    }

    public static bool CanShowVideoAd()
    {
        // --- less than five watches
        if (videoAdTimes.Count < 5)
            return true;

        // --- remove watch dates older than a day
        while ((DateTime.Now - videoAdTimes.Peek()).Hours > 24)
            videoAdTimes.Dequeue();

        return videoAdTimes.Count < 5;
    }

    private static void LoadVideoAdTimes()
    {
        if (!PlayerPrefs.HasKey(kVideoAdTimesKey))
            return;

        string[] data = PlayerPrefs.GetString(kVideoAdTimesKey).Split();
        for (int i = 0; i < data.Length; i++)
        {
            DateTime dt = DateTime.FromBinary(Convert.ToInt64(data[i]));
            videoAdTimes.Enqueue(dt);
        }
    }

    private static void SaveVideoAdTimes()
    {
        string data = "";
        DateTime[] dateArray = videoAdTimes.ToArray();
        for (int i = 0; i < dateArray.Length; i++)
        {
            data += dateArray[i].ToBinary().ToString();
            if (i < dateArray.Length - 1)
                data += " ";
        }
        PlayerPrefs.SetString(kVideoAdTimesKey, data);
    }

    public static void OnVideoAdWatched()
    {
        videoAdTimes.Enqueue(DateTime.Now);
        SaveVideoAdTimes();
    }
}
