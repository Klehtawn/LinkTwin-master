using UnityEngine;
using System.Collections;
using System;

public class DesktopUtils
{
    public static void ShowMessageBox(string txt, Action closeAction = null )
    {
        /*MessageBox msg = WindowA.Create("UI/MessageBox") as MessageBox;
        msg.message.text = txt;
        msg.ShowModal();
        msg.OnWindowClosed = (WindowA sender, int returnValue) => closeAction();*/

        ConfirmationBox.Show(txt, closeAction, null, "ok");

        //msg.OnDefaultButtonPressedAction = closeAction;
    }

    public static void ShowLocalizedMessageBox(string key, Action closeAction = null)
    {
        string msg = Locale.GetString(key);
        ShowMessageBox(msg, closeAction);
    }

    public static void ShowNotAvailable()
    {
        ShowMessageBox("NOT AVAILABLE IN THIS VERSION!");
    }

    static WindowA loadingIndicator = null;
    public static void ShowLoadingIndicator(bool showCancelButton = false, float percents = 0.0f, string txt = "")
    {
        if (loadingIndicator != null) return;

        LoadingThingy msg = WindowA.Create("UI/LoadingThingy") as LoadingThingy;
        msg.message.text = txt;
        msg.cancelButton.visible = showCancelButton;
        msg.ShowModal();

        loadingIndicator = msg;
    }

    public static void HideLoadingIndicator()
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.Close();
            loadingIndicator = null;
        }
    }
}
