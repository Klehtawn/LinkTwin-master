package com.amberstudio.amberlib;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.support.v4.app.NotificationCompat;
import android.util.Log;

public class NotificationBroadcaster extends BroadcastReceiver {

    @Override
    public void onReceive(Context context, Intent intent)
    {
        Log.i("[ALARM]", "Alarm triggered");

        String packageName = context.getPackageName();
        Intent launchIntent = context.getPackageManager().getLaunchIntentForPackage(packageName);
        launchIntent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
        launchIntent.putExtra(Constants.INTENT_TYPE, "LOCAL");
        PendingIntent notificationPendingIntent = PendingIntent.getActivity(context, 0, launchIntent, PendingIntent.FLAG_UPDATE_CURRENT);
        Resources res = context.getResources();

        int iconSmallId = res.getIdentifier("app_icon", "drawable", packageName);
        int iconLargeId = res.getIdentifier("app_icon", "drawable", packageName);

        Bitmap iconBitmap = BitmapFactory.decodeResource(res, iconLargeId);

        NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(context)
                .setSmallIcon(iconSmallId)
                .setLargeIcon(iconBitmap)
                .setContentIntent(notificationPendingIntent)
                .setContentTitle(intent.getStringExtra(Constants.NOTIFICATION_TITLE))
                .setContentText(intent.getStringExtra(Constants.NOTIFICATION_CONTENTS))
                .setDefaults(Notification.DEFAULT_SOUND | Notification.DEFAULT_VIBRATE)
                .setAutoCancel(true);

        NotificationManager notificationManager = (NotificationManager)context.getSystemService(Context.NOTIFICATION_SERVICE);
        notificationManager.notify(0, notificationBuilder.build());
    }
}
