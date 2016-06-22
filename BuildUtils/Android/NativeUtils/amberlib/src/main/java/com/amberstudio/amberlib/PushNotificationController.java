package com.amberstudio.amberlib;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;
import android.support.v4.content.LocalBroadcastManager;
import android.util.Log;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GoogleApiAvailability;
import com.unity3d.player.UnityPlayer;

public class PushNotificationController {

    private static final String TAG = Constants.TAG;

    private static BroadcastReceiver sRegistrationBroadcastReceiver;
    private static boolean isReceiverRegistered;
    private static String sGcmSenderId;
    private static String sUnityListenerName;

    public static void InitRegistrationReceiver(String senderId, String listenerName)
    {
        sGcmSenderId = senderId;
        sUnityListenerName = listenerName;

        Log.i(TAG, "InitRegistrationReceiver(" + senderId + ", " + listenerName + ")");

        sRegistrationBroadcastReceiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(context);
                boolean sentToken = sharedPreferences.getBoolean(Constants.SENT_TOKEN_TO_SERVER, false);
                Log.i(TAG, "onReceive, sentToken: " + sentToken);
            }
        };
        Log.i(TAG, "new receiver: " + sRegistrationBroadcastReceiver);

        RegisterReceiver();

        GoogleApiAvailability apiAvailability = GoogleApiAvailability.getInstance();
        if (apiAvailability.isGooglePlayServicesAvailable(UnityPlayer.currentActivity) == ConnectionResult.SUCCESS) {
            Intent intent = new Intent(UnityPlayer.currentActivity, RegistrationIntentService.class);
            UnityPlayer.currentActivity.startService(intent);
            Log.i(TAG, "Started registration service");
        } else {
            Log.e(TAG, "Google Play Services not available");
        }
    }

    private static void RegisterReceiver()
    {
        if (!isReceiverRegistered) {
            LocalBroadcastManager.getInstance(UnityPlayer.currentActivity.getApplicationContext()).registerReceiver(sRegistrationBroadcastReceiver,
                    new IntentFilter(Constants.REGISTRATION_COMPLETE));
            isReceiverRegistered = true;
        }
    }

    public static String GetSenderId()
    {
        return sGcmSenderId;
    }

    public static void ReportGcmTokenToUnity(String key)
    {
        Log.i(TAG, "Sending Gcm token to Unity");
        UnityPlayer.UnitySendMessage(sUnityListenerName, "SetGcmToken", key);
    }

}
