using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Leaderboard : MonoBehaviour
{
    //public struct LeaderboardRow
    //{
    //    public int rank;
    //    public string name;
    //    public string score;

    //    public LeaderboardRow(int r, string n, string s)
    //    {
    //        rank = r;
    //        name = n;
    //        score = s;
    //    }
    //}

    //public GameObject itemPrefab;

    //public SimpleWidget backButton;

    //protected List<LeaderboardRow> entries = new List<LeaderboardRow>();

    //void Awake()
    //{

    //}

    //// Use this for initialization
    //void Start()
    //{
    //    SetupEnvironment();

    //    if (GameSparksManager.Instance.Available())
    //    {
    //        new GameSparks.Api.Requests.LeaderboardDataRequest().SetLeaderboardShortCode("HighLevelLeaderboard").SetEntryCount(10).Send((response) =>
    //        {
    //            if (response.HasErrors)
    //            {
    //                Debug.LogWarning("error retrieving leaderboard");
    //            }
    //            else
    //            {
    //                Debug.Log("leaderboards retrieved successfully");
    //                entries.Clear();
    //                foreach (GameSparks.Api.Responses.LeaderboardDataResponse._LeaderboardData entry in response.Data)
    //                {
    //                    entries.Add(new LeaderboardRow((int)entry.Rank, entry.UserName, entry.JSONData["LEVEL"].ToString()));
    //                    Debug.Log("Rank:" + (int)entry.Rank + " Name:" + entry.UserName + " \n Score:" + entry.JSONData["LEVEL"].ToString());
    //                }
    //                FillItems();
    //            }
    //        });
    //    }

    //    backButton.OnTouchUp += BackToMain;
    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    //void BackToMain(MonoBehaviour sender)
    //{
    //}

    //void SetupEnvironment()
    //{
    //    float aspect = (float)Screen.width / (float)Screen.height;
    //    Camera.main.orthographicSize = 50.0f / aspect;
    //}

    //void FillItems()
    //{
    //    Canvas canvasRoot = transform.GetComponentInChildren<Canvas>();

    //    float canvasWidth = (float)canvasRoot.GetComponent<RectTransform>().rect.width;

    //    //int numLevels = GameSession.GetNumLevels();

    //    float offsetX = 10.0f;

    //    //float buttonWidth = itemPrefab.GetComponent<RectTransform>().sizeDelta.x * 1.05f;
    //    float buttonHeight = itemPrefab.GetComponent<RectTransform>().sizeDelta.y * 1.05f;

    //    //float numColumns = Mathf.Floor((canvasWidth - 2.0f * offsetX) / buttonSize);

    //    offsetX = canvasWidth * 0.5f;

    //    //float x = offsetX;
    //    float y = canvasRoot.GetComponent<RectTransform>().rect.height - offsetX;


    //    for (int i = 0; i < entries.Count; i++)
    //    {
    //        GameObject obj = GameObject.Instantiate<GameObject>(itemPrefab);
    //        obj.transform.SetParent(canvasRoot.transform);
    //        obj.transform.localScale = Vector3.one;

    //        obj.GetComponent<RectTransform>().anchorMin = Vector3.zero;
    //        obj.GetComponent<RectTransform>().anchorMax = Vector3.zero;
    //        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(canvasWidth * 0.5f, y - buttonHeight * 0.5f);

    //        //x += buttonSize;
    //        //if (x > canvasWidth - buttonSize)
    //        //{
    //        //    x = offsetX;
    //        //    y -= buttonSize;
    //        //}
    //        y -= buttonHeight;

    //        int level = i + 1;

    //        LeaderboardEntry button = obj.GetComponent<LeaderboardEntry>();

    //        //button.levelIndex = level;
    //        obj.name = "Entry" + (i + 1).ToString();
    //        //button.content = "" + entries[i].name + " reached level " + entries[i].score;
    //        LeaderboardRow row = entries[i];
    //        button.rank = row.rank;
    //        button.player = row.name;
    //        button.level = "max level: " + row.score;

    //        if (level > GameSession.GetMaxUnlockedLevel())
    //            button.active = false;

    //        button.OnTouchUp = OnButtonTouchUp;
    //    }
    //}

    //void OnButtonTouchUp(MonoBehaviour sender)
    //{
    //    SagaButton button = sender.GetComponent<SagaButton>();
    //    if (button != null)
    //    {
    //        StartCoroutine(StartLevel(button.levelIndex));
    //    }
    //}

    //IEnumerator StartLevel(int index)
    //{
    //    yield return new WaitForSeconds(0.4f);
    //    //GameSession.LoadLevel(index);
    //}
}
