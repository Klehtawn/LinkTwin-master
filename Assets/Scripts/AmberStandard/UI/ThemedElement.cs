using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ThemedElement : MonoBehaviour
{
    public enum Backgrounds
    {
        None,
        Desktop,
        Window,
        Button,
        MessageBox,
        MessageBoxSimple,
        WideButton,
        WideButtonCustom1,
        WideButtonCustom2,
        ButtonCustom1,
        ButtonCustom2,
        ButtonCustom3,
        Panel
    };

    public enum Colors
    {
        None,
        Desktop,
        Background,
        Foreground,
        BackgroundInactive,
        Link,
        LinkInactive,
        Facebook,
        PopupBackground,
        PopupForeground,
        ButtonBackground,
        ButtonForeground,
        PopupButtonBackground,
        PopupButtonForeground,
        SpecialColor1,
        SpecialColor2,
        SpecialColor3
    };

    public enum Fonts
    {
        None,
        GeneralFont,
        PopupWindowFont,
        SpecialFont1,
        SpecialFont2,
        SpecialFont3
    };

    public enum Effects
    {
        None,
        ButtonPress,
    }

    public enum ShowEffects
    {
        None,
        ScreenShowEffect,
        PopupShowEffect,
    }

    public enum CloseEffects
    {
        None,
        ScreenCloseEffect,
        PopupCloseEffect,
    }

    public enum Sprites
    {
        None,
        Background
    }

    public enum Dimensions
    {
        None,
        WideButtonHeight,
        WideButtonContentHeight,
        StdButtonSize,
        StdButtonContentSize,
        WideButtonWidthAndHeight,
        WideButtonWidthAndHeightTotal
    }

    //private ThemeElements prevAssign = ThemeElements.None;
    //public ThemeElements assign = ThemeElements.None;

    private Backgrounds prevSetBackground = Backgrounds.None;
    public Backgrounds setBackground = Backgrounds.None;

    private Colors prevSetColor = Colors.None;
    public Colors setColor = Colors.None;

    private Fonts prevSetFont = Fonts.None;
    public Fonts setFont = Fonts.None;

    private Effects prevSetEffect = Effects.None;
    public Effects setEffect = Effects.None;

    private ShowEffects prevShowEffect = ShowEffects.None;
    public ShowEffects showEffect = ShowEffects.None;

    private CloseEffects prevCloseEffect = CloseEffects.None;
    public CloseEffects closeEffect = CloseEffects.None;

    private Dimensions prevSetDimension = Dimensions.None;
    public Dimensions setDimension;

    private Text myText;
    private Image myImage;
    private RectTransform myRectTransform;

    protected virtual void Start()
    {
        myText = GetComponent<Text>();
        myImage = GetComponent<Image>();
        myRectTransform = GetComponent<RectTransform>();

        UpdateTheme();
    }

    private void UpdateBackgrounds()
    {
#if !UNITY_EDITOR
        return;
#endif
        if (prevSetBackground == setBackground) return;

        switch (setBackground)
        {
            case Backgrounds.None:
                {
                    DetachBackground();
                    break;
                }
            case Backgrounds.Window:
                {
                    AttachBackground(Desktop.main.theme.windowBackground);
                    break;
                }
            case Backgrounds.Button:
                {
                    AttachBackground(Desktop.main.theme.smallButtonAppearance.preferredBackground);
                    break;
                }
            case Backgrounds.ButtonCustom1:
                {
                    AttachBackground(Desktop.main.theme.smallButtonAppearance.specialBackground1);
                    break;
                }
            case Backgrounds.ButtonCustom2:
                {
                    AttachBackground(Desktop.main.theme.smallButtonAppearance.specialBackground2);
                    break;
                }
            case Backgrounds.ButtonCustom3:
                {
                    AttachBackground(Desktop.main.theme.smallButtonAppearance.specialBackground3);
                    break;
                }
            case Backgrounds.WideButton:
                {
                    AttachBackground(Desktop.main.theme.wideButtonAppearance.preferredBackground);
                    break;
                }
            case Backgrounds.WideButtonCustom1:
                {
                    AttachBackground(Desktop.main.theme.wideButtonAppearance.specialBackground1);
                    break;
                }
            case Backgrounds.WideButtonCustom2:
                {
                    AttachBackground(Desktop.main.theme.wideButtonAppearance.specialBackground2);
                    break;
                }
            case Backgrounds.MessageBox:
                {
                    AttachBackground(Desktop.main.theme.messageBoxBackground);
                    break;
                }
            case Backgrounds.MessageBoxSimple:
                {
                    AttachBackground(Desktop.main.theme.messageBoxBackgroundSimple);
                    break;
                }

            case Backgrounds.Panel:
                {
                    AttachBackground(Desktop.main.theme.panelBackground);
                    break;
                }
        }

        prevSetBackground = setBackground;
    }

    private void UpdateFonts()
    {
#if !UNITY_EDITOR
        if (prevSetFont == setFont) return;
#endif


        switch (setFont)
        {
            case Fonts.GeneralFont:
            case Fonts.PopupWindowFont:
            case Fonts.SpecialFont1:
            case Fonts.SpecialFont2:
            case Fonts.SpecialFont3:
                {
                    UpdateText(setFont);
                    break;
                }
        }

        prevSetFont = setFont;
    }

    private void UpdateColors(Colors which)
    {
#if !UNITY_EDITOR
        if (prevSetColor == which) return;
#endif

        UpdateBaseColor(which);
        
        prevSetColor = which;
    }

    private void UpdateEffects()
    {
        if (prevSetEffect == setEffect) return;

        switch (setEffect)
        {
            case Effects.ButtonPress:
                {
                    UpdateButtonPressEffect();
                    break;
                }
        }

        prevSetEffect = setEffect;
    }

    public virtual void UpdateTheme(bool force = false, ThemedElement mixWith = null)
    {
        if (Desktop.main == null) return;
        if (Desktop.main.theme == null) return;

        if(myText == null)
            myText = GetComponent<Text>();

        if(myImage == null)
            myImage = GetComponent<Image>();

        if(myRectTransform == null)
            myRectTransform = GetComponent<RectTransform>();

        if(force)
        {
            prevSetBackground = Backgrounds.None;
            prevSetFont = Fonts.None;
            prevSetColor = Colors.None;
            prevSetEffect = Effects.None;
        }

        Colors _setColor = setColor;

        if(mixWith != null)
        {
            /*if (setBackground == Backgrounds.None)
                setBackground = mixWith.setBackground;*/

            if (setFont == Fonts.None)
                setFont = mixWith.setFont;

            if (setColor == Colors.None)
                _setColor = mixWith.setColor;

            /*if (setEffect == Effects.None)
                setEffect = mixWith.setEffect;

            if(setDimension == Dimensions.None)
                setDimension = mixWith.setDimension;*/
        }

        setColor = _setColor;

        UpdateBackgrounds();
        UpdateFonts();
        UpdateColors(_setColor);
        UpdateEffects();
        UpdateDimensions();

        UpdateShowEffect();
        UpdateCloseEffect();
    }

    static public void SafeDel(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.SetParent(null);
            if (Application.isPlaying)
                GameObject.Destroy(obj);
            else
                GameObject.DestroyImmediate(obj);
        }
    }

    static public void RemoveComponent(Component c)
    {
        if (c != null)
        {         
            if (Application.isPlaying)
                Component.Destroy(c);
            else
                Component.DestroyImmediate(c);
        }
    }

    static public void SafeDel(Transform t)
    {
        if(t != null)
            SafeDel(t.gameObject);
    }

    static public GameObject CreateReference(GameObject src, bool force = false)
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            //if (force)
                return GameObject.Instantiate(src);
            //return null;
        }
        return UnityEditor.PrefabUtility.InstantiatePrefab(src) as GameObject;
