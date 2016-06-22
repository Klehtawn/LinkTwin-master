using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_IPHONE

using Prime31;

public class InAppPurchaseIOS : InAppPurchase
{ 
    public InAppPurchaseIOS()
    {
		StoreKitManager.purchaseFailedEvent     += onPurchaseFailed;
		StoreKitManager.purchaseCancelledEvent  += onPurchaseCancelled;
		StoreKitManager.purchaseSuccessfulEvent += onPurchaseSuccessful;

        StoreKitManager.productListReceivedEvent      += onProductListReceived;
        StoreKitManager.productListRequestFailedEvent += onProductListReqFailed;

    }

    public override void RetrieveProductList()
    {
        StoreKitBinding.requestProductData(GetProductIds());
    }

    public override bool IsInAppPurchaseEnabled()
    {
        if (StoreKitBinding.canMakePayments())
        {
            return true;
        } 
        else
        {
            return false;
        }
    }

    public void onProductListReceived(List<StoreKitProduct> productList)
    {
        for(int i = 0 ; i < productList.Count ; i++)
        {
            Debug.Log( ">> >> Received product :" + productList[i].ToString());

            float price = 0.0f;
            float.TryParse(productList[i].price, out price);
            onProductReceived(productList[i].productIdentifier, productList[i].title, productList[i].description, productList[i].formattedPrice, price);
        }
        productListReceived = true;
    }

    public void onProductListReqFailed(string error)
    {
        Debug.Log("Failed to retrieve product list! ");
        if (error == null)
            error = "";
        Debug.Log("Error : " + error );
        productListReceived = false;
    }

    public override void PurchaseProduct(string productId)
    {
		if(StoreKitBinding.canMakePayments())
		{
        	StoreKitBinding.purchaseProduct(productId, 1);
		}
    }

	public override void ConsumeTransaction(string transactionId)
	{
		//do we implement this or leave the Store auto validate?
		//because it does so unless we tell him otherwise
	}
	
    public override void RestorePurchases()
    {
        if (StoreKitBinding.canMakePayments()) 
		{
			StoreKitBinding.restoreCompletedTransactions();
		}
    }

	public void onPurchaseFailed(string error)
	{
		PurchaseFailed();
	}

	public void onPurchaseCancelled(string error)
	{
		PurchaseCancelled();
	}

    protected StoreKitTransaction pendingTransaction = null;

    protected void verifyPurchaseCallback(bool success, int code, string name, string message)
    {
        //if (success)
        //    Debug.Log("Verified VALID");
        //else
        //    Debug.Log("Verified INVALID");
        //Debug.Log("status code " + code);
        //Debug.Log("verification name " + name);
        //Debug.Log("verification message " + message);
        if (success)
            PurchaseSuccessful(pendingTransaction.productIdentifier, pendingTransaction.transactionIdentifier);
        else
        {
#if USE_DMO_ANALYTICS
            Analytics.LogGameAction("IAP", "failed_receipt", pendingTransaction.productIdentifier, name, code);
#endif
            PurchaseSuccessful(pendingTransaction.productIdentifier, pendingTransaction.transactionIdentifier, false);
        }
        pendingTransaction = null;
    }

	public void onPurchaseSuccessful(StoreKitTransaction transaction)
	{
        Debug.Log(transaction.ToString());
        Debug.Log("Purchase succesfull (" + transaction.productIdentifier + "," + transaction.transactionIdentifier + " )");
        PurchaseSuccessful(transaction.productIdentifier, transaction.transactionIdentifier);
	}


    public override bool HasMadeTransactions()
    {
        List<StoreKitTransaction> transactionList = StoreKitBinding.getAllSavedTransactions();
        if (transactionList.Count > 0)
            return true;
        else
            return false;
    }
}

#endif
