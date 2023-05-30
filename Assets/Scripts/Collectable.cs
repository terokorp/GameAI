using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour, IAiTask
{
    [SerializeField] bool aiCollectable = true;
    [SerializeField] int _priority;
    [SerializeField] private AnimationStates workAnimation;
    private AutonomyTask task;

    int IAiTask.Priority { get => _priority; set => _priority = value; }

    // Start is called before the first frame update
    void Start()
    {
        if (!aiCollectable)
            return;

        task = new AutonomyTask
        {
            name = gameObject.name,
            taskObject = this,
            taskPriority = _priority,
            taskPosition = transform,
            workDistance = 0.2f,
            queueTime = Time.time
        };
        AiTaskManager.AddTask(task);
    }

    IEnumerator IAiTask.DoTask(Character character)
    {
        Debug.Log("DoTask");
        // Starting animation
        yield return character.AnimationWait(workAnimation);
        // Returning to idle
        character.AnimationSet(AnimationStates.IDLE);
        AiTaskManager.RemoveTask(task);
        Destroy(gameObject);
    }
}
