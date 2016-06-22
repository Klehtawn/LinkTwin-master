package com.amberstudio.amberlib;

import android.util.Log;

import com.samsung.android.sdk.iap.lib.helper.SamsungIapHelper;
import com.samsung.android.sdk.iap.lib.listener.OnGetItemListener;
import com.samsung.android.sdk.iap.lib.listener.OnPaymentListener;
import com.samsung.android.sdk.iap.lib.vo.ErrorVo;
import com.samsung.android.sdk.iap.lib.vo.ItemVo;
import com.samsung.android.sdk.iap.lib.vo.PurchaseVo;
import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;

public class SamsungIapController implements OnPaymentListener, OnGetItemListener {

    private static final String TAG = Constants.TAG;

    private static final int MODE = SamsungIapHelper.IAP_MODE_TEST_SUCCESS;
    private SamsungIapHelper mIapHelper = null;
    private String mUnityListenerName = null;

    public SamsungIapController(String listenerName)
    {
        mUnityListenerName = listenerName;
        mIapHelper = SamsungIapHelper.getInstance(UnityPlayer.currentActivity.getApplicationContext(), MODE);
    }

    public void StartPayment(String itemId)
    {
        mIapHelper.startPayment(itemId, true, this);
    }

    @Override
    public void onPayment(ErrorVo _errorVo, PurchaseVo _purchaseVo)
    {
        Log.i(TAG, "onPayment, _errorVo: " + _errorVo + ", _purchaseVo: " + _purchaseVo);
        if (_errorVo.getErrorCode() == SamsungIapHelper.IAP_ERROR_NONE) {
            UnityPlayer.UnitySendMessage(mUnityListenerName, "PurchaseSuccessful", _purchaseVo.getJsonString());
        } else {
            Log.i(TAG, "error code: " + _errorVo.getErrorCode() + " - " + _errorVo.getErrorString() + " - " + _errorVo.getExtraString());
            if (_errorVo.getErrorCode() == SamsungIapHelper.IAP_PAYMENT_IS_CANCELED)
                UnityPlayer.UnitySendMessage(mUnityListenerName, "PurchaseCanceled", "" + _errorVo.getErrorCode());
            else
                UnityPlayer.UnitySendMessage(mUnityListenerName, "PurchaseFailed", "" + _errorVo.getErrorCode());
        }

    }

    public void StartGetItemList(int start, int end, String type)
    {
        mIapHelper.getItemList(start, end, type, MODE, this);
    }

    @Override
    public void onGetItem(ErrorVo _errorVo, ArrayList<ItemVo> _itemList)
    {
        Log.i(TAG, "onGetItem, _errorVo: " + _errorVo + ", _itemList: " + _itemList);
        if (_errorVo.getErrorCode() == SamsungIapHelper.IAP_ERROR_NONE) {
            Log.i(TAG, "list size: " + _itemList.size());
            String result = "[";
            for (int i = 0; i < _itemList.size(); i++) {
                ItemVo item = _itemList.get(i);
                //Log.i(TAG, "\t" + item.getJsonString());
                result += item.getJsonString();
                result += i == _itemList.size() - 1 ? "]" : ",";
            }
            UnityPlayer.UnitySendMessage(mUnityListenerName, "ProductListReceived", result);
        }
    }
}
