using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LeaderboardEntry : SimpleWidget
{
    public int rank = 0;
    public string player = string.Empty;
    public string level = string.Empty;
    public Text captionRank;
    public Text captionPlayer;
    public Text captionLevel;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        if (captionRank != null)
            captionRank.text = "" + rank;
        if (captionPlayer != null)
            captionPlayer.text = "" + player;
        if (captionLevel != null)
            captionLevel.text = "" + level;
    }

    // Update is called once per frame
    protected override void Update()
    {

        base.Update();
    }
}
