using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeTask : AiTask
{
    [SerializeField] private AnimationStates workAnimation;
    [SerializeField] private GameObject logPrefab;
    [SerializeField] private float dropRadius=3;
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
        var dropPoint = Random.insideUnitCircle * dropRadius;
        var randomPositionAround = new Vector3(dropPoint.x, transform.position.y, dropPoint.y);
        Instantiate(logPrefab, transform.position + randomPositionAround, Quaternion.Euler(0f, Random.Range(0,360), 0f));
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
