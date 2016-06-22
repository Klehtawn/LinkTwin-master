
#if UNITY_ANDROID
using UnityEngine;
using System.Collections;
using Prime31;
using System.Collections.Generic;


public class InAppPurchaseAndroid : InAppPurchase
{
    public const string HAS_MADE_PURCHASES = "IAPHasMadePruchase";
	
	protected static string disneyKey = 
        "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAsqCKsbgpoXD/e/XIbxtBm" + 
        "89dJYppKkUmrGspSroQRR5zNxpO1m6TQS007QyVPRaVIa/h+5xW/mcq6G6G3nbFZk" + 
        "xIS5uCZiCB67OTDZMO6tL8oCNBbdZLVRjEU0PbAHgdys69R3nGBeQ1Br5aDXrPZoD" + 
        "+JTEXeThofgvZawkRdi8iOmX7gWONGpDoz0YBTjyhw9evERyFAgzVWWf1jWAmyyZ6" + 
        "VOT1V9lJKw6pfxW2Qa8oVeuM2EIjJHp8roDLVWLM6/QPruRe39jcvE23K1ILlCtpC" + 
        "q/0+lejqDkzpFEfOtKvbnnP0dCSAia5EaWSGM7rL9dW2bMrqGFWUAOEumEPGwIDAQAB";


	protected bool billingSupported = false;
	private string purchasingProduct;
	List<string> prevPurchaseIDs = new List<string>(); 
	List<string> prevPurchaseTransactionIDs = new List<string>();

    private const int BILLING_RESPONSE_RESULT_OK = 0; //Success
    private const int BILLING_RESPONSE_RESULT_USER_CANCELED = 1; //User pressed back or canceled a dialog
    private const int BILLING_RESPONSE_RESULT_SERVICE_UNAVAILABLE = 4; //Network connection is down
    private const int BILLING_RESPONSE_RESULT_BILLING_UNAVAILABLE = 3; //Billing API version is not supported for the type requested
    private const int BILLING_RESPONSE_RESULT_ITEM_UNAVAILABLE = 4; //Requested product is not available for purchase
    private const int BILLING_RESPONSE_RESULT_DEVELOPER_ERROR = 5; //Invalid arguments provided to the API.This error can also indicate that the application was not correctly signed or properly set up for In-app Billing in Google Play, or does not have the necessary permissions in its manifest
    private const int BILLING_RESPONSE_RESULT_ERROR = 6; //Fatal error during the API action
    private const int BILLING_RESPONSE_RESULT_ITEM_ALREADY_OWNED = 7; //Failure to purchase since item is already owned
    private const int BILLING_RESPONSE_RESULT_ITEM_NOT_OWNED = 8; //Failure to consume since item is not owned
    
    public InAppPurchaseAndroid()
	{
        productListReceived = false;
    }
	
	public override bool IsInAppPurchaseEnabled()
	{
		return billingSupported;
    }
	
	void OnApplicationPause(bool pauseStatus) {
		if(!pauseStatus && !billingSupported)
		{
			RetrieveProductList();
		}
	}

	public override void RetrieveProductList()
	{
		GoogleIABManager.billingSupportedEvent += OnBillingSupported;
		GoogleIABManager.billingNotSupportedEvent += OnBillingNotSupported;
        Debug.Log("InAppPurchaseAndroid RetrieveProductList " + disneyKey);
        GoogleIAB.init(disneyKey);
	}

	protected void OnBillingSupported()
	{
		GoogleIABManager.billingSupportedEvent -= OnBillingSupported;
		GoogleIABManager.billingNotSupportedEvent -= OnBillingNotSupported;
		
		billingSupported = true;
        Debug.Log("InAppPurchaseAndroid OnBillingSupported productListReceived = " + productListReceived);

        if (!productListReceived)
			QueryInventory();
	}
	
	protected void OnBillingNotSupported(string error)
	{
		GoogleIABManager.billingSupportedEvent -= OnBillingSupported;
		GoogleIABManager.billingNotSupportedEvent -= OnBillingNotSupported;
		
		billingSupported = false;

		if(productListReceived)
			PurchaseDisabled ();
		Debug.Log("InAppPurchaseAndroid OnBillingNotSupported " + error);
	}

	protected void QueryInventory(bool onResume = false)
	{
        Debug.Log("QueryInventory");
        GoogleIABManager.queryInventorySucceededEvent += OnQueryInventorySucceeded;
		GoogleIABManager.queryInventoryFailedEvent += OnQueryInventoryFailed;
        Debug.Log("GetProductIds()=" + GetProductIds().GetValue(0));
        GoogleIAB.queryInventory(GetProductIds());
	}

