using UnityEngine;

[RequireComponent(typeof(CarController),typeof(WaypointProgressTracker), typeof(Rigidbody))]
public abstract class CarControl : MonoBehaviour
{
    internal CarController carController;
    internal WaypointProgressTracker waypointTracker;
    internal new Rigidbody rigidbody;
    private Vector3 respawnPosition;
    private Quaternion respawnRotation;

    internal virtual void Awake()
    {
        carController = GetComponent<CarController>();
        waypointTracker = GetComponent<WaypointProgressTracker>();
        rigidbody = GetComponent<Rigidbody>();
        SetRespawnPoint(transform);
    }

    private void SetRespawnPoint(Transform point)
    {
        respawnPosition = point.position;
        respawnRotation = point.rotation;
    }

    internal virtual void Respawn()
    {
        rigidbody.isKinematic = true;

        transform.position = respawnPosition + Vector3.up * 0.5f;
        transform.rotation = respawnRotation;

        // yield return new WaitForEndOfFrame();

        // Reset values
        transform.localScale = Vector3.one;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.isKinematic = false;
    }
}
