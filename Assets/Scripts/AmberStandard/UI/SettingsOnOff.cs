using UnityEngine;
using System.Collections;
using System;

public class SettingsOnOff : MonoBehaviour {

    public string setting;

	void Start () {

        firstTime = true;
	}

    bool firstTime = true;
    void Update()
    {
        if (firstTime)
        {
            OnOffButton oob = GetComponent<OnOffButton>();

            if (setting != null && oob != null)
            {
                oob.isOn = PlayerPrefs.GetInt(setting, 1) == 1;
                oob.UpdateState();
                oob.OnStateChanged += OnButtonStateChanged;
            }

            firstTime = false;
        }
    }
	
	void OnButtonStateChanged()
    {
        OnOffButton oob = GetComponent<OnOffButton>();

        Debug.Log(setting + " is " + oob.isOn);
        PlayerPrefs.SetInt(setting, oob.isOn ? 1 : 0);
        PlayerPrefs.Save();

        Desktop.main.sounds.UpdateFromSettings();

        if (OnSettingChanged != null)
            OnSettingChanged(this);
    }

    public Action<MonoBehaviour> OnSettingChanged;
}
