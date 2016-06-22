using UnityEngine;
using System.Collections;

public class GhostsSimulation : MonoBehaviour
{
    public float moveDuration = 0.7f;
    public bool loop = true;

    private int currentMove = 0;

    public GameObject boyGhost, girlGhost;

    private GameObject[] ghosts;

    public float ghostsTransparency = 0.4f;

    void Start()
    {
        ghosts = new GameObject[2];
        ghosts[0] = GameObject.Instantiate<GameObject>(boyGhost);
        ghosts[1] = GameObject.Instantiate<GameObject>(girlGhost);

        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].transform.SetParent(transform);
            ghosts[i].gameObject.SetActive(false);
            Widget.SetLayer(ghosts[i], gameObject.layer);
            FadeMaterial fm = ghosts[i].GetComponent<FadeMaterial>();
            if (fm != null)
                fm.FadeTo(new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, 0.0f));
        }

        isPlaying = false;
        isPaused = false;
    }

    float moveTimer = 0.0f;
    void Update()
    {
        if(isPlaying && !isPaused)
        {
            moveTimer += Time.deltaTime;
            if(moveTimer >= moveDuration)
            {
                NextMove();
            }
        }
    }

    void NextMove()
    {
        currentMove++;
        moveTimer = 0.0f;


        if(currentMove >= TheGame.Instance.solution.Length)
        {
            for (int i = 0; i < ghosts.Length; i++)
            {
                FadeMaterial fm = ghosts[i].GetComponent<FadeMaterial>();
                if (fm != null)
                    fm.FadeTo(new Color(1.0f, 1.0f, 1.0f, ghostsTransparency), new Color(1.0f, 1.0f, 1.0f, 0.0f));
            }

            if (loop)
            {
                currentMove = -2;
            }
            else
            {
                StopSimulation();
                return;
            }
        }

        PerformCurrentMove();
    }

    void PerformCurrentMove()
    {

        if(currentMove == -1)
        {
            MoveGhostsToInitialPositions();
        }

        if (currentMove < 0) return;

        Vector3 dir = TheInput.indexedDirections[TheGame.Instance.solution[currentMove]];
        Vector3 delta = dir * GameSession.gridUnit;
       
        foreach(GameObject g in ghosts)
        {
            Vector3 newPos = g.transform.position + delta;
            Block b = TheGame.Instance.blocks.ItemAt(newPos);
            if (b != null && b.CanBeWalkedOn(-dir))
                LeanTween.move(g, newPos, moveDuration * 0.8f).setEase(LeanTweenType.linear);
        }
    }

    void MoveGhostsToInitialPositions()
    {
        Spawner[] spawners = TheGame.Instance.blocks.GetBlocksOfType<Spawner>();

        for (int i = 0; i < ghosts.Length; i++)
        {
            LeanTween.cancel(ghosts[i]);

            Vector3 pos = spawners[i].position;
            pos.y = TheGame.Instance.players[i].position.y;

            ghosts[i].transform.position = pos;
            FadeMaterial fm = ghosts[i].GetComponent<FadeMaterial>();
            if (fm != null)
                fm.FadeTo(new Color(1.0f, 1.0f, 1.0f, 0.0f), new Color(1.0f, 1.0f, 1.0f, ghostsTransparency));
        }
    }

    bool isPlaying = false;
    public void PlaySimulation()
    {
        if (TheGame.Instance.solution == null) return;

        isPlaying = true;
        moveTimer = 0.0f;
        currentMove = -2;

        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].gameObject.SetActive(true);
        }

        MoveGhostsToInitialPositions();

        PerformCurrentMove();
    }

    public void StopSimulation()
    {
        isPlaying = false;

        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].gameObject.SetActive(false);
        }
    }

    public bool IsPlaying
    {
        get
        {
            return isPlaying;
        }
    }

    bool isPaused = false;
    public void PauseSimulation()
    {
        isPaused = true;
    }

    public void ResumeSimulation()
    {
        isPaused = false;
        moveTimer = moveDuration;
    }

    public static GhostsSimulation Create(Transform t)
    {
        GameObject obj = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("GhostsSimulation"));
        obj.transform.SetParent(t);
        Widget.SetLayer(obj, LayerMask.NameToLayer("Game"));
        return obj.GetComponent<GhostsSimulation>();
    }
}