	protected void OnQueryInventorySucceeded(List<GooglePurchase> purchases, List<GoogleSkuInfo> productList)
	{
		Debug.Log( "InAppPurchaseAndroid: OnQueryInventorySucceeded with " + productList.Count + " products");

		GoogleIABManager.queryInventorySucceededEvent -= OnQueryInventorySucceeded;
		GoogleIABManager.queryInventoryFailedEvent -= OnQueryInventoryFailed;
		
		string sTemp;
		int index;
		char[] priceChars = {'0','1','2','3','4','5','6','7','8','9',',','.'};
		
		for(int i = 0 ; i < productList.Count ; i++)
		{
			Debug.Log( "[Amber]>> >> Received product :" + productList[i].ToString());
			
			string currencySymbol        = productList[i].priceCurrencyCode;

			sTemp = productList[i].price.Trim(currencySymbol.ToCharArray());

			index = 0;
			while (index < sTemp.Length)
			{
				char c = sTemp[index];
				bool found = false;
				for (int j = 0; j < priceChars.Length && !found; j++)
				{
					if (c == priceChars[j])
					{
						found = true;
					}
				}

				if (!found)
					sTemp = sTemp.Replace("" + c, "");
				else
					index++;
			}

            float price = 0.0f;
            float.TryParse(sTemp, out price);
            onProductReceived(productList[i].productId, productList[i].title, productList[i].description, productList[i].price, price);
		}

		if (purchases == null || purchases.Count == 0)
		{
			Debug.Log ("[InAppPurchaseAndroid] no previous purchases found!");
		}
		else
		{
			for (int i = 0 ; i < purchases.Count ; i++)
			{
				Debug.Log ("[InAppPurchaseAndroid] found prev purchase: "
				           + purchases[i].type + ", "
				           + purchases[i].purchaseState.ToString() + ", "
				           + (purchases[i].purchaseState == GooglePurchase.GooglePurchaseState.Purchased ? "purchased" : "other") + ", "
				           + purchases[i].productId);
				if (purchases[i].purchaseState == GooglePurchase.GooglePurchaseState.Purchased)
				{
					Debug.Log ("[InAppPurchaseAndroid] save prev purchase: " + purchases[i].productId);

					prevPurchaseIDs.Add(purchases[i].productId);
					prevPurchaseTransactionIDs.Add(purchases[i].orderId);
				}
			}
		}

		productListReceived = true;
	}
	
	protected void OnQueryInventoryFailed(string error)
	{
		GoogleIABManager.queryInventorySucceededEvent -= OnQueryInventorySucceeded;
		GoogleIABManager.queryInventoryFailedEvent -= OnQueryInventoryFailed;
		
		Debug.Log("InAppPurchaseAndroid.OnQueryInventoryFailed Failed to retrieve product list! ");
		Debug.Log("InAppPurchaseAndroid.OnQueryInventoryFailed Error : " + error );
		productListReceived = false;
	}


	public override void PurchaseProduct(string productId)
	{
		if (billingSupported) 
		{
			Debug.Log("InAppPurchaseAndroid Purchasing product: " + productId);
			GoogleIABManager.purchaseSucceededEvent += onPurchaseSuccessful;
			GoogleIABManager.purchaseFailedEvent += onPurchaseFailed;
			RetrieveProductList();
			purchasingProduct = productId;
			GoogleIAB.purchaseProduct(purchasingProduct);
		}
	}
	
	public override void ConsumeTransaction(string transactionId)
	{
		//do we implement this or leave the Store auto validate?
		//because it does so unless we tell him otherwise
	}
	
	public override void RestorePurchases()
	{
		Debug.Log("[InAppPurchaseAndroid] RestorePurchases called");

		if (prevPurchaseIDs == null || prevPurchaseIDs.Count == 0)
		{
			Debug.Log("[InAppPurchaseAndroid] No purchases to restore!");
            //TODO
            //UIManager.Instance.ShowTimedMessage(Locale.GetWrappedString("RESTORE_NO_PURCHASES", 24), null, "DimingMask", -100);
		}
		else
		{
			string[] prevPurchaseIDsArray = prevPurchaseIDs.ToArray();
			string[] prevPurchaseTransactionIDsArray = prevPurchaseTransactionIDs.ToArray();

			for (int i = 0; i < prevPurchaseIDsArray.Length; i++)
			{
				PurchaseSuccessful(prevPurchaseIDsArray[i], prevPurchaseTransactionIDsArray[i], false);
				prevPurchaseIDs.Remove(prevPurchaseIDsArray[i]);
				prevPurchaseTransactionIDs.Remove(prevPurchaseTransactionIDsArray[i]);
			}
		}
	}

