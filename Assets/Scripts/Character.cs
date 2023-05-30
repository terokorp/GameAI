using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour, IStateCallback
{
    private NavMeshAgent _agent;
    private Animator _animator;
    private static int animStateID = Animator.StringToHash("AnimationState");
    private AnimationStates state;

    bool animationIsPlaying = false;

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
        state = AnimationStates.IDLE;
        if (_agent.velocity.magnitude > 0.1f)
        {
            state = AnimationStates.WALK;
        }
        _animator.SetInteger(animStateID, (int)state);
    }

    internal IEnumerator AnimationWait(AnimationStates workAnimation)
    {
        _animator.SetInteger(animStateID, (int)workAnimation);
        yield return new WaitUntil(() => animationIsPlaying == false);
    }

    internal void AnimationSet(AnimationStates state)
    {
        _animator.SetInteger(animStateID, (int)state);
    }

    void IStateCallback.OnStateEnter() 
    {
        // Nothing to do
    }
    void IStateCallback.OnStateExit()
    {
        animationIsPlaying = false;
    }

    internal void SetPosition(Vector3 position, Quaternion rotation)
    {
        _agent.nextPosition = position;
        transform.position = position;
        transform.rotation = rotation;
    }
}
