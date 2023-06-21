using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDestination : MonoBehaviour
{
    [SerializeField] private float radius = 10f;

    public static AiDestination Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public static Vector3 GetNewDestination()
    {
        Vector3 randomPosition = Random.insideUnitCircle * Instance.radius;
        return Instance.transform.position + new Vector3(randomPosition.x, 0f, randomPosition.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 start;
        Vector3 end;
        for (int i = 0; i < 365; i++)
        {
            start = new Vector3(Mathf.Sin(i * Mathf.Deg2Rad), 0f, Mathf.Cos(i * Mathf.Deg2Rad));
            end = new Vector3(Mathf.Sin((i + 1) * Mathf.Deg2Rad), 0f, Mathf.Cos((i + 1) * Mathf.Deg2Rad));
            Gizmos.DrawLine(transform.position + (start * radius), transform.position + (end * radius));
        }
    }
}
