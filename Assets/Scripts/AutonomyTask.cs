using UnityEngine;

[System.Serializable]
public struct AutonomyTask
{
    public string name;
    public IAiTask taskObject;
    internal float queueTime;
    internal Transform taskPosition;
    internal int taskPriority;
    internal float workDistance;
}