using UnityEngine;
using UnityEngine.Events;

public class AISensor : MonoBehaviour
{
    public UnityEvent<Collider> TriggerEnter;
    public UnityEvent<Collider> TriggerExit;
    public UnityEvent<Collider> TriggerStay;
    [SerializeField] internal LayerMask layerMask;

    private void OnTriggerEnter(Collider other)
    {
        if ((layerMask & (1 << other.gameObject.layer)) != 0)
            TriggerEnter?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if ((layerMask & (1 << other.gameObject.layer)) != 0)
            TriggerExit?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if ((layerMask & (1 << other.gameObject.layer)) != 0)
            TriggerStay?.Invoke(other);
    }

}
