using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PushObjectTask : AiTask
{
    [SerializeField] private AnimationStates workAnimation;
    [SerializeField] float pushDistance = 3f;
    [SerializeField] AnimationCurve cuve;
    [SerializeField] private float pushSpeed = .2f;
    [SerializeField] UnityEvent OnTaskDone;

    internal protected override IEnumerator DoTask(Character character)
    {
        Debug.Log("DoTask PushObjectTask");
        character.transform.rotation = transform.rotation;

        Vector3 startPosition = character.transform.position;
        Vector3 endPosition = character.transform.position + character.transform.forward * pushDistance;
        Debug.DrawLine(startPosition, endPosition, new Color(0f,0f,1f,.8f), 1f);

        // Start animation
        character.AnimationSet(workAnimation);

        // Move character

        character.transform.position = startPosition;


        float startTime = Time.time;
        for (float i = 0; i < pushDistance; i += Time.deltaTime * pushSpeed)
        {
            cuve.AddKey(new Keyframe(Time.time - startTime, i / pushDistance));
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, Mathf.Clamp01(i / pushDistance));
            character.SetPosition(currentPosition, transform.rotation);
            Debug.DrawLine(startPosition, currentPosition, Color.blue, 0.1f);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();

        character.SetPosition(endPosition, transform.rotation);
        
        // Returning to idle
        character.AnimationSet(AnimationStates.IDLE);
        AiTaskManager.RemoveTask(task);
        yield return new WaitForSeconds(.5f);
        OnTaskDone?.Invoke();
    }

    internal protected override bool IsValid(Character character)
    {
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Debug.DrawLine(transform.position, transform.position + transform.forward * pushDistance, Color.blue);
    }
}
