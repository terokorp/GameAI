using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Autonomy : MonoBehaviour
{
    private bool _initialized;
    private NavMeshAgent _agent;
    private Character _character;
    private NavMeshPath _path = null;
    private AutonomyTask _task;
    private Coroutine _tasksCoroutine;
    [SerializeField] private float taskDistanceModifier=5f;

    #region Unity Callbacks
    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Finding references
        _agent = GetComponent<NavMeshAgent>();
        _character = GetComponent<Character>();
        _path = new NavMeshPath();
        _initialized = true;
    }

    // This function is called when the object becomes enabled and active
    private void OnEnable()
    {
        if (!_initialized)
            return;

        if (AgentManager.Instance)
            AgentManager.Instance.RegisterAgent(this);

        _tasksCoroutine = StartCoroutine(DoTasks());
    }

    // This function is called when the behaviour becomes disabled or inactive
    private void OnDisable()
    {
        if (AgentManager.Instance)
            AgentManager.Instance.UnregisterAgent(this);

        StopCoroutine(_tasksCoroutine);
    }
    #endregion

    private IEnumerator DoTasks()
    {
        yield return new WaitUntil(() => AiTaskManager.Instance != null);

        while (true)
        {
            yield return SolveTask();

            if (_task != null)
            {
                _agent.SetDestination(_task.taskTransform.position);
                yield return new WaitUntil(() => (_agent.hasPath == true && _agent.pathPending == false) && CheckIsTaskDone(_task));

                if (CheckisTaskValid(_task))
                    yield return _task.task.DoTask(_character);
            }
            else
                yield return new WaitForSeconds(1f); // No task, doing nothing
        }
    }

    private bool CheckisTaskValid(AutonomyTask? task)
    {
        if (task == null)
            return false;
        if (!AiTaskManager.Instance.tasks.Contains(task))
            return false;
        if (task.taskTransform == null || task.task == null)
            return false;
        return true;
    }

    private bool CheckIsTaskDone(AutonomyTask? task)
    {
        if (!CheckisTaskValid(task))
            return true;
        return Vector3.Distance(_agent.transform.position, task.taskTransform.position) <= task.workDistance;
    }

    // Solves which task to do
    private IEnumerator SolveTask()
    {
        //Debug.Log("Solving task");
        _task = null;

        foreach (var t in AiTaskManager.Instance.tasks.OrderByDescending(o => o.priority - ((transform.position - o.taskTransform.position).magnitude * taskDistanceModifier)).ThenBy(o => o.queueTime))
        {
            if (t.task.IsValid(_character) && IsPathValid(t, ref _path) && !t.IsTaken)
            {
                Debug.DrawLine(transform.position, t.taskTransform.position, Color.red, 1f);
                _task = t;
                t.IsTaken = true;
                break;
            }

            // Solving one path per frame
            yield return new WaitForEndOfFrame();
        }
    }

    private bool IsPathValid(AutonomyTask t, ref NavMeshPath path)
    {
        _agent.CalculatePath(t.taskTransform.position, path);
        if (path.status == NavMeshPathStatus.PathComplete)
            return true;
        return false;
    }

    internal float GetDistanceToTarget()
    {
        return _agent.remainingDistance;
    }

    private void OnDrawGizmos()
    {
        DrawGizmo();
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmo();
    }

    private void DrawGizmo()
    {
        if (_path == null || _path.corners.Length == 0)
            return;

        Vector3 previous = _path.corners[0];

        Gizmos.color = _path.status == NavMeshPathStatus.PathComplete ? Color.green : Color.red;

        foreach (Vector3 corner in _path.corners)
        {
            Gizmos.DrawLine(previous, corner);
            previous = corner;
        }
    }
}
