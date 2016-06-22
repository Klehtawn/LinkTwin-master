using System;
using System.Collections.Generic;
#if UNITY_METRO || UNITY_WP8

public class WinStoreManager
{
	public static event Action<List<WinStoreProduct>> productListReceivedEvent;

	public static event Action<string> productListRequestFailedEvent;

	public static event Action<WinStoreProduct> purchaseSuccessfulEvent;

	public static event Action<string> purchaseFailedEvent;

	public static event Action<string> purchaseCancelledEvent;

	public static event Action<List<WinStoreProduct>, string> purchaseRestoreFinishedEvent;

	public void productListReceived(List<WinStoreProduct> productList)
	{
		if(productListReceivedEvent != null)
		{
			productListReceivedEvent(productList);
		}
	}

	public void productListRequestFailed(string error)
	{
		if(productListRequestFailedEvent != null)
		{
			productListRequestFailedEvent( error );
		}
	}

	public void productPurchased(WinStoreProduct product)
	{
		if(purchaseSuccessfulEvent != null)
		{
			purchaseSuccessfulEvent(product);
		}
	}

	public void productPurchaseFailed(string error)
	{
		if(purchaseFailedEvent != null)
		{
			purchaseFailedEvent(error);
		}
	}

	public void productPurchaseCancelled(string error)
	{
		if(purchaseCancelledEvent != null)
		{
			purchaseCancelledEvent(error);
		}
	}

	public void purchaseRestoreFinished(List<WinStoreProduct> durableItems, string error)
	{
		if(purchaseRestoreFinishedEvent != null)
		{
			purchaseRestoreFinishedEvent(durableItems, error);
		}
	}
}
#endif