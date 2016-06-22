using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class EconomyItemToText : MonoBehaviour {

    public GameEconomy.EconomyItemType type;
    Text myText;
    void Start()
    {
        myText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        myText.text = GameEconomy.boughtAmmount(type).ToString();
    }
}
