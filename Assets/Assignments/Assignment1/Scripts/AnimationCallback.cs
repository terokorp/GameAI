using UnityEngine;

public class AnimationCallback : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CollectItems collectItems = animator.GetComponentInParent<CollectItems>();
        if (collectItems)
            collectItems.AnimationDone = true;
    }
}
