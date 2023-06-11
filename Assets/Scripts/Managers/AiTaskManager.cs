using System.Collections.Generic;
using UnityEngine;

public class AiTaskManager : MonoBehaviour
{
    public List<AutonomyTask> tasks;
    public static AiTaskManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    internal static void AddTask(AutonomyTask autonomyTask)
    {
        if(ValidateTask(autonomyTask))
        {
            Instance.tasks.Add(autonomyTask);
        }
    }
    internal static void RemoveTask(AutonomyTask autonomyTask)
    {
        Instance.tasks.Remove(autonomyTask);
    }

    private static bool ValidateTask(AutonomyTask autonomyTask)
    {
        return autonomyTask.task != null;
    }
}
