using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeTarget : MonoBehaviour, IAiTask
{
    [SerializeField] bool aiCollectable = true;
    [SerializeField] int _priority;
    [SerializeField] float workDistance = .2f;
    [SerializeField] private AnimationStates workAnimation;
    private AutonomyTask task;
    private Health health;

    int IAiTask.Priority { get => _priority; set => _priority = value; }

    private void Awake()
    {
        health = GetComponent<Health>();
    }
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
            workDistance = workDistance,
            queueTime = Time.time
        };
        AiTaskManager.AddTask(task);
    }

    IEnumerator IAiTask.DoTask(Character character)
    {
        Debug.Log("DoTask");
        while (health.health >= 0f)
        {
            // Starting animation
            yield return character.AnimationWait(workAnimation);
            health.TakeDamage(1);

        }
        
        // Returning to idle
        character.AnimationSet(AnimationStates.IDLE);
        AiTaskManager.RemoveTask(task);
    }
}
