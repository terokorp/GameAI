using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogTask : AiTask
{
    internal protected override IEnumerator DoTask(Character character)
    {
        if (character is FarmCharacter c)
        {
            c.hasLog = true;
        }
        yield return new WaitForEndOfFrame();
        Destroy(transform);
    }

    internal protected override bool IsValid(Character character)
    {
        if (character is FarmCharacter c)
        {
            if (c.type == FarmCharacter.Type.WOODCUTTER)
                return true;
        }
        return false;
    }
}
