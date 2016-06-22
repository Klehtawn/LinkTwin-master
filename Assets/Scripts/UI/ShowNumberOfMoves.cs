using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowNumberOfMoves : MonoBehaviour {

    Text myText;
    int prevMovesNumber = -1;

	void Start () {
        myText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {

        if(TheGame.Instance != null)
        {
            int l = TheGame.Instance.moves;
            if (l != prevMovesNumber)
            {
                myText.text = l.ToString();
                prevMovesNumber = l;
            }
        }
        else
        {
            myText.text = "NA";
        }
	}
}
