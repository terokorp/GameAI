using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float health = 100f;
    public UnityEvent OnDying;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage">Damage amount</param>
    /// <returns>returns true if object has died</returns>
    public bool TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0f)
            OnDying?.Invoke();
        return damage <= 0f;
    }
}
