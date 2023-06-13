using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrotTask : AiTask
{
    [SerializeField] float growTime = 5f;
    [SerializeField] Vector3 smallSizeScale = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] Vector3 fullSizeScale = new Vector3(0.2f, 0.2f, 0.2f);

    private bool _ready;
    private float growTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        if (_ready)
            return;

        if (growTimer < growTime)
        {
            growTimer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(smallSizeScale, fullSizeScale, growTimer / growTime);
        }
        else
        {
            _ready = true;
            AiTaskManager.AddTask(new AutonomyTask()
            {
                name = transform.name,
                taskTransform = transform,
                priority = _priority,
                workDistance = 0.5f
            });
        }
    }
    internal protected override IEnumerator DoTask(Character character)
    {
        if (character is FarmCharacter c)
        {
            c.hasCarrot = true;
        }
        yield return new WaitForEndOfFrame();
        Destroy(transform);
    }

    internal protected override bool IsValid(Character character)
    {
        if (character is FarmCharacter c)
        {
            if (c.type == FarmCharacter.Type.FARMER)
                return true;
        }
        return false;
    }
}
