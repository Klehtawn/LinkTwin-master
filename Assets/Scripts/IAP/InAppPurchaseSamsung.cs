#if UNITY_ANDROID

using UnityEngine;

public class InAppPurchaseSamsung : InAppPurchase
{
    private SamsungIapEventListener listener;
    private static string javaClassName = "com.amberstudio.amberlib.SamsungIapController";
    AndroidJavaObject javaIapController = null;

    public InAppPurchaseSamsung()
    {
        listener = new GameObject("SamsungIapListener").AddComponent<SamsungIapEventListener>();
        javaIapController = new AndroidJavaObject(javaClassName, listener.gameObject.name);
        if (javaIapController != null)
        {
            Debug.Log("Initialized Samsung IAP Java class: " + javaIapController);
        }
        else
        {
            Debug.LogError("Unable to initialize Samsung IAP Java class " + javaClassName);
        }
    }

    public override void RetrieveProductList()
    {
        if (javaIapController == null) return;

        GameSession.IgnoreNextInterrupt();
        javaIapController.Call("StartGetItemList", 1, 512, "10");
    }

    public override void PurchaseProduct(string productId)
    {
        if (javaIapController == null) return;

        GameSession.IgnoreNextInterrupt();
        listener.mLastItemId = productId;
        javaIapController.Call("StartPayment", productId);
    }
}

#endif