#else
        if(force)
            return GameObject.Instantiate(src);
        return null;;
#endif
    }

    void DetachBackground()
    {
        // erase old bkg
        List<WidgetBackground> backgrounds = new List<WidgetBackground>();
        for (int i = 0; i < transform.childCount; i++)
        {
            WidgetBackground already = transform.GetChild(i).GetComponent<WidgetBackground>();
            if (already != null)
            {
                backgrounds.Add(already);
            }
        }

        foreach (WidgetBackground wb in backgrounds)
        {
            SafeDel(wb.gameObject);
        }
    }

    void AttachBackground(RectTransform bkgSrc)
    {
        if (bkgSrc == null) return;

        GameObject obj = CreateReference(bkgSrc.gameObject);

        if (obj == null) return;

        DetachBackground();

        obj.transform.SetParent(transform);
        obj.transform.localScale = Vector3.one;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = bkgSrc.anchoredPosition;
        rt.sizeDelta = bkgSrc.sizeDelta;

        obj.transform.SetAsFirstSibling();
    }

    void UpdateText(Fonts which)
    {
        Text src = Desktop.main.theme.generalAppearance.font;
        Text dest = myText;
        switch(which)
        {
            case Fonts.GeneralFont: src = Desktop.main.theme.generalAppearance.font; break;
            case Fonts.PopupWindowFont: src = Desktop.main.theme.generalAppearance.font; break;
            case Fonts.SpecialFont1: src = Desktop.main.theme.otherAppearanceSettings.specialFont1; break;
            case Fonts.SpecialFont2: src = Desktop.main.theme.otherAppearanceSettings.specialFont2; break;
            case Fonts.SpecialFont3: src = Desktop.main.theme.otherAppearanceSettings.specialFont3; break;
        }
        if (src == null || dest == null)
            return;

        dest.font = src.font;
        dest.fontSize = src.fontSize;
        dest.fontStyle = src.fontStyle;
        //dest.alignment = src.alignment;
        dest.horizontalOverflow = src.horizontalOverflow;
        dest.verticalOverflow = src.verticalOverflow;
        //dest.resizeTextForBestFit = src.resizeTextForBestFit;
        //dest.lineSpacing = src.lineSpacing;

        if (dest.transform.parent.GetComponent<LayoutGroup>() == null)
        {
            Vector2 p = dest.GetComponent<RectTransform>().anchoredPosition;
            p.x = src.GetComponent<RectTransform>().anchoredPosition.x;
            p.y = src.GetComponent<RectTransform>().anchoredPosition.y;
            dest.GetComponent<RectTransform>().anchoredPosition = p;
        }
        // etc
    }

