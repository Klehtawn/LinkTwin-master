using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class BuildNumber : MonoBehaviour
{
    void Start()
    {
        Text myText = GetComponent<Text>();
        myText.text = GameSession.GetBuildInfo();
    }
}
