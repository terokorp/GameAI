using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour, IStateCallback
{
    private NavMeshAgent _agent;
    private Animator _animator;
    private static int animStateID = Animator.StringToHash("AnimationState");

    bool waitinForAnimation = false;
    private AnimationStates state;

    private void Awake()
    {
        // Finding references
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _agent.autoTraverseOffMeshLink = false;
    }

    // Update is called once per frame
    void Update()
    {
        AnimationStates newState;
        if(state == AnimationStates.IDLE || state == AnimationStates.WALK)
        {
            newState = _agent.velocity.magnitude > 0.1f ? AnimationStates.WALK : AnimationStates.IDLE;
            AnimationSet(newState);
        }
    }

    internal IEnumerator AnimationWait(AnimationStates workAnimation)
    {
        AnimationSet(workAnimation);
        waitinForAnimation = true;
        yield return new WaitUntil(() => waitinForAnimation == false);
    }

    internal void AnimationSet(AnimationStates newState)
    {
        if (state == newState)
            return;
        Debug.Log("State changed " + newState);
        waitinForAnimation = false;
        state = newState;
        _animator.SetInteger(animStateID, (int)newState);
    }

    void IStateCallback.OnStateEnter() 
    {
        // Nothing to do
    }
    void IStateCallback.OnStateExit()
    {
        waitinForAnimation = false;
    }

    internal void SetPosition(Vector3 position, Quaternion rotation)
    {
        _agent.nextPosition = position;
        transform.position = position;
        transform.rotation = rotation;
    }

    public void Die()
    {
        StartCoroutine(DieAnimation());
    }

    private IEnumerator DieAnimation()
    {
        AnimationSet(AnimationStates.CROUCH);
        yield return new WaitUntil(() => waitinForAnimation == false);
        Destroy(gameObject);
    }
}
