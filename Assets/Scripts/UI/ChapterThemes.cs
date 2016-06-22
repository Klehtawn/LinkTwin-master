using UnityEngine;
using System.Collections;
using System;

public class ChapterThemes : MonoBehaviour {

    [Serializable]
    public class ChapterTheme
    {
        public RectTransform chapterIntro;
        public RectTransform chapterBackground;
        public Transform ingameBackground;
    }

    public ChapterTheme[] themes;

    private static ChapterThemes themeLoaded = null;

    public static ChapterTheme GetThemeForChapter(int index)
    {
        if (themeLoaded == null)
            themeLoaded = Resources.Load<ChapterThemes>("ChapterThemes");

        if (index >= themeLoaded.themes.Length) return null;
        if (index < 0) return null;

        return themeLoaded.themes[index];
    }

    public static RectTransform GetChapterIntro(int index)
    {
        ChapterTheme ct = GetThemeForChapter(index);
        return ct != null ? ct.chapterIntro : null;
    }

    public static RectTransform GetChapterBackground(int index)
    {
        ChapterTheme ct = GetThemeForChapter(index);
        return ct != null ? ct.chapterBackground : null;
    }

    public static Transform GetChapterInGameBackground(int index)
    {
        ChapterTheme ct = GetThemeForChapter(index);
        return ct != null ? ct.ingameBackground : null;
    }
}
