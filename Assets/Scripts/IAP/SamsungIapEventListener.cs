using UnityEngine;
using System;
using LitJson;

public class SamsungIapEventListener : MonoBehaviour
{
    // --- used for error/cancel analytics events, Samsung API doesn't provide the item id in those cases
    public string mLastItemId;

    public void ProductListReceived(string itemData)
    {
        JsonReader reader = new JsonReader(itemData);
        JsonData data = JsonMapper.ToObject(reader);
        Debug.Log("Got " + data.Count + " items from store");
        for (int i = 0; i < data.Count; i++)
        {
            JsonData item = data[i];
            float price = 0;
            JsonData priceInfo = item["mItemPrice"];
            Debug.Log("price info type: " + priceInfo.GetType() + " json: " + priceInfo.GetJsonType());
            if (priceInfo.GetJsonType() == JsonType.Double)
                price = (float)(double)priceInfo;
            else if (priceInfo.GetJsonType() == JsonType.Int)
                price = (int)priceInfo;
            else if (priceInfo.GetJsonType() == JsonType.String)
                float.TryParse((string)priceInfo, out price);

            GameEconomy.UpdateShopItem(
                (string)item["mItemId"],
                (string)item["mItemPriceString"],
                (string)item["mCurrencyCode"],
                price
            );
        }
    }

    public void PurchaseSuccessful(string jsonInfo)
    {
        JsonReader reader = new JsonReader(jsonInfo);
        JsonData data = JsonMapper.ToObject(reader);
        Debug.Log(jsonInfo);
        Debug.Log("Product Purchased: " + data["mItemId"]);
        GameEconomy.OnItemPurchaseComplete(GameEconomy.GetEconomyItemById((string)data["mItemId"]));
    }

    public void PurchaseCanceled(string errorInfo)
    {
        Debug.Log("Purchase canceled: " + mLastItemId);
        GameEconomy.OnItemPurchaseCanceled(GameEconomy.GetEconomyItemById(mLastItemId), errorInfo);
    }

    public void PurchaseFailed(string errorInfo)
    {
        Debug.Log("Product Purchased: " + mLastItemId);
        GameEconomy.OnItemPurchaseError(GameEconomy.GetEconomyItemById(mLastItemId), errorInfo);
    }
}
