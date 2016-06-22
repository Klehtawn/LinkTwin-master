using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_METRO || UNITY_WP8
public class WinStoreBinding
{
	public delegate void RequestProductDataCallBack();
	
	public static RequestProductDataCallBack storeRequestProductData = null;

	public static void requestProductData()
	{
		if (storeRequestProductData != null)
		{
			Debug.Log("[WinStoreBinding.requestProductData] requesting");
			storeRequestProductData();
		}
		else
		{
			Debug.Log("[WinStoreBinding.requestProductData] callback NULL, doing nothing");
		}
	}

	public delegate void PurchaseProductCallBack(string productIdentifier);
	
	public static PurchaseProductCallBack storePurchaseProduct = null;

	public static void purchaseProduct(string productIdentifier)
	{
		if (storePurchaseProduct != null)
		{
			storePurchaseProduct(productIdentifier);
		}
	}

	public delegate bool CanMakePaymentsCallBack();
	
	public static CanMakePaymentsCallBack canMakePaymentsCallBack = null;
	
	public static bool canMakePayments()
	{
		if(canMakePaymentsCallBack != null)
		{
			return canMakePaymentsCallBack();
		}
		return false;
	}
	
	public delegate void RestoreCompletedTransactionsCallBack();
	
	public static RestoreCompletedTransactionsCallBack restoreCompletedTransactionsCallBack = null;
	
	public static void restoreCompletedTransactions()
	{
		if(restoreCompletedTransactionsCallBack != null)
		{
			restoreCompletedTransactionsCallBack();
		}
	}
}
#endif