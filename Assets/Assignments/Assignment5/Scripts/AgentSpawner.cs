using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 5f;

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
        // Random time beofre firest spawn
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, spawnInterval));

        while (true)
        {
            Spawn();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void Spawn()
    {
        if (AgentManager.Instance.autonomies.Count > 50)
            return;

        List<Autonomy> agentPrefabs = AgentManager.Instance.agentPrefabs;
        Instantiate(agentPrefabs[UnityEngine.Random.Range(0, agentPrefabs.Count)], transform.position, transform.rotation, transform);
    }
}
