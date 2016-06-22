using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuotesScreen : GameScreen {

    private GameObject previousBackground;
    public GameObject preferredBackground;

    public TextAnchor preferredAlignment = TextAnchor.MiddleRight;

    public Vector2 preferredPos = Vector2.zero;
    public Vector2 preferredSize = Vector2.one;
    
    private RectTransform textBox;
    private Text text;

    public string message;
    public string author;
    public bool italicText = true;

    int showBackground = 2;

    public TweenColor fadeToBlack;

	// Use this for initialization
	protected override void Start () {
        base.Start();

        OnWindowStartClosing += WindowIsClosing;

        if(fadeToBlack != null)
            fadeToBlack.OnCompleted += OnStartFadeCompleted;
	}

    void OnStartFadeCompleted()
    {
        fadeToBlack.OnCompleted -= OnStartFadeCompleted;
        fadeToBlack.gameObject.SetActive(false);
    }

    void getTextObjs()
    {
        if (textBox == null)
        {
            Transform t = transform.Find("TextBox");
            if (t != null)
            {
                textBox = transform.Find("TextBox").GetComponent<RectTransform>();
                Text[] allText = textBox.GetComponentsInChildren<Text>();
                foreach(Text txt in allText)
                {
                    if (txt.gameObject.name != "Shadow")
                    {
                        text = txt;
                        break;
                    }
                }
            }
        }
    }

    void UpdateTextParams()
    {
        getTextObjs();

        if (text == null) return;

        text.alignment = preferredAlignment;

        textBox.anchorMin = preferredPos;
        textBox.anchorMax = preferredPos + preferredSize;

        Color c = text.color;
        c *= 0.8f;

        byte rByte = (byte)(c.r * 256);
        byte gByte = (byte)(c.g * 256);
        byte bByte = (byte)(c.b * 256);
        byte aByte = (byte)(c.a * 256);

        string rgbStr = rByte.ToString("X2") + gByte.ToString("X2") + bByte.ToString("X2") + aByte.ToString("X2");

        string txt = message; 
        
        if(author != null && author.Length > 0)
        {
            txt += "\n\n" + "<color=#" + rgbStr + ">" + author + "</color>";
        }

         if (italicText)
             txt = "<i>" + txt + "</i>";
       
       
        text.text = txt;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        UpdateTextParams();
    }

	// Update is called once per frame
	protected override void Update () {

        base.Update();

        if(showBackground > 0)
        {
            showBackground--;
            if(showBackground == 0)
            {
                if (preferredBackground != null)
                {
                    previousBackground = Desktop.main.GetBackgroundSource();
                    Desktop.main.SetBackground(preferredBackground.GetComponent<RectTransform>(), 0.7f);
                }
            }
        }
	}

    void WindowIsClosing(WindowA sender, int retValue)
    {
        if(previousBackground != null)
        {
            float fadeDuration = 0.7f;
            CloseEffect ce = GetComponent<CloseEffect>();
            if (ce != null)
                fadeDuration = ce.duration;

            Desktop.main.SetBackground(previousBackground.GetComponent<RectTransform>(), fadeDuration);
        }

        if (fadeToBlack != null)
        {
            fadeToBlack.startColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            fadeToBlack.endColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
            fadeToBlack.delay = 0.0f;
            fadeToBlack.duration = GetComponent<CloseEffect>().duration;
            fadeToBlack.gameObject.SetActive(true);
            fadeToBlack.Start();
        }
    }

}
