using UnityEngine;
using System.Collections;

public class WorkshopSelectScreen : GameScreen {

    public Widget randomLevelButton;
    public Widget createNewButton;
    public Widget myLevelsButton;
    public Widget top100Button;
    public Widget favoritesButton;

	// Use this for initialization
	protected override void Start ()
    {
        base.Start();

        randomLevelButton.OnClick += OnRandomLevelPressed;
        myLevelsButton.OnClick += OnMyLevelsPressed;
        top100Button.OnClick += OnTop100Pressed;
        favoritesButton.OnClick += OnFavoritesPressed;
	}
	
	// Update is called once per frame
    protected override void Update()
    {
        base.Update();
	}

    void OnMyLevelsPressed(MonoBehaviour sender, Vector2 p)
    {
        if (GameSparksManager.Instance.Available())
        {
            Close();
            UserLevelsScreen screen = WindowA.Create("UI/UserLevelsScreen").GetComponent<UserLevelsScreen>();
            screen.title = "MY LEVELS";
            string userId = GameSparks.Core.GS.GSPlatform.UserId;
            screen.Request = "GetUserLevels";
            screen.Query = "{\"playerId\":\"" + userId + "\"}";
            screen.ButtonDetailsEnabled = true;
            screen.Show();
        }
        else
        {
            DesktopUtils.ShowLocalizedMessageBox("GS_CANNOT_CONNECT");
        }
    }

    void OnRandomLevelPressed(MonoBehaviour sender, Vector2 p)
    {
        GameSession.LoadRandomLevel(() =>
            {
                Close();
                WindowA.Create("UI/NormalPlayScreen").Show();
            });
    }

    void OnTop100Pressed(MonoBehaviour sender, Vector2 p)
    {
        if (GameSparksManager.Instance.Available())
        {
            Close();
            UserLevelsScreen screen = WindowA.Create("UI/UserLevelsScreen").GetComponent<UserLevelsScreen>();
            screen.title = "TOP 100";
            screen.Request = "GetUserLevels";
            screen.Query = "{}";
            screen.ButtonDetailsEnabled = false;
            screen.Show();
        }
        else
        {
            DesktopUtils.ShowLocalizedMessageBox("GS_CANNOT_CONNECT");
        }
    }

    void OnFavoritesPressed(MonoBehaviour sender, Vector2 p)
    {
        if (GameSparksManager.Instance.Available())
        {
            Close();
            UserLevelsScreen screen = WindowA.Create("UI/UserLevelsScreen").GetComponent<UserLevelsScreen>();
            screen.title = "FAVORITES";
            screen.Request = "GetFavouriteLevels";
            screen.Query = null;
            screen.ButtonDetailsEnabled = false;
            screen.Show();
        }
        else
        {
            DesktopUtils.ShowLocalizedMessageBox("GS_CANNOT_CONNECT");
        }
    }
}