	public void onPurchaseFailed(string error, int code)
	{
		GoogleIABManager.purchaseSucceededEvent -= onPurchaseSuccessful;
		GoogleIABManager.purchaseFailedEvent -= onPurchaseFailed;

		if (code == BILLING_RESPONSE_RESULT_USER_CANCELED)
		{
			onPurchaseCancelled(error);
			return;
		}

		PurchaseFailed();

		Debug.Log("InAppPurchaseAndroid onPurchaseFailed " + error);
	}
	
	public void onPurchaseCancelled(string error)
	{
		GoogleIABManager.purchaseSucceededEvent -= onPurchaseSuccessful;
		GoogleIABManager.purchaseFailedEvent -= onPurchaseFailed;

		PurchaseCancelled();
		Debug.Log("InAppPurchaseAndroid onPurchaseCancelled " + error);
	}
	
	public void onPurchaseSuccessful(GooglePurchase transaction)
	{
		GoogleIABManager.purchaseSucceededEvent -= onPurchaseSuccessful;
		GoogleIABManager.purchaseFailedEvent -= onPurchaseFailed;
		PlayerPrefs.SetInt(HAS_MADE_PURCHASES, 1);
		
		Debug.Log("InAppPurchaseAndroid onPurchaseSuccessful (" + transaction.productId + "," + transaction.orderId + " )");

		//consume product
		GoogleIABManager.consumePurchaseSucceededEvent += onConsumePurchaseSuccessful;
		GoogleIABManager.consumePurchaseFailedEvent += onConsumePurchaseFailed;
		GoogleIAB.consumeProduct(transaction.productId);
	}

	public void onConsumePurchaseSuccessful(GooglePurchase purchase)
	{
		GoogleIABManager.consumePurchaseSucceededEvent -= onConsumePurchaseSuccessful;
		GoogleIABManager.consumePurchaseFailedEvent -= onConsumePurchaseFailed;
		
		PurchaseSuccessful(purchase.productId, purchase.orderId);
		Debug.Log("InAppPurchaseAndroid onConsumePurchaseSuccessful (" + purchase.productId + "," + purchase.orderId + " )");
	}

	public void onConsumePurchaseFailed(string error)
	{
		Debug.Log("InAppPurchaseAndroid onConsumePurchaseFailed " + error);

		GoogleIABManager.consumePurchaseSucceededEvent -= onConsumePurchaseSuccessful;
		GoogleIABManager.consumePurchaseFailedEvent -= onConsumePurchaseFailed;
		
		PurchaseFailed();
	}

	public void onSilentConsumePurchaseSuccessful(GooglePurchase purchase)
	{
		GoogleIABManager.consumePurchaseSucceededEvent -= onSilentConsumePurchaseSuccessful;
		GoogleIABManager.consumePurchaseFailedEvent -= onSilentConsumePurchaseFailed;
		
		AddProductSilent(purchase.productId, purchase.orderId);
		Debug.Log("InAppPurchaseAndroid onSilentConsumePurchaseSuccessful (" + purchase.productId + "," + purchase.orderId + " )");

		if(prevPurchaseIDs.Count > 0)
		{
			GoogleIABManager.consumePurchaseSucceededEvent += onSilentConsumePurchaseSuccessful;
			GoogleIABManager.consumePurchaseFailedEvent += onSilentConsumePurchaseFailed;
			GoogleIAB.consumeProduct(prevPurchaseIDs.ToArray()[0]);
			prevPurchaseIDs.RemoveAt(0);
			prevPurchaseTransactionIDs.RemoveAt(0);
		}
	}

	public void onSilentConsumePurchaseFailed(string error)
	{
		GoogleIABManager.consumePurchaseSucceededEvent -= onSilentConsumePurchaseSuccessful;
		GoogleIABManager.consumePurchaseFailedEvent -= onSilentConsumePurchaseFailed;

		Debug.Log("InAppPurchaseAndroid onSilentConsumePurchaseFailed " + error);

		if(prevPurchaseIDs.Count > 0)
		{
			GoogleIABManager.consumePurchaseSucceededEvent += onSilentConsumePurchaseSuccessful;
			GoogleIABManager.consumePurchaseFailedEvent += onSilentConsumePurchaseFailed;
			GoogleIAB.consumeProduct(prevPurchaseIDs.ToArray()[0]);
			prevPurchaseIDs.RemoveAt(0);
			prevPurchaseTransactionIDs.RemoveAt(0);
		}
	}

	public override bool HasMadeTransactions()
	{
		bool hasMadeTransactions = (PlayerPrefs.GetInt(HAS_MADE_PURCHASES) == 1);			
		return hasMadeTransactions;
	}
}

#endif
