using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class WatchVideoAdBox : MonoBehaviour {

    public int reward = 100;

    public Text rewardText;

    public Widget watchVideoButton;
    public GameObject congratsText;

    private int prevReward = -1;

    void Start()
    {
        congratsText.SetActive(false);
        watchVideoButton.OnTouchUp += OnVideoButtonPress;
	}
	
	void Update()
    {
        if(reward != prevReward)
        {
            rewardText.text = "+" + reward.ToString();
            prevReward = reward;
        }
	}

    void OnVideoButtonPress(MonoBehaviour sender, Vector2 p)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        AdUtils.rewardShowEvent += OnVideoWatched;
        AdUtils.Instance.InitVideo(AdUtils.Instance.GetAdId(AdUtils.Location.UnlockBonus));
#else
        OnVideoWatched(true);
#endif
    }

    void OnVideoWatched(bool success)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        AdUtils.rewardShowEvent -= OnVideoWatched;
#endif

        GameEconomy.currency += reward;
        watchVideoButton.gameObject.SetActive(false);
        congratsText.SetActive(true);
        GameEconomy.OnVideoAdWatched();
        GameEconomy.SaveLocal();
    }
}
