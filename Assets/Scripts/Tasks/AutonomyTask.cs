using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class AutonomyTask
{
    public string name;
    public AiTask task;
    internal float queueTime;
    internal Transform taskTransform;
    internal int priority;
    internal float workDistance;
    internal bool IsTaken { get; set; }
}