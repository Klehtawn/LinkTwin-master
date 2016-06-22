using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChapterButton : Button {

    public int levelIndex = 0;
    public int levelIndexOffset = 0;

    public int starsCount = 1;

    public Transform starsEmpty, starsFull;

    public Transform playedVersion;
    public Transform unlockedVersion;
    public Transform lockedVersion;

    public enum ButtonState
    {
        Played,
        Locked,
        Unlocked
    }

    Text unlockedCaption;
	
    protected override void Start()
    {
        base.Start();

        unlockedCaption = unlockedVersion.GetComponentInChildren<Text>();

        //highlight = false;

        Refresh();
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
	}

    public void Refresh()
    {
        string levelNumber = (levelIndex + levelIndexOffset + 1).ToString();
        if (caption != null)
            caption.text = levelNumber;

        if (unlockedCaption != null)
            unlockedCaption.text = levelNumber;

        UpdateStars();
    }

    void UpdateStars()
    {
        if (starsFull != null)
        {
            for (int i = 0; i < starsFull.childCount; i++)
            {
                Transform c = starsFull.GetChild(i);
                c.gameObject.SetActive(i < starsCount);
            }
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        UpdateStars();
    }

    public void SetButtonState(ButtonState s)
    {
        if(s == ButtonState.Played)
        {
            playedVersion.gameObject.SetActive(true);
            lockedVersion.gameObject.SetActive(false);
            unlockedVersion.gameObject.SetActive(false);
        }
        else if (s == ButtonState.Locked)
        {
            playedVersion.gameObject.SetActive(false);
            lockedVersion.gameObject.SetActive(true);
            unlockedVersion.gameObject.SetActive(false);
        }
        else if (s == ButtonState.Unlocked)
        {
            playedVersion.gameObject.SetActive(false);
            lockedVersion.gameObject.SetActive(false);
            unlockedVersion.gameObject.SetActive(true);
        }
    }
}
