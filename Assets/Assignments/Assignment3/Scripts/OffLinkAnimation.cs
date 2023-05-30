using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OffLinkAnimation : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    private int jumpAnimationID = Animator.StringToHash("Jump");

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.autoTraverseOffMeshLink = false;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool(jumpAnimationID, agent.isOnOffMeshLink);

        if (!agent.isOnOffMeshLink)
            return;

        OffMeshLinkData data = agent.currentOffMeshLinkData;

        // Calculate the final point of the link
        Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;

        // Move the agent to the end point
        agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime * .75f);

        // When the agent reach the end point you should tell it, and the agent will "exit" the link and work normally after that
        if (agent.transform.position == endPos)
        {
            agent.CompleteOffMeshLink();
        }
    }
}
