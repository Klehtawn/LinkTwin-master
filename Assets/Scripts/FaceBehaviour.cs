using UnityEngine;
using System.Collections;

public class FaceBehaviour : MonoBehaviour {

    Animator animator;
	void Start () {

        animator = GetComponentInChildren<Animator>();
        animator.SetTrigger("Idle");

        prevParentPosition = getParentPosition();

        ResetIdle();
	}

    Vector3 prevParentPosition = Vector3.zero;
    Vector3 getParentPosition()
    {
        return transform.position;
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

        if(idleTimer > 1.85f)
        {
            int which = Random.Range(0, 3);
            if (which == 0)
                Blink();
            if (which == 1)
                Look(Vector3.left);
            if (which == 2)
                Look(Vector3.right);

            ResetIdle();
        }
	}

    bool eyesDefaultState = false;
    void EyesDefault()
    {
        if (eyesDefaultState) return;

        eyesDefaultState = true;

        animator.SetTrigger("Idle");
    }

    void Blink()
    {
        eyesDefaultState = false;
        animator.SetTrigger("Blink");
    }

    void Look(Vector3 dir, float delay = 0.0f, float speed = 1.0f)
    {
        eyesDefaultState = false;
        StartCoroutine(_Look(dir, delay, speed));
    }

    IEnumerator _Look(Vector3 dir, float delay, float speed)
    {
        yield return new WaitForSeconds(delay);

        if(dir.x < 0.0f)
            animator.SetTrigger("LookLeft");
        if (dir.x > 0.0f)
            animator.SetTrigger("LookRight");

        //yield return new WaitForSeconds(1.85f);

        //EyesDefault();
    }
}
