using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogTask : AiTask
{
    internal protected override IEnumerator DoTask(Character character)
    {
        throw new System.NotImplementedException();
    }

    internal protected override bool IsValid(Character character)
    {
        if (character is FarmCharacter c)
        {
            return true;
        }
        return false;
    }
}
