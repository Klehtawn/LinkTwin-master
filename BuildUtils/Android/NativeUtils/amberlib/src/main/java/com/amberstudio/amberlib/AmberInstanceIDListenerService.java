package com.amberstudio.amberlib;

import android.content.Intent;
import android.util.Log;

import com.google.android.gms.iid.InstanceIDListenerService;
import com.unity3d.player.UnityPlayer;

public class AmberInstanceIDListenerService extends InstanceIDListenerService{

    private final static String TAG = Constants.TAG;

    @Override
    public void onTokenRefresh() {
        Intent intent = new Intent(UnityPlayer.currentActivity, RegistrationIntentService.class);
        startService(intent);
        Log.i(TAG, "onTokenRefresh");
    }

}
