using UnityEngine;
using System.Collections;
using System.Xml;
using System;
using System.Collections.Generic;
using System.IO;

#if UNITY_ANDROID
using System.Runtime.InteropServices;
#endif

public class Locale
{
    private static string currentLocale = null;
    private static Dictionary<string, string> texts = new Dictionary<string, string>();

    public static string GetString(string key)
    {
#if UNITY_EDITOR
        if (texts.Count == 0)
        {
            LoadLocale("en_US");
        }
#endif

        if (texts.ContainsKey(key))
        {
            return texts[key];
        }
        else
        {
            //Debug.Log("[Locale] string not found: " + key);
            return key + "*";
        }
    }

    public static bool StringExists(string key)
    {
#if UNITY_EDITOR
        if (texts.Count == 0)
        {
            LoadLocale("en_US");
        }
#endif

        return texts.ContainsKey(key);
    }

    public static string GetCurrentLocale()
    {
        if (string.IsNullOrEmpty(currentLocale))
        {
            LoadLocale(DetectLocale());
        }
        return currentLocale;
    }

    public static void LoadLocale(string locale)
    {
        texts.Clear();
        string new_locale = locale;

        // --- load local strings and add texts that aren't present on AMPS
        string file_name = "Texts/texts_" + locale;

        string txt = ContentManager.LoadTextFile(file_name);

        // --- default to english if localized pack doesn't exist
        if (txt == null)
        {
            Debug.Log("Cannot load texts for locale " + locale + " from file " + file_name);
            file_name = "Texts/texts_en_US";
            txt = ContentManager.LoadTextFile(file_name);
            if (txt == null)
            {
                return;
                //Debug.Log("Cannot load default texts from file " + file_name);
            }
        }

        string[] strings = txt.Split('\n');

        for (int i = 1; i < strings.Length - 2; i += 2)
        {
            string key = strings[i];
            string val = strings[i + 1].Replace("\\n", "\n");
            texts.Add(key, val);
        }

        Debug.Log("Loaded locale " + new_locale + ", " + texts.Count + " strings");

        currentLocale = new_locale;

        FixColors();
    }

    static void FixColors()
    {
        string color1 = ColorUtility.ToHtmlStringRGBA(Desktop.main.theme.generalAppearance.background);
        
        color1 = "<color=#" + color1 + ">";

        List<string> keys = new List<string>(texts.Keys);
        foreach (string k in keys)
        {
            string v = texts[k];
            if (v != null && v.Contains("<color1>"))
            {
                v = v.Replace("</color1>", "</color>");
                v = v.Replace("<color1>", color1);
                texts[k] = v;
            }
        }
    }

    public static void ChangeLocale(string newLocale)
    {
        if (newLocale != currentLocale)
        {
            LoadLocale(newLocale);
        }
    }

    public static string DetectLocale()
    {
#if UNITY_EDITOR

        string path = Path.Combine(Application.dataPath, "locale.txt");
        string locale_string = "en_US";

        if (File.Exists(path))
        {
            TextReader reader = File.OpenText(path);
            do
            {
                locale_string = reader.ReadLine();
                locale_string.Trim();
            }
            while (locale_string.StartsWith("//"));
        }

        return locale_string;

#elif UNITY_IPHONE

        string language = PlayerPrefs.GetString("language");
        string locale = PlayerPrefs.GetString("locale");

        Debug.Log("language = " + language);
        Debug.Log("locale = " + locale);
        
        if (language.StartsWith("en"))
            return "en_US";

        if (language.StartsWith("fr"))
            return "fr_FR";

        if (language.StartsWith("it"))
            return "it_IT";

        if (language.StartsWith("de"))
            return "de_DE";

        if (language.StartsWith("es"))
            return "es_ES";

        if (language.StartsWith("pt"))
            return "pt_BR";

        if (language.StartsWith("ru"))
            return "ru_RU";

        if (language.StartsWith("ja"))
            return "ja_JP";

        if (language.StartsWith("ko"))
            return "ko_KR";

        if (language.StartsWith("zh"))
        {
            if (language.StartsWith("zh-Hant"))
                return "zh_TW";
            else
                return "zh_CN";
        }

        return "en_US";
		
#elif UNITY_METRO || UNITY_WP8
		
		string locale = "";//WinNativeWrapper.GetLanguage();
		//Debug.Log("locale = " + locale);

		SystemLanguage language = Application.systemLanguage;
		//Debug.Log("language = " + language);
		
		switch (language)
		{
		case SystemLanguage.English:
			return "en_US";
			
		case SystemLanguage.French:
			return "fr_FR";
			
		case SystemLanguage.Italian:
			return "it_IT";
			
		case SystemLanguage.German:
			return "de_DE";
			
		case SystemLanguage.Spanish:
    		return "es_ES";
			
		case SystemLanguage.Portuguese:
			return "pt_BR";
			
		case SystemLanguage.Russian:
			return "ru_RU";
			
		case SystemLanguage.Japanese:
			return "ja_JP";
			
		case SystemLanguage.Korean:
			return "ko_KR";
			
		case SystemLanguage.Chinese:
			if (locale.StartsWith("zh-Hant") || locale.StartsWith("zh-TW"))
				return "zh_TW";
			else
				return "zh_CN";
			
		default:
			return "en_US";
		}
		
#elif UNITY_ANDROID
		
        AndroidJavaClass javaLocaleClass = new AndroidJavaClass("java.util.Locale");
		AndroidJavaObject defaultLocale = javaLocaleClass.CallStatic< AndroidJavaObject >("getDefault");

		string locale = string.Format("{0}_{1}", defaultLocale.Call<string>("getLanguage"), defaultLocale.Call<string>("getCountry"));
		//Debug.Log("Native_GetLanguage locale = " + locale);

        SystemLanguage language = Application.systemLanguage;
		//Debug.Log("Native_GetLanguage language = " + language);

        switch (language)
        {
            case SystemLanguage.English:
                return "en_US";

            case SystemLanguage.French:
                return "fr_FR";

            case SystemLanguage.Italian:
                return "it_IT";

            case SystemLanguage.German:
                return "de_DE";

            case SystemLanguage.Spanish:
    			return "es_ES";

            case SystemLanguage.Portuguese:
                return "pt_BR";

            case SystemLanguage.Russian:
                return "ru_RU";

            case SystemLanguage.Japanese:
                return "ja_JP";

            case SystemLanguage.Korean:
                return "ko_KR";

            case SystemLanguage.Chinese:
				if (locale.StartsWith("zh_TW"))
					return "zh_TW";
				else
					return "zh_CN";

            default:
                return "en_US";
        }
		
#else

        SystemLanguage language = Application.systemLanguage;

        switch (language)
        {
            case SystemLanguage.English:
                return "en_US";

            case SystemLanguage.French:
                return "fr_FR";

            case SystemLanguage.Italian:
                return "it_IT";

            case SystemLanguage.German:
                return "de_DE";

            case SystemLanguage.Spanish:
                return "es_ES";

            case SystemLanguage.Portuguese:
                return "pt_BR";

            case SystemLanguage.Russian:
                return "ru_RU";

            case SystemLanguage.Japanese:
                return "ja_JP";

            case SystemLanguage.Korean:
                return "ko_KR";

            case SystemLanguage.Chinese:
                return "zh_CN";

            default:
                return "en_US";
        }

#endif
    }
}
