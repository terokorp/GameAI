using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class CollectItems : MonoBehaviour
{
    [SerializeField] List<GameObject> items;
    [SerializeField] private float startDelay;
    private static int animStateID = Animator.StringToHash("AnimationState");
    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField][Range(0f, 2f)] private float pickupDistance = .5f;
    public bool AnimationDone { private get; set; }


    // Start is called before the first frame update
    IEnumerator Start()
    {
        // Finding references
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // Waiting for start delay
        yield return new WaitForSeconds(startDelay);

        // Collecting all items
        foreach (var i in items)
        {
            // Setting destination to navmesh agent
            agent.isStopped = false;
            agent.stoppingDistance = pickupDistance;
            agent.SetDestination(i.transform.position);

            // Starting walk animation
            animator.SetInteger(animStateID, (int)AnimationStates.WALK);

            // Wait until agent stops
            yield return new WaitUntil(() => agent.hasPath == true && agent.pathPending == false && agent.remainingDistance < pickupDistance);
            agent.isStopped = true;

            // Pickup item animation
            animator.SetInteger(animStateID, (int)AnimationStates.CROUCH);

            // Wait until animation is done
            AnimationDone = false;
            yield return new WaitUntil(() => AnimationDone);

            // Destoy collectable
            Destroy(i);

            // Set animation back to idle
            animator.SetInteger(animStateID, (int)AnimationStates.IDLE);
        }
    }
}
