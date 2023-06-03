using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Turret : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnpoint;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private Transform aimTarget;
    [SerializeField][Range(0f, 360f)] private float rotationSpeed;
    private float projectileSpeed;
    Vector3 currentPosition;
    Vector3 previousPosition;
    private bool aimming;

    private void OnEnable()
    {
        StartCoroutine(ShootCoroutine());
        projectileSpeed = projectilePrefab.GetComponent<Projectile>().speed;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        FindNewTarget();
        AimToTarget();
    }

    private void AimToTarget()
    {
        if (aimTarget == null)
            return;

        // Calculate target speed
        previousPosition = currentPosition;
        currentPosition = aimTarget.transform.position;
        float targetSpeed = (previousPosition - currentPosition).magnitude / Time.deltaTime;

        // Predict position
        float timeOfFlight = (projectileSpawnpoint.position - aimTarget.position).magnitude / projectileSpeed;
        Vector3 predictedPosition = aimTarget.position + (aimTarget.forward * targetSpeed) * timeOfFlight;
        predictedPosition += Vector3.up * .5f; // Offset by character height
        Debug.DrawLine(pivot.position, predictedPosition, aimming ? Color.green : Color.red, default, false);

        // Aim
        Quaternion targetRotation = Quaternion.LookRotation(predictedPosition - projectileSpawnpoint.position, Vector3.up);
        pivot.rotation = Quaternion.RotateTowards(pivot.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        Debug.DrawLine(pivot.position, pivot.position + targetRotation * Vector3.forward * 2f, Color.blue, default, false);

        Debug.DrawLine(projectileSpawnpoint.position, aimTarget.transform.position + Vector3.up * .5f, Color.yellow, default, false);

        aimming = Quaternion.Angle(pivot.rotation, targetRotation) < 1f;
    }

    private void FindNewTarget()
    {
        if (aimTarget != null)
            return;
        aimming = false;
        var target = AgentManager.Instance.autonomies.OrderBy(o => o.GetDistanceToTarget()).FirstOrDefault();
        if(target != null)
        {
            aimTarget = target.transform;
            currentPosition = aimTarget.transform.position;
            previousPosition = aimTarget.transform.position;
        }
    }

    private IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, spawnInterval));

        while (true)
        {
            yield return new WaitUntil(() => aimming);
            Instantiate(projectilePrefab, projectileSpawnpoint.position, projectileSpawnpoint.rotation);
            yield return new WaitForSeconds(spawnInterval);
        }

    }
}
