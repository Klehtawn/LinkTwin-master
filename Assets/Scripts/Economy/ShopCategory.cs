using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class ShopCategory : MonoBehaviour {

    public ShopItemBase itemPrefab;

    public Text title;
    public Transform subtitleObject;

    public bool showTotalCurrency = true;

    private RectTransform contents;

    public bool dynamicHeightUpdate = true;

    public int numColumns = 1;
    public bool extendItemSize = true;

    private float initialContentY;

    void Awake()
    {
        contents = transform.Find("Contents").GetComponent<RectTransform>();

        RectTransform contentRect = contents.GetComponent<RectTransform>();
        initialContentY = contentRect.offsetMax.y;
    }

    void Start()
    {
        //FillDummy();
    }

    void Update()
    {
        if (dynamicHeightUpdate)
        {
            if (myTransform == null) myTransform = GetComponent<RectTransform>();
            myTransform.sizeDelta = new Vector2(0.0f, height);
        }
    }

	public void Fill(GameEconomy.EconomyItemType itemType, bool isBaseType = false)
    {       
        List<GameEconomy.EconomyItem> items = isBaseType ? GameEconomy.GetItemsForSaleCategorized(itemType) : GameEconomy.GetItemsForSale(itemType);

        title.GetComponent<LocalizedText>().text = itemType.ToString().ToUpper();

        AdjustLayout(items.Count);

        for(int i = 0; i < items.Count; i++)
        {
            if(i == 0)
            {
                title.GetComponent<LocalizedText>().text = items[0].displayName.ToUpper();
            }
            GameObject o = GameObject.Instantiate<GameObject>(itemPrefab.gameObject);
            o.transform.SetParent(contents);
            o.transform.localScale = Vector3.one;

            Widget w = o.GetComponent<Widget>();
            w.OnClick += _OnItemClicked;

            ShopItemBase sib = o.GetComponent<ShopItemBase>();
            sib.shopItem = items[i];

            if (items[i].bundleItems == null || items[i].bundleItems.Length == 0)
            {
                sib.itemsDesc = new ShopItemBase.ItemDesc[1];
                sib.itemsDesc[0] = new ShopItemBase.ItemDesc();
                sib.itemsDesc[0].icon = GameEconomy.GetSpriteForItem(items[i].type);
                sib.itemsDesc[0].ammount = items[i].ammount + " " + items[i].displayName.ToUpper();
            }
            else
            {
                int c = items[i].bundleItems.Length;
                sib.itemsDesc = new ShopItemBase.ItemDesc[c];
                for (int k = 0; k < c; k++)
                {
                    sib.itemsDesc[k] = new ShopItemBase.ItemDesc();
                    sib.itemsDesc[k].icon = GameEconomy.GetSpriteForItem(items[i].bundleItems[k]);
                    sib.itemsDesc[k].ammount = items[i].bundleAmmounts[k].ToString();
                }
            }
            sib.priceText.text = items[i].priceStr;
        }

        gameObject.name = itemType.ToString();
    }

    public float height
    {
        get
        {
            return contents.rect.height + Mathf.Abs(contents.anchoredPosition.y);
        }
    }

    RectTransform myTransform = null;
    public Vector2 pos
    {
        get
        {
            if (myTransform == null) myTransform = GetComponent<RectTransform>();
            return myTransform.anchoredPosition;
        }
        set
        {
            if (myTransform == null) myTransform = GetComponent<RectTransform>();
            myTransform.anchoredPosition = value;
        }
    }

    public Action<GameEconomy.EconomyItem> OnItemClicked;
    void _OnItemClicked(MonoBehaviour sender, Vector2 pos)
    {
        if (OnItemClicked != null)
            OnItemClicked(sender.GetComponent<ShopItemBase>().shopItem);
    }

    void AdjustLayout(int numElements)
    {
        RectTransform contentRect = contents.GetComponent<RectTransform>();

        if (showTotalCurrency)
        {
            //contentRect.offsetMax = new Vector2(contentRect.offsetMax.x, initialContentY - 30.0f);
            Text t = subtitleObject.GetComponentInChildren<Text>();
            t.text = "YOU HAVE  " + GameEconomy.currency;
        }
        else
        {
            contentRect.offsetMax = new Vector2(contentRect.offsetMax.x, initialContentY);
            subtitleObject.gameObject.SetActive(false);
        }

        RectTransform itemRect = itemPrefab.GetComponent<RectTransform>();

        GridLayoutGroup glg = contents.GetComponent<GridLayoutGroup>();
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = numColumns;

        float myWidth = contentRect.rect.width;
        float myHeight = contentRect.rect.height;

        if (numColumns == 1)
            extendItemSize = true;

        float itemsPerColumn = Mathf.Round(numElements / numColumns);

        if (extendItemSize)
        {
            Vector2 cellSize = Vector2.zero;
            cellSize.x = (myWidth - (float)(numColumns - 1) * glg.spacing.x) / (float)numColumns;
            cellSize.y = itemPrefab.GetComponent<RectTransform>().rect.height;

            glg.cellSize = cellSize;
        }
        else
        {
            Vector2 cellSize = Vector2.zero;
            cellSize.x = itemRect.rect.width;
            cellSize.y = itemRect.rect.height;

            glg.cellSize = cellSize;
        }

        if (extendItemSize)
        {
            Vector2 spacing = Vector2.zero;
            spacing.x = glg.spacing.x;
            spacing.y = (myHeight - (float)numElements * itemRect.rect.height) / (float)(numElements - 1);
            glg.spacing = spacing;
        }
        else
        {
            if (numColumns > 1)
            {
                Vector2 spacing = Vector2.zero;
                spacing.x = (myWidth - (float)numColumns * itemRect.rect.width) / (float)(numColumns - 1);
                spacing.y = (myHeight - (float)itemsPerColumn * itemRect.rect.height) / (itemsPerColumn - 1.0f);
                glg.spacing = spacing;
            }
        }
    }

    void FillDummy()
    {
        int c = 8;

        AdjustLayout(c);

        for(int i = 0; i < c; i++)
        {
            GameObject obj = GameObject.Instantiate<GameObject>(itemPrefab.gameObject);
            obj.transform.SetParent(contents);
            obj.transform.localScale = Vector3.one;
        }
    }
}
