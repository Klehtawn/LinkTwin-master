using UnityEngine;
using System.Collections;

public class BoxCreateEffect : MonoBehaviour {

    public GameObject finalStage;
    public GameObject creationStage;

    public float duration = 1.0f;

    private float effectTimer = 0.0f;

    public Renderer animateRenderer;
    public string animationUniformName = "_AnimationTime";
    public float animationExponent = 2.0f;

	// Use this for initialization
	void Start () {
        finalStage.gameObject.SetActive(false);
        creationStage.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {

        if(isStarted)
        {
            float factor = effectTimer / duration;
            DoAnimation(factor);

            effectTimer += Time.deltaTime;
            if(effectTimer > duration)
            {
                isStarted = false;

                if (isBackwards == false)
                    ShowFinalStage();
                else
                    ShowCreationStage();

                return;
            }
        }
	
	}


    bool isStarted = false;
    bool isBackwards = false;

    public void ShowEffect(float delay = 0.0f)
    {
        StartCoroutine(_ShowEffect(delay, false));
    }

    IEnumerator _ShowEffect(float delay, bool backwards)
    {
        yield return new WaitForSeconds(delay);
        isStarted = true;
        effectTimer = 0.0f;
        isBackwards = backwards;
        if (backwards)
            ShowFinalStage();
        else
        {
            ShowCreationStage();
        }
    }
    public void ShowEffectBackwards(float delay)
    {
        StartCoroutine(_ShowEffect(delay, true));
    }

    void ShowFinalStage()
    {
        finalStage.gameObject.SetActive(true);
        creationStage.gameObject.SetActive(false);
    }

    void ShowCreationStage()
    {
        finalStage.gameObject.SetActive(false);
        creationStage.gameObject.SetActive(true);

        Vector3 wind = new Vector3(Random.Range(0.7f, 1.0f) * 8.0f, 0.0f, Random.Range(-1.0f, 1.0f) * 2.5f);
        animateRenderer.material.SetVector("_Wind", wind);
    }

    int animationFactorUniform = -1;

    public LeanTweenType animationType = LeanTweenType.punch;
    void DoAnimation(float factor)
    {
        if(animationFactorUniform < 0)
        {
            animationFactorUniform = Shader.PropertyToID(animationUniformName);
        }

        //animateRenderer.material.SetFloat(animationFactorUniform, Mathf.Pow(1.0f - factor, animationExponent));
        animateRenderer.material.SetFloat(animationFactorUniform, 0.0f);

        if(factor == 0.0f)
        {
            Vector3 pos = creationStage.transform.position;
            Vector3 initialScale = creationStage.transform.localScale;
            Vector3 dir = TheGame.Instance.blocks.GetGroundCenter() - pos; dir.y = 0.0f;
            dir.Normalize();

            Vector3 startPos = pos - dir * UnityEngine.Random.Range(3.0f, 6.0f) * GameSession.gridUnit;

            LeanTween.move(creationStage, pos, duration).setFrom(startPos).setEase(animationType);
            LeanTween.scale(creationStage, initialScale, duration * 0.85f).setFrom(initialScale * 0.001f).setEase(animationType);
        }
    }
}