#if !UNITY_EDITOR
    bool onlyFirstTime = true;
#endif

    public static void UpdateAppearance(Transform root, ThemedElement mixWith)
    {
        ThemedElement te = root.GetComponent<ThemedElement>();
        Colors prevSetColor = Colors.None;
        Fonts prevSetFont = Fonts.None;
        if (te != null)
        {
            prevSetColor = te.setColor;
            prevSetFont = te.setFont;
            te.UpdateTheme(false, mixWith);
        }

        for(int i = 0; i < root.transform.childCount; i++)
        {
            Transform c = root.transform.GetChild(i);
            if (c.gameObject.activeSelf)
            {
                UpdateAppearance(c, te != null ? te : mixWith);
            }
        }

        if (te != null)
        {
            te.setColor = prevSetColor;
            te.setFont = prevSetFont;
        }
    }

    void UpdateBaseColor(Colors which)
    {
        if(which != Colors.None)
            setColorRGB(Desktop.main.theme.GetInterpolatedColor(which));

        /*switch (which)
        {
            case Colors.Background: setColorRGB(Desktop.main.theme.backgroundColor); break;
            case Colors.BackgroundInactive: setColorRGB(Desktop.main.theme.backgroundColorInactive); break;
            case Colors.Foreground: setColorRGB(Desktop.main.theme.foregroundColor); break;
            case Colors.WindowText: setColorRGB(Desktop.main.theme.windowTextColor); break;
            case Colors.Desktop: setColorRGB(Desktop.main.theme.desktopColor); break;
            case Colors.Facebook: setColorRGB(Desktop.main.theme.facebookColor); break;
            case Colors.Link: setColorRGB(Desktop.main.theme.linkColor); break;
            case Colors.LinkInactive: setColorRGB(Desktop.main.theme.linkInactiveColor); break;
            case Colors.BaseColor1: setColorRGB(Desktop.main.theme.baseColor1); break;
            case Colors.BaseColor2: setColorRGB(Desktop.main.theme.baseColor2); break;
            case Colors.BaseColor3: setColorRGB(Desktop.main.theme.baseColor3); break;
            case Colors.BaseColor4: setColorRGB(Desktop.main.theme.baseColor4); break;
            case Colors.BaseColor5: setColorRGB(Desktop.main.theme.baseColor5); break;
        }
        */
    }

    Color getColor()
    {
        if (myImage != null)
            return myImage.color;

        if (myText != null)
            return myText.color;

        return Color.white;
    }

    void _setColor(Color c)
    {
        if (myImage != null)
        {
            myImage.color = c;
            return;
        }

        if (myText != null)
        { 
            myText.color = c;
            return;
        }
    }

    void setColorRGB(Color c)
    {
        Color cc = c;
        cc.a = getColor().a;
        _setColor(cc);
    }

    void UpdateButtonPressEffect()
    {
        ButtonPressEffect[] bpe = GetComponents<ButtonPressEffect>();
        foreach(ButtonPressEffect e in bpe)
            RemoveComponent(e);

        if(Desktop.main.theme.buttonPressEffect != null)
        {
            ButtonPressEffect c = gameObject.AddComponent(Desktop.main.theme.buttonPressEffect.GetType()) as ButtonPressEffect;
            c.CopyFrom(Desktop.main.theme.buttonPressEffect);
        }
    }

    void UpdateCloseEffect()
    {
        if (prevCloseEffect == closeEffect) return;

        CloseEffect[] ce = GetComponents<CloseEffect>();
        foreach (CloseEffect e in ce)
        {
            RemoveComponent(e);
        }

        if(closeEffect == CloseEffects.PopupCloseEffect && Desktop.main.theme.popupWindowAppearance.closeEffect != null)
        {
            CloseEffect e = gameObject.AddComponent(Desktop.main.theme.popupWindowAppearance.closeEffect.GetType()) as CloseEffect;
            e.Sync(Desktop.main.theme.popupWindowAppearance.closeEffect);
        }

        if (closeEffect == CloseEffects.ScreenCloseEffect && Desktop.main.theme.generalAppearance.screenCloseEffect != null)
        {
            CloseEffect e = gameObject.AddComponent(Desktop.main.theme.generalAppearance.screenCloseEffect.GetType()) as CloseEffect;
            e.Sync(Desktop.main.theme.generalAppearance.screenCloseEffect);
        }

        prevCloseEffect = closeEffect;
    }

    void UpdateShowEffect()
    {
        if (prevShowEffect == showEffect) return;

        ShowEffect[] se = GetComponents<ShowEffect>();
        foreach (ShowEffect e in se)
        {
            if(e != null)
                e.Restore();
            RemoveComponent(e);
        }

        if (showEffect == ShowEffects.PopupShowEffect && Desktop.main.theme.popupWindowAppearance.showEffect != null)
        {
            ShowEffect e = gameObject.AddComponent(Desktop.main.theme.popupWindowAppearance.showEffect.GetType()) as ShowEffect;
            e.Sync(Desktop.main.theme.popupWindowAppearance.showEffect);
        }

        if (showEffect == ShowEffects.ScreenShowEffect && Desktop.main.theme.generalAppearance.screenShowEffect != null)
        {
            ShowEffect e = gameObject.AddComponent(Desktop.main.theme.generalAppearance.screenShowEffect.GetType()) as ShowEffect;
            e.Sync(Desktop.main.theme.generalAppearance.screenShowEffect);
        }

        prevShowEffect = showEffect;
    }

    void UpdateDimensions()
    {
        //if (prevSetDimension == setDimension) return;

        if(setDimension == Dimensions.WideButtonHeight)
        {
            myRectTransform.sizeDelta = new Vector2(myRectTransform.sizeDelta.x, Desktop.main.theme.wideButtonAppearance.preferredHeight);
        }

        if (setDimension == Dimensions.WideButtonWidthAndHeight)
        {
            myRectTransform.sizeDelta = new Vector2(Desktop.main.theme.wideButtonAppearance.preferredWidth, Desktop.main.theme.wideButtonAppearance.preferredHeight);
        }

        if (setDimension == Dimensions.WideButtonWidthAndHeightTotal)
        {
            myRectTransform.sizeDelta = new Vector2(Desktop.main.theme.wideButtonAppearance.preferredTotalWidth, Desktop.main.theme.wideButtonAppearance.preferredHeight);
        }

        if(setDimension == Dimensions.WideButtonContentHeight)
        {
            myRectTransform.anchorMin = new Vector2(myRectTransform.anchorMin.x, 0.5f - Desktop.main.theme.wideButtonAppearance.preferredContentHeight * 0.5f);
            myRectTransform.anchorMax = new Vector2(myRectTransform.anchorMax.x, 0.5f + Desktop.main.theme.wideButtonAppearance.preferredContentHeight * 0.5f);

            myRectTransform.offsetMin = new Vector2(myRectTransform.offsetMin.x, 0.0f);
            myRectTransform.offsetMax = new Vector2(myRectTransform.offsetMax.x, 0.0f);
        }

        if (setDimension == Dimensions.StdButtonSize)
        {
            myRectTransform.sizeDelta = new Vector2(Desktop.main.theme.smallButtonAppearance.preferredHeight, Desktop.main.theme.smallButtonAppearance.preferredHeight);
        }

        if (setDimension == Dimensions.StdButtonContentSize)
        {
            myRectTransform.anchorMin = new Vector2(0.5f - Desktop.main.theme.smallButtonAppearance.preferredContentHeight * 0.5f, 0.5f - Desktop.main.theme.smallButtonAppearance.preferredContentHeight * 0.5f);
            myRectTransform.anchorMax = new Vector2(0.5f + Desktop.main.theme.smallButtonAppearance.preferredContentHeight * 0.5f, 0.5f + Desktop.main.theme.smallButtonAppearance.preferredContentHeight * 0.5f);
        }

        prevSetDimension = setDimension;
    }
}
