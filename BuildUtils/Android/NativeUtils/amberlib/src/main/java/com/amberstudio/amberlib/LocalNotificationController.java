package com.amberstudio.amberlib;

import android.app.AlarmManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.util.Calendar;

public class LocalNotificationController {

    private final static String TAG = Constants.TAG;

    public static void ScheduleNotification(String category, String title, String contents, long time) throws PackageManager.NameNotFoundException
    {
        Context unityContext = UnityPlayer.currentActivity.getApplicationContext();

        Intent alarmIntent = new Intent(unityContext, NotificationBroadcaster.class);
        alarmIntent.addCategory(category);
        alarmIntent.putExtra(Constants.NOTIFICATION_TITLE, title);
        alarmIntent.putExtra(Constants.NOTIFICATION_CONTENTS, contents);
        Log.i(TAG, "intent: " + alarmIntent);

        PendingIntent alarmPendingIntent = PendingIntent.getBroadcast(unityContext, 0, alarmIntent, PendingIntent.FLAG_CANCEL_CURRENT);

        Log.i(TAG, "pendingIntent: " + alarmPendingIntent);

        Calendar c = Calendar.getInstance();
        c.add(Calendar.SECOND, (int) time);
        long firstTime = c.getTimeInMillis();

        AlarmManager alarmManager = (AlarmManager)unityContext.getSystemService(Context.ALARM_SERVICE);
        alarmManager.set(AlarmManager.RTC_WAKEUP, firstTime, alarmPendingIntent);
        Log.i(TAG, "Alarm was set: " + time + " seconds");
    }

    public static void CancelNotification(String category)
    {
        Context unityContext = UnityPlayer.currentActivity.getApplicationContext();

        Intent alarmIntent = new Intent(unityContext, NotificationBroadcaster.class);
        alarmIntent.addCategory(category);

        PendingIntent alarmPendingIntent = PendingIntent.getBroadcast(unityContext, 0, alarmIntent, PendingIntent.FLAG_CANCEL_CURRENT);

        AlarmManager alarmManager = (AlarmManager)unityContext.getSystemService(Context.ALARM_SERVICE);
        alarmManager.cancel(alarmPendingIntent);
    }

}
