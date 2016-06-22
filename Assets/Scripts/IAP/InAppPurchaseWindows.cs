using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_METRO || UNITY_WP8

public class InAppPurchaseWindows : InAppPurchase
{
	bool bFetchingProductList = false;
	bool bShowMessage = false;
	string sMessage = "";

    public InAppPurchaseWindows()
    {
		Debug.Log("[InAppPurchaseWindows.ctor()]");
		bFetchingProductList = false;

		WinStoreManager.purchaseFailedEvent     += onPurchaseFailed;
		WinStoreManager.purchaseCancelledEvent  += onPurchaseCancelled;
		WinStoreManager.purchaseSuccessfulEvent += onPurchaseSuccessful;

        WinStoreManager.productListReceivedEvent      += onProductListReceived;
        WinStoreManager.productListRequestFailedEvent += onProductListReqFailed;

		WinStoreManager.purchaseRestoreFinishedEvent += onPurchaseRestoreFinished;

		bShowMessage = false;
		sMessage = "";
    }

    public override void RetrieveProductList()
    {
		if (!bFetchingProductList)
		{
			bFetchingProductList = true;
			Debug.Log("[InAppPurchaseWindows.RetrieveProductList()] fetching list");
        	WinStoreBinding.requestProductData();
		}
		else
		{
			Debug.Log("[InAppPurchaseWindows.RetrieveProductList()] already fetching list, returning");
		}
    }

    public override bool IsInAppPurchaseEnabled()
    {
        if (WinStoreBinding.canMakePayments())
        {
            return true;
        } 
        else
        {
            return false;
        }
    }

    public void onProductListReceived(List<WinStoreProduct> productList)
    {
		for(int i = 0 ; i < productList.Count ; i++)
		{
			Debug.Log( ">> >> Received product :" + productList[i]);

            float price = 0.0f;
            float.TryParse(productList[i].price, out price);
            onProductReceived(productList[i].productIdentifier, productList[i].title, productList[i].description, productList[i].price, price);
		}
		productListReceived = true;
		bFetchingProductList = false;
    }

    public void onProductListReqFailed(string error)
    {
		Debug.Log("[InAppPurchaseWindows] onProductListReqFailed");
		productListReceived = false;
		bFetchingProductList = false;
    }

    public override void PurchaseProduct(string productId)
    {
		Debug.Log("[InAppPurchaseWindows] PurchaseProduct" + productId);
		if(WinStoreBinding.canMakePayments())
		{
        	WinStoreBinding.purchaseProduct(productId);
		}
    }

	public override void ConsumeTransaction(string transactionId)
	{
		//do we implement this or leave the Store auto validate?
		//because it does so unless we tell him otherwise
		Debug.Log ("[InappPurchaseWindows.ConsumeTransaction]");
	}
	
    public override void RestorePurchases()
    {
        if (WinStoreBinding.canMakePayments()) 
		{
			WinStoreBinding.restoreCompletedTransactions();
		}
    }

	public void onPurchaseFailed(string error)
	{
		Debug.Log("[InAppPurchaseWindows.onPurchaseFailed] Error : " + error );
		PurchaseFailed(); //dummy value, not used
	}

	public void onPurchaseCancelled(string error)
	{
		Debug.Log("[InAppPurchaseWindows.onPurchaseCancelled] Error : " + error );
		PurchaseCancelled(); //dummy value, not used
	}

	public void onPurchaseSuccessful(WinStoreProduct p)
	{
		Debug.Log("[onPurchaseSuccessful]" + p.productIdentifier);
        //Here we need to add the purchased product to the game 

        Debug.Log("Purchase succesfull (" + p.productIdentifier + " )");
        PurchaseSuccessful(p.productIdentifier, "" /*p.transactionIdentifier*/); //TODO[AMBER]:store validation
	}

	public void onPurchaseRestoreFinished (List<WinStoreProduct> durableItems, string error)
	{
		/* no restorable items yet, no need to do anything for now */
		Debug.Log("[InAppPurchaseWindows.onPurchaseRestoreFinished] count = " + durableItems.Count + ", error string = " + error);
	}

	/* overrides */
	public override void PurchaseFailed()
	{
        /*TODO
		sMessage = Locale.GetWrappedString("STORE_FAILED_TEXT", 24);
		bShowMessage = true;
        */
	}
	
	public override void PurchaseCancelled()
	{
        /*TODO
		sMessage = Locale.GetWrappedString("TRANSACTION_CANCELED", 24);
		bShowMessage = true;
        */
    }

    public override void PurchaseSuccessful(string productId, string transactionId, bool log_action = true)
	{
		//Convert the purchased product into game items (lives, moves, powerups etc...) 
		//Call the comsune/validate transaction
		Debug.Log("consuming transaction");
			
		ConsumeTransaction(transactionId);
        /*TODO
		Analytics.LogEventPaymentAction(productId);

		sMessage = Locale.GetWrappedString("STORE_PURCHASED_TEXT", 24);
		bShowMessage = true;

		UserData.Instance.Save(true); //force save to cloud after a purchase
        */
    }

    void Update()
	{
        /*TODO
		if (bShowMessage)
		{
			UIManager.Instance.ShowTimedMessage(sMessage);
			bShowMessage = false;
		}
        */
    }
}
#endif