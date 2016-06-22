using UnityEngine;
using System.Collections;

public class WorkshopPlayScreen : PlayScreen
{

    protected override void Start()
    {
        base.Start();

        OnWindowStartClosing += OnScreenStartClosing;
    }
    protected override void OnGameEnded(bool hasWon)
    {
        StartCoroutine(_OnGameEnded(hasWon, 0.5f));
    }

    IEnumerator _OnGameEnded(bool hasWon, float delay)
    {
        yield return new WaitForSeconds(delay);

        WindowA w = null;

        if (hasWon)
        {
            w = WindowA.Create("UI/WorkshopLevelCompleteBox");
            w.OnWindowStartClosing += OnGameWonClosed;
        }
        else
        {
            w = WindowA.Create("UI/GameLostBox");
            w.OnWindowStartClosing += OnGameLostClosed;
        }

        w.ShowModal(true);
    }

    void OnGameWonClosed(WindowA sender, int retValue)
    {
        if (retValue == (int)WorkshopLevelCompleteBox.ReturnValues.PressedUpload)
        {
            ConfirmationBox.Show(Locale.GetString("UPLOAD_LEVEL_CONFIRM"), DoUpload, ()=>Desktop.main.windowsFlow.Backward());
        }
    }

    void DoUpload()
    {
        string encodedLevel = System.Convert.ToBase64String(GameSession.customLevelBytes);

        if (GameSparksManager.Instance.Available())
        {
            GameSparks.Api.Requests.LogEventRequest req = new GameSparks.Api.Requests.LogEventRequest();
            req.SetEventKey("PublishLevel");
            req.SetEventAttribute("LevelData", encodedLevel);
            DesktopUtils.ShowLoadingIndicator();
            req.Send((response) =>
            {
                DesktopUtils.HideLoadingIndicator();
                if (response.HasErrors)
                {
                    //Desktop.main.windowsFlow.Backward();
                    DesktopUtils.ShowLocalizedMessageBox("LEVEL_UPLOAD_FAILED");
                    Debug.LogWarning("publish level failed");
                }
                else
                {
                    Desktop.main.windowsFlow.Backward(0, false);
                    Desktop.main.windowsFlow.Backward(0, false);
                    UserLevelsScreen screen = WindowA.Create("UI/UserLevelsScreen").GetComponent<UserLevelsScreen>();
                    screen.title = "MY LEVELS";
                    string userId = GameSparks.Core.GS.GSPlatform.UserId;
                    screen.Request = "GetUserLevels";
                    screen.Query = "{\"playerId\":\"" + userId + "\"}";
                    screen.ButtonDetailsEnabled = true;
                    screen.Show();
                    string id = response.ScriptData.GetString("levelId");
                    Debug.Log("publish level success, level id: " + id);
                }
            });
        }
        else
        {
            DesktopUtils.ShowLocalizedMessageBox("GS_CANNOT_CONNECT");
        }
    }


    void OnGameLostClosed(WindowA sender, int retValue)
    {
        if (retValue == (int)GameLostBox.ReturnValues.PressedRetry)
        {
            RetryLevel();
        }
    }

    void OnScreenStartClosing(WindowA w, int ret)
    {
        gameArea.gameObject.SetActive(false);
    }
}
