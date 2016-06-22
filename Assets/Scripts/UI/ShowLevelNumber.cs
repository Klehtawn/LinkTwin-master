using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowLevelNumber : MonoBehaviour {

    Text myText;
    int prevLevelNumber = -1;

    public string prefix = "";

    public bool showChapter = true;

	void Start () {
        myText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {

        if (GameSession.IsUserLevel() == false)
        {
            int l = GameSession.currentPlaying + 1 + GameSession.GetStartIndexForChapter(GameSession.currentChapter);
            if (l != prevLevelNumber)
            {
                if(showChapter)
                    myText.text = RomanNumerals.FromArabic(GameSession.currentChapter + 1) + " . " + (GameSession.currentPlaying + 1).ToString();
                else
                    myText.text = prefix + (GameSession.currentPlaying + 1).ToString();
                prevLevelNumber = l;
            }
        }
        else
        {
            myText.text = GameSession.customLevelId;
        }
	}
}
