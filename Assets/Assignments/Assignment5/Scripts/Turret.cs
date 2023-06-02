using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnpoint;
    [SerializeField] private float spawnInterval = 3f;

    private void OnEnable()
    {
        StartCoroutine(SpawnerCoroutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnerCoroutine()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, spawnInterval));

        while (true)
        {
            Instantiate(projectilePrefab, projectileSpawnpoint.position, projectileSpawnpoint.rotation);
            yield return new WaitForSeconds(spawnInterval);
        }

    }
}
