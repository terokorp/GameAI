using System;
using System.Collections;
using UnityEngine;

public abstract class AiTask : MonoBehaviour
{
    [SerializeField] private protected AutonomyTask task;
    [SerializeField] private protected int _priority;
    [SerializeField] private protected float workDistance;

    internal virtual void OnEnable()
    {
        StartCoroutine(LateOnEnable());
    }

    private IEnumerator LateOnEnable()
    {
        yield return new WaitUntil(() => AiTaskManager.Instance != null);
        task = new AutonomyTask
        {
            name = gameObject.name,
            task = this,
            priority = _priority,
            taskTransform = transform,
            workDistance = workDistance,
            queueTime = Time.time,
        };
        AiTaskManager.AddTask(task);
    }

    internal virtual void OnDisable()
    {
        AiTaskManager.RemoveTask(task);
    }

    internal protected virtual IEnumerator DoTask(Character character)
    {
        throw new NotImplementedException();
    }
    internal protected virtual bool IsValid(Character character)
    {
        throw new NotImplementedException();
    }
}