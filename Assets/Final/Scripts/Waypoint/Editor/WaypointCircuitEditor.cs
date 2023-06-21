using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static WaypointCircuit;

[CustomEditor(typeof(WaypointCircuit))]
public class WaypointCircuitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Update track"))
        {
            WaypointCircuit circuit = (WaypointCircuit)target;
            circuit.waypointList = new WaypointList();

            List<Transform> points = new List<Transform>();
            foreach (Transform c in circuit.transform)
                points.Add(c);

            circuit.waypointList.items = points.ToArray();
        }
    }
}
