package com.amberstudio.amberlib;

import android.os.Bundle;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

public class MainController {

    private static final String TAG = Constants.TAG;

    public static String GetIntentType()
    {
        Bundle extras = UnityPlayer.currentActivity.getIntent().getExtras();
        Log.i(TAG, "intent: " + UnityPlayer.currentActivity.getIntent());
        if (extras != null && extras.containsKey(Constants.INTENT_TYPE))
        {
            Log.i(TAG, "intent key found: " + extras.getString(Constants.INTENT_TYPE));
            return extras.getString(Constants.INTENT_TYPE);
        }
        else
        {
            Log.i(TAG, "intent key not found, returning NONE, extras: " + extras);
            return "NONE";
        }
    }
}
