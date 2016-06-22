using UnityEngine;
using System.Collections;
using System;

public class TabbedWidget : Widget {

    [Serializable]
    public class TabInfo
    {
        public Sprite icon;
        public string caption;
        public GameObject contents;

        public void AddContents(GameObject newContents)
        {
            Transform p = contents.transform.parent;
            Widget.DeleteAllChildren(p);
            newContents.transform.SetParent(p);
            contents = newContents;
        }
    };

    public TabInfo[] tabsInformation;
    public float maxTabButtonWidth = 150.0f;

    private GameObject tabButtonTemplate;
    private RectTransform tabButtons;

    private int currentActiveTab = 0;

    bool tabsCreated = false;

    [NonSerialized]
    public RectTransform clientArea;

	// Use this for initialization
	protected override void Start () {
        base.Start();

        tabButtonTemplate = transform.Find("TabButtonTemplate").gameObject;
        tabButtonTemplate.gameObject.SetActive(false);

        tabButtons = transform.Find("TabButtons").GetComponent<RectTransform>();

        clientArea = transform.Find("ClientArea").GetComponent<RectTransform>();
        clientArea.offsetMax = new Vector2(0.0f, -tabButtons.rect.height);
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();

        if(tabsCreated == false && tabsInformation.Length > 0)
        {
            CreateTabs();
        }
	}

    public void CreateTabs()
    {
        tabsCreated = true;

        Widget.DeleteAllChildren(tabButtons);

        float tabButtonWidth = Mathf.Min(maxTabButtonWidth, width / (float)tabsInformation.Length);

        for(int i = 0; i < tabsInformation.Length; i++)
        {

            // create buttons

            GameObject b = GameObject.Instantiate<GameObject>(tabButtonTemplate);
            b.SetActive(true);
            b.transform.SetParent(tabButtons);
            b.transform.localScale = Vector3.one;

            Transform buttonBorder = b.transform.Find("RightBorder");
            if (buttonBorder != null)
                buttonBorder.gameObject.SetActive(i < tabsInformation.Length - 1);

            if(tabsInformation[i].caption != null)
            {
                b.name = tabsInformation[i].caption + " tab button";
            }

            RectTransform rt = b.GetComponent<RectTransform>();
            rt.anchorMin = new Vector3(0.0f, 0.0f);
            rt.anchorMax = new Vector3(0.0f, 1.0f);
            rt.pivot = Vector2.zero;
            float x = (float)i * tabButtonWidth;
            rt.anchoredPosition = new Vector2(x, 0.0f);
            rt.sizeDelta = new Vector2(tabButtonWidth, 0.0f);

            Widget w = b.GetComponent<Widget>();

            if(tabsInformation[i] != null)
            {
                for (int k = 0; k < 2; k++)
                {
                    w.active = k == 0;
                    Button[] allChildButtons = b.GetComponentsInChildren<Button>();
                    foreach (Button cb in allChildButtons)
                    {
                        cb.icon.sprite = tabsInformation[i].icon;
                        if (tabsInformation[i].caption != null && tabsInformation[i].caption.Length > 0)
                            cb.caption.GetComponent<LocalizedText>().text = tabsInformation[i].caption;
                        else
                        {
                            cb.caption.gameObject.SetActive(false);
                            cb.iconAlign = TextAlignment.Center;
                        }
                    }
                }
            }

            w.OnClick += OnTabButtonClick;

            // add contents

            GameObject contents = tabsInformation[i].contents;

            if (contents == null)
            {
                contents = new GameObject();
                contents.name = "DummyContents #" + i.ToString();
                contents.AddComponent<RectTransform>();

                tabsInformation[i].contents = contents;
            }

            contents.transform.SetParent(clientArea);
            rt = contents.GetComponent<RectTransform>();
            rt.pivot = Vector2.one * 0.5f;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.transform.localScale = Vector3.one;
        }

        currentActiveTab = -1;
        ActivateTab(0);
    }

    void OnTabButtonClick(MonoBehaviour mb, Vector2 pos)
    {
        Widget sender = mb.GetComponent<Widget>();

        for(int i = 0; i < tabButtons.childCount; i++)
        {
            Widget w = tabButtons.GetChild(i).GetComponent<Widget>();
            if(w == sender)
            {
                ActivateTab(i);
                break;
            }
        }
    }

    public void ActivateTab(int index)
    {
        if (index == currentActiveTab) return;

        if (currentActiveTab >= 0)
        {
            Widget oldt = tabButtons.GetChild(currentActiveTab).GetComponent<Widget>();
            oldt.active = false;
        }

        Widget t = tabButtons.GetChild(index).GetComponent<Widget>();
        t.active = true;

        for (int i = 0; i < tabsInformation.Length; i++)
        {
            Widget w = tabsInformation[i].contents.GetComponent<Widget>();
            if (w != null)
                w.active = i == index;
        }

        int oldIndex = currentActiveTab;
        currentActiveTab = index;

        if(OnTabSwitch != null)
        {
            OnTabSwitch(oldIndex, index);
        }

        tabsInformation[index].contents.transform.SetAsLastSibling();
    }

    public Action<int, int> OnTabSwitch;

    public void AddTab(string caption, Sprite icon, GameObject contents)
    {
        if (tabsInformation == null)
            tabsInformation = new TabbedWidget.TabInfo[1];
        else
            Array.Resize<TabbedWidget.TabInfo>(ref tabsInformation, tabsInformation.Length + 1);

        TabbedWidget.TabInfo tab = new TabbedWidget.TabInfo();

        tab.caption = caption;
        tab.icon = icon;
        tab.contents = contents;

        tabsInformation[tabsInformation.Length - 1] = tab;

        CreateTabs();
    }
}
