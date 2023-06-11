using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableTask : AiTask
{
    [SerializeField] bool aiCollectable = true;
    [SerializeField] private AnimationStates workAnimation;

    // Start is called before the first frame update
    void Start()
    {
        if (!aiCollectable)
            return;

        task = new AutonomyTask
        {
            name = gameObject.name,
            task = this,
            priority = _priority,
            taskTransform = transform,
            workDistance = 0.2f,
            queueTime = Time.time
        };
        AiTaskManager.AddTask(task);
    }

    internal protected override IEnumerator DoTask(Character character)
    {
        Debug.Log("DoTask");
        // Starting animation
        yield return character.AnimationWait(workAnimation);
        // Returning to idle
        character.AnimationSet(AnimationStates.IDLE);
        AiTaskManager.RemoveTask(task);
        Destroy(gameObject);
    }

    internal protected override bool IsValid(Character character)
    {
        return true;
    }
}
