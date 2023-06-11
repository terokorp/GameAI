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
    private AutonomyTask? _task;
    private Coroutine _tasksCoroutine;

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

            if (_task.HasValue)
            {
                _agent.SetDestination(_task.Value.taskTransform.position);
                yield return new WaitUntil(() => (_agent.hasPath == true && _agent.pathPending == false) && CheckIsTaskDone(_task.Value));

                if (CheckisTaskValid(_task.Value))
                    yield return _task.Value.task.DoTask(_character);
            }
            else
                yield return new WaitForSeconds(1f); // No task, doing nothing
        }
    }

    private bool CheckisTaskValid(AutonomyTask? task)
    {
        if (!task.HasValue)
            return false;
        if (!AiTaskManager.Instance.tasks.Contains(task.Value))
            return false;
        if (task.Value.taskTransform == null || task.Value.task == null)
            return false;
        return true;
    }

    private bool CheckIsTaskDone(AutonomyTask? task)
    {
        if (!CheckisTaskValid(task.Value))
            return true;
        return Vector3.Distance(_agent.transform.position, task.Value.taskTransform.position) <= task.Value.workDistance;
    }

    // Solves which task to do
    private IEnumerator SolveTask()
    {
        //Debug.Log("Solving task");
        _task = null;

        foreach (var t in AiTaskManager.Instance.tasks.OrderBy(o => o.priority).ThenBy(o => o.queueTime))
        {
            if (t.task.IsValid(_character) && IsPathValid(t, ref _path))
            {
                _task = t;
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
