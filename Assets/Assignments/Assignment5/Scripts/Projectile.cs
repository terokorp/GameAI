using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 2f;
    public float damage = 1f;

    private void Start()
    {
        // Destorying current object after 10 seconds
        Destroy(gameObject, 10f);
    }
    private void Update()
    {
        transform.position += transform.forward * Time.deltaTime * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        var healt = other.GetComponent<Health>();
        if (healt != null)
            healt.TakeDamage(damage);
        Destroy(gameObject);
    }
}
