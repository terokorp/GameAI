using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField] internal List<Autonomy> agentPrefabs;

    public static AgentManager Instance { get; private set; }
    [SerializeField] internal List<Autonomy> autonomies;

    private void Awake()
    {
        Instance = this;
    }

    internal void RegisterAgent(Autonomy autonomy)
    {
        autonomies.Add(autonomy);
    }

    internal void UnregisterAgent(Autonomy autonomy)
    {
        autonomies.Remove(autonomy);
    }
}
