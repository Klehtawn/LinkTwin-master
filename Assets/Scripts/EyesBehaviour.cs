using UnityEngine;
using System.Collections;

public class EyesBehaviour : MonoBehaviour {

    public Transform eyesDefault;
    public Transform eyesBlink;

    Vector3 eyesDefaultPosition = Vector3.zero;
	// Use this for initialization
	void Start () {

        if (eyesDefault != null)
            eyesDefault.gameObject.SetActive(true);

        if (eyesBlink != null)
            eyesBlink.gameObject.SetActive(false);

        prevParentPosition = getParentPosition();
        eyesDefaultPosition = transform.localPosition;

        EyesDefault();
	}

    Vector3 prevParentPosition = Vector3.zero;
    Vector3 getParentPosition()
    {
        return transform.parent.position;
    }

    void ResetIdle()
    {
        idleTimer = Random.Range(-2.0f, 0.0f);
    }
	
	// Update is called once per frame
    float idleTimer = 0.0f;
    //float moveTimer = 0.0f;
	void Update ()
    {
        Vector3 d = getParentPosition() - prevParentPosition;
        if(d.sqrMagnitude > 2.0f)
        {
            // cancel idle
            EyesDefault();

            prevParentPosition = getParentPosition();

            ResetIdle();

            //Look(d.normalized, 0.1f, 2.0f);
        }
        else
        {
            idleTimer += Time.deltaTime;
        }

        if(idleTimer > 3.5f)
        {
            int which = Random.Range(0, 4);
            if (which == 0)
                Blink();
            if (which == 1)
                Look(Vector3.left);
            if (which == 2)
                Look(Vector3.right);
            if (which == 3)
                Look(Vector3.forward * 0.5f);
            if (which == 4)
                Look(Vector3.back * 0.5f);

            ResetIdle();
        }
	}

    bool eyesDefaultState = false;
    void EyesDefault()
    {
        if (eyesDefaultState) return;

        eyesDefaultState = true;

        if (eyesDefault != null)
            eyesDefault.gameObject.SetActive(true);

        if (eyesBlink != null)
            eyesBlink.gameObject.SetActive(false);

        LeanTween.moveLocal(gameObject, eyesDefaultPosition, 0.85f).setEase(LeanTweenType.easeOutBack);
    }

    void Blink()
    {
        eyesDefaultState = false;
        StartCoroutine(_Blink());
    }

    void Look(Vector3 dir, float delay = 0.0f, float speed = 1.0f)
    {
        eyesDefaultState = false;
        StartCoroutine(_Look(dir, delay, speed));
    }

    IEnumerator _Look(Vector3 dir, float delay, float speed)
    {
        yield return new WaitForSeconds(delay);
        Vector3 pos = eyesDefaultPosition + dir * 1.05f;
        LeanTween.moveLocal(gameObject, pos, 0.85f / speed).setEase(LeanTweenType.easeInBack);

        float wait = Random.Range(0.5f, 1.0f) * 2.0f;

        yield return new WaitForSeconds(wait + 0.85f);

        EyesDefault();
    }

    IEnumerator _Blink()
    {
        if (eyesDefault != null)
            eyesDefault.gameObject.SetActive(false);

        if (eyesBlink != null)
            eyesBlink.gameObject.SetActive(true);

        yield return new WaitForSeconds(Random.Range(1.0f, 2.5f));

        EyesDefault();

        yield return null;
    }
}
