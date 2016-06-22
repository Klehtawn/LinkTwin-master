using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Theme : MonoBehaviour
{
    public RectTransform desktopBackground;
    public RectTransform blackBackground;
    public RectTransform windowBackground;
    public RectTransform messageBoxBackground;
    public RectTransform messageBoxBackgroundSimple;
    public RectTransform panelBackground;

    public WindowA modalsTransparentBlack;

    public ButtonPressEffect buttonPressEffect;

    [Serializable]
    public class ButtonAppearance
    {
        public float preferredHeight = 60.0f;
        public float preferredWidth = 220.0f;
        public float preferredTotalWidth = 340.0f;

        [Range(0.05f, 1.0f)]
        public float preferredContentHeight = 0.5f;

        public RectTransform preferredBackground;
        public RectTransform specialBackground1;
        public RectTransform specialBackground2;
        public RectTransform specialBackground3;
        public RectTransform specialBackground4;
        public RectTransform specialBackground5;
    }

    public ButtonAppearance smallButtonAppearance = new ButtonAppearance();
    public ButtonAppearance wideButtonAppearance = new ButtonAppearance();

    [Serializable]
    public class GeneralAppearance
    {
        public Color background = Color.blue;
        public Color backgroundInactive = Color.blue;
        public Color foreground = Color.blue;
        public Color foregroundInactive = Color.blue;
        public Color desktop = Color.blue;
        public Color buttonBackground = Color.blue;
        public Color buttonForeground = Color.white;

        public Text font;

        public ShowEffect screenShowEffect;
        public CloseEffect screenCloseEffect;
    };

    public GeneralAppearance generalAppearance = new GeneralAppearance();

    [Serializable]
    public class PopupWindowAppearance
    {
        public Color background = Color.white;
        public Color foreground = Color.black;
        public Color buttonBackground = Color.blue;
        public Color buttonForeground = Color.white;
        public Text font;
        public ShowEffect showEffect;
        public CloseEffect closeEffect;
    };

    public PopupWindowAppearance popupWindowAppearance = new PopupWindowAppearance();

    [Serializable]
    public class OtherAppearanceSettings
    {
        public Color facebook = Color.blue;
        public Color link = Color.blue;
        public Color linkInactive = Color.blue;
        public Color specialColor1 = Color.blue;
        public Color specialColor2 = Color.blue;
        public Color specialColor3 = Color.blue;
        public Text specialFont1;
        public Text specialFont2;
        public Text specialFont3;
    }

    public OtherAppearanceSettings otherAppearanceSettings = new OtherAppearanceSettings();

    public SoundScheme soundScheme;

    void OnValidate()
    {
        _hasChanged = true;
    }

    private bool _hasChanged = false;
    public bool hasChanged
    {
        get
        {
            return _hasChanged;
        }
        set
        {
            _hasChanged = value;
        }
    }

    public void FadeFrom(Theme other, float fadeDuration)
    {
        this.fadeFrom = other;
        this.fadeDuration = fadeDuration;
        this.fadeTimer = 0.0f;
    }

    public bool isFading
    {
        get
        {
            return fadeFrom != null && fadeTimer <= fadeDuration && fadeDuration > 0.0f;
        }
    }

    private Theme fadeFrom = null;
    private float fadeDuration = -1.0f;
    private float fadeTimer = 0.0f;

    void Update()
    {
        if(isFading)
        {
            fadeTimer += Time.deltaTime;
        }
    }

    public Color GetInterpolatedColor(ThemedElement.Colors which)
    {
        if (fadeFrom == null || fadeDuration <= 0.0f)
        {
            switch (which)
            {
                case ThemedElement.Colors.Background: return generalAppearance.background;
                case ThemedElement.Colors.BackgroundInactive: return generalAppearance.backgroundInactive;
                case ThemedElement.Colors.Desktop: return generalAppearance.desktop;
                case ThemedElement.Colors.Facebook: return otherAppearanceSettings.facebook;
                case ThemedElement.Colors.Foreground: return generalAppearance.foreground;
                case ThemedElement.Colors.Link: return otherAppearanceSettings.link;
                case ThemedElement.Colors.LinkInactive: return otherAppearanceSettings.linkInactive;

                case ThemedElement.Colors.PopupBackground: return popupWindowAppearance.background;
                case ThemedElement.Colors.PopupForeground: return popupWindowAppearance.foreground;

                case ThemedElement.Colors.PopupButtonBackground: return popupWindowAppearance.buttonBackground;
                case ThemedElement.Colors.PopupButtonForeground: return popupWindowAppearance.buttonForeground;

                case ThemedElement.Colors.ButtonBackground: return generalAppearance.buttonBackground;
                case ThemedElement.Colors.ButtonForeground: return generalAppearance.buttonForeground;

                case ThemedElement.Colors.SpecialColor1: return otherAppearanceSettings.specialColor1;
                case ThemedElement.Colors.SpecialColor2: return otherAppearanceSettings.specialColor2;
                case ThemedElement.Colors.SpecialColor3: return otherAppearanceSettings.specialColor3;
            }
        }
        else
        {
            float fadeFactor = Mathf.Clamp01(fadeTimer / fadeDuration);
            switch (which)
            {
                case ThemedElement.Colors.Background: return Color.Lerp(fadeFrom.generalAppearance.background, generalAppearance.background, fadeFactor);
                case ThemedElement.Colors.BackgroundInactive: return Color.Lerp(fadeFrom.generalAppearance.backgroundInactive, generalAppearance.backgroundInactive, fadeFactor);
                case ThemedElement.Colors.Desktop: return Color.Lerp(fadeFrom.generalAppearance.desktop, generalAppearance.desktop, fadeFactor);
                case ThemedElement.Colors.Facebook: return Color.Lerp(fadeFrom.otherAppearanceSettings.facebook, otherAppearanceSettings.facebook, fadeFactor);
                case ThemedElement.Colors.Foreground: return Color.Lerp(fadeFrom.generalAppearance.foreground, generalAppearance.foreground, fadeFactor);
                case ThemedElement.Colors.Link: return Color.Lerp(fadeFrom.otherAppearanceSettings.link, otherAppearanceSettings.link, fadeFactor);
                case ThemedElement.Colors.LinkInactive: return Color.Lerp(fadeFrom.otherAppearanceSettings.linkInactive, otherAppearanceSettings.linkInactive, fadeFactor);

                case ThemedElement.Colors.PopupBackground: return Color.Lerp(fadeFrom.popupWindowAppearance.background, popupWindowAppearance.background, fadeFactor);
                case ThemedElement.Colors.PopupForeground: return Color.Lerp(fadeFrom.popupWindowAppearance.foreground, popupWindowAppearance.foreground, fadeFactor);

                case ThemedElement.Colors.PopupButtonBackground: return Color.Lerp(fadeFrom.popupWindowAppearance.buttonBackground, popupWindowAppearance.buttonBackground, fadeFactor);
                case ThemedElement.Colors.PopupButtonForeground: return Color.Lerp(fadeFrom.popupWindowAppearance.buttonForeground, popupWindowAppearance.buttonForeground, fadeFactor);

                case ThemedElement.Colors.ButtonBackground: return Color.Lerp(fadeFrom.generalAppearance.buttonBackground, generalAppearance.buttonBackground, fadeFactor);
                case ThemedElement.Colors.ButtonForeground: return Color.Lerp(fadeFrom.generalAppearance.buttonForeground, generalAppearance.buttonForeground, fadeFactor);

                case ThemedElement.Colors.SpecialColor1: return Color.Lerp(fadeFrom.otherAppearanceSettings.specialColor1, otherAppearanceSettings.specialColor1, fadeFactor);
                case ThemedElement.Colors.SpecialColor2: return Color.Lerp(fadeFrom.otherAppearanceSettings.specialColor2, otherAppearanceSettings.specialColor2, fadeFactor);
                case ThemedElement.Colors.SpecialColor3: return Color.Lerp(fadeFrom.otherAppearanceSettings.specialColor3, otherAppearanceSettings.specialColor3, fadeFactor);
            }
        }

        return Color.magenta;
    }
}
