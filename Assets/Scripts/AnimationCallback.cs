using UnityEngine;

public class AnimationCallback : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        IStateCallback callback = animator.GetComponentInParent<IStateCallback>();
        callback?.OnStateEnter();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        IStateCallback callback = animator.GetComponentInParent<IStateCallback>();
        callback?.OnStateExit();
    }
}
