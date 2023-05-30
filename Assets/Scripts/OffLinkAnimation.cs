using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class OffLinkAnimation : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Animator _animator;

    private int _jumpAnimationID = Animator.StringToHash("Jump");
    private bool _onOffMeshLink;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponentInParent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _animator.SetBool(_jumpAnimationID, _agent.isOnOffMeshLink);

        if (!_agent.isOnOffMeshLink)
            return;

        if (!_onOffMeshLink)
        {
            _onOffMeshLink = true;
            StartCoroutine(DoOffMeshLinkAnimation(_agent.currentOffMeshLinkData));
        }
    }

    private IEnumerator DoOffMeshLinkAnimation(OffMeshLinkData data)
    {
        // Calculate the final point of the link
        bool backwards = Vector3.Distance(_agent.transform.position, data.endPos) < Vector3.Distance(_agent.transform.position, data.startPos);
        Vector3 startPos = (backwards ? data.endPos : data.startPos) + Vector3.up * _agent.baseOffset;
        Vector3 endPos = (backwards ? data.startPos : data.endPos) + Vector3.up * _agent.baseOffset;
        Vector3 desiredDirection;
        Vector3 directionOnPlane;
        do
        {
            _agent.updateRotation = false;
            // Rotating towards end point
            desiredDirection = Vector3.RotateTowards(_agent.transform.forward, endPos - _agent.transform.position, Time.deltaTime * 5f, 0.0f);
            _agent.transform.rotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
            Debug.DrawLine(_agent.transform.position, _agent.transform.position + (endPos - transform.position), Color.blue);
            yield return new WaitForEndOfFrame();
            directionOnPlane = endPos - _agent.transform.position;
            directionOnPlane.y = 0f;
            Debug.DrawLine(_agent.transform.position, _agent.transform.position + _agent.transform.forward, Color.black);
            Debug.DrawLine(_agent.transform.position, _agent.transform.position + directionOnPlane, Color.blue, 1f);
            _agent.updateRotation = true;
        }
        while (Vector3.Angle(_agent.transform.forward, directionOnPlane) > 2f);

        while (Vector3.Distance(_agent.transform.position, endPos) > .1f)
        {
            // Move the agent to the end point
            _agent.transform.position = Vector3.MoveTowards(_agent.transform.position, endPos, _agent.speed * Time.deltaTime * .75f);
            _agent.transform.rotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
            yield return new WaitForEndOfFrame();
        }

        // Done
        _agent.CompleteOffMeshLink();
        _onOffMeshLink = false;
    }

}
