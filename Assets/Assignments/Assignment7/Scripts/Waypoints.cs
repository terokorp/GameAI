using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [SerializeField] List<Transform> waypoints;

    private void Reset()
    {
        RaycastHit hitinfo;
        // Drop object to ground
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hitinfo, 10f))
            transform.SetPositionAndRotation(hitinfo.point, Quaternion.identity);

        //Repopulate list
        waypoints.Clear();
        foreach(Transform t in transform)
        {
            if (Physics.Raycast(t.position + Vector3.up, Vector3.down, out hitinfo, 10f))
                t.SetPositionAndRotation(hitinfo.point, Quaternion.identity);
            waypoints.Add(t);
        }
    }

    internal IEnumerable<Transform> GetTrack() => waypoints;


    private void OnDrawGizmos()
    {
        DrawGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmos();
    }

    private void DrawGizmos()
    {
        if (waypoints.Count == 0)
            return;

        Vector3 _previous = waypoints[0].position;

        foreach (Transform t in waypoints)
        {
            Gizmos.DrawLine(_previous, t.position);
            _previous = t.position;
        }
    }
}
