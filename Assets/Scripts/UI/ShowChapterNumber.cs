using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowChapterNumber : MonoBehaviour {

    Text myText;
    int prevLevelChapter = -1;

    public bool useRomanNumbers = true;

	void Start () {
        myText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {

        if (GameSession.IsUserLevel() == false)
        {
            int l = GameSession.currentChapter + 1;
            if (l != prevLevelChapter)
            {
                if (useRomanNumbers)
                    myText.text = RomanNumerals.FromArabic(l);
                else
                    myText.text = l.ToString();
                prevLevelChapter = l;
            }
        }
        else
        {
            myText.text = "X";
        }
	}
}
