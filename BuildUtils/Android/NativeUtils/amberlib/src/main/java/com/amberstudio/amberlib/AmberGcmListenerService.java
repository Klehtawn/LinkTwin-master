package com.amberstudio.amberlib;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Bundle;
import android.support.v4.app.NotificationCompat;
import android.util.Log;

import com.google.android.gms.gcm.GcmListenerService;

public class AmberGcmListenerService extends GcmListenerService {

    private static final String TAG = Constants.TAG;

    @Override
    public void onMessageReceived(String from, Bundle data) {
        String message = data.getString("tickerText");
        String title = data.getString("title");
        Log.d(TAG, "From: " + from);
        Log.d(TAG, "Title: " + title);
        Log.d(TAG, "Message: " + message);
        Log.d(TAG, "Data size: " + data.size());
        Log.d(TAG, "tickerText: " + data.getString("tickerText"));
        Log.d(TAG, "title: " + data.getString("title"));
        Log.d(TAG, "subtitle: " + data.getString("subtitle"));
        for (String key: data.keySet()) {
            Log.d(TAG, "data[" + key + "] = " + data.get(key).toString());
        }

        sendNotification(title, message);
    }

    private void sendNotification(String title, String message) {

        String packageName = this.getPackageName();
        Intent launchIntent = this.getPackageManager().getLaunchIntentForPackage(packageName);
        launchIntent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
        launchIntent.putExtra(Constants.INTENT_TYPE, "PUSH");
        PendingIntent notificationPendingIntent = PendingIntent.getActivity(this, 0, launchIntent, PendingIntent.FLAG_ONE_SHOT | PendingIntent.FLAG_UPDATE_CURRENT);

        Resources res = this.getResources();

        int iconSmallId = res.getIdentifier("app_icon", "drawable", packageName);
        int iconLargeId = res.getIdentifier("app_icon", "drawable", packageName);

        Bitmap iconBitmap = BitmapFactory.decodeResource(res, iconLargeId);

        NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this)
                .setSmallIcon(iconSmallId)
                .setLargeIcon(iconBitmap)
                .setContentIntent(notificationPendingIntent)
                .setContentTitle(title)
                .setContentText(message)
                .setDefaults(Notification.DEFAULT_SOUND | Notification.DEFAULT_VIBRATE)
                .setAutoCancel(true);

        NotificationManager notificationManager = (NotificationManager)this.getSystemService(Context.NOTIFICATION_SERVICE);
        notificationManager.notify(1, notificationBuilder.build());
    }
}
