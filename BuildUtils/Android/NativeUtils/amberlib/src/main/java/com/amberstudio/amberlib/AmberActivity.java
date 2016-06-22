package com.amberstudio.amberlib;

import android.content.Intent;

import com.unity3d.player.UnityPlayerActivity;

public class AmberActivity extends UnityPlayerActivity {

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
    }
}
