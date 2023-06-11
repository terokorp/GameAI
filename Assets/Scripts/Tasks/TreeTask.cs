using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeTask : AiTask
{
    [SerializeField] bool aiCollectable = true;
    [SerializeField] private AnimationStates workAnimation;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    internal protected override IEnumerator DoTask(Character character)
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

    internal protected override bool IsValid(Character character)
    {
        if(character is FarmCharacter c)
        {
            if (c.type == FarmCharacter.Type.WOODCUTTER)
                return true;
        }
        return false;
    }
}
