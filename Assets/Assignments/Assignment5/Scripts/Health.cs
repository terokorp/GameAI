using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float health = 100f;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage">Damage amount</param>
    /// <returns>returns true if object has died</returns>
    public bool TakeDamage(float damage)
    {
        health -= damage;
        return damage < 100f;
    }
}
