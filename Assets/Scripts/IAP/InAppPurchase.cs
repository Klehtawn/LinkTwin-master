using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InAppPurchase
{

    protected static string applicationId = Application.bundleIdentifier;

    public InAppPurchase()
    {
        productListReceived = false;
    }

    protected bool productListReceived;
    
    public string[] GetProductIds()
    {
        return GameEconomy.GetProductIds();
    }

	public virtual bool IsProductListReceived()
    {
        return productListReceived;
    }

	public virtual void PurchaseDisabled()
    {
       // DesktopUtils.ShowMessageBox("Store Not Available");
    }

	public virtual void PurchaseFailed()
    {
        //Show purchase failed window... and nothing else.
        DesktopUtils.ShowLocalizedMessageBox("PURCHASE_FAILED");
    }

	public virtual void PurchaseCancelled()
    {
        //Show purchase canceled window... and nothing else.
        DesktopUtils.ShowLocalizedMessageBox("PURCHASE_CANCELED");
    }

    public virtual void PurchaseSuccessful(string productId, string transactionId, bool log_action = true)
    {
        //Show purchase success window.
        AddProductSilent(productId, transactionId, true);
    }

	public virtual void AddProductSilent(string productId, string transactionId, bool showMessage= false)
	{
        //Convert the purchased product into game items (lives, moves, powerups etc...) 
        //Call the comsune/validate transaction
        Debug.Log("consuming transaction");

        GameEconomy.OnCurrencyItemPurchased(productId, showMessage);

        ConsumeTransaction(transactionId);
    }
    
    public virtual bool IsInAppPurchaseEnabled()
    {
        //implemented in derrived classes
        return false;
    }

    public virtual void RetrieveProductList()
    {
        //implemented in derrived classes
    }

    public virtual void PurchaseProduct(string productId)
    {
        //implemented in derrived classes
    }

    public virtual void ConsumeTransaction(string transactionId)
	{
        //implemented in derrived classes
    }

    public virtual void RestorePurchases()
    {
        //implemented in derrived classes
    }

    public virtual bool HasMadeTransactions()
    {
        return false;
    }
    
    public void onProductReceived(string productId, string title, string description, string formattedPrice, 
        float price)
    {
        GameEconomy.onProductReceived(productId, title, description, formattedPrice, price);
    }

}
