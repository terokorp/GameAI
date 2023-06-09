using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAi : MonoBehaviour
{
    public float maxSpeed = 200f;
    [SerializeField] private AnimationCurve cautiousDistance;
    [SerializeField] [Range(0.1f, 1f)] private float maxCautiousCurve = .5f;
    [SerializeField] [Range(0, 90)] private float cautiousAngleMin = 5;
    [SerializeField] [Range(0, 90)] private float cautiousAngleMax = 45;
    [SerializeField] private float maxSteerinAngle;
    [SerializeField] private float targetTriggerDistance = 5f;

    private CarController cc;
    private Waypoints waypoints;
    private readonly Queue<Vector3> aiTargets = new Queue<Vector3>();
    private Vector3? currentTarget;
    private Vector3? nextTarget;
    private float _steerAngle;


    [Header("Debug")]
    [SerializeField] private float speedDerivate;
    [SerializeField] private float desiredSpeed;
    [SerializeField] private float cautiousRate;
    [SerializeField] private float _currentSpeed;
    [SerializeField] private float distanceToTarget;
    [SerializeField] private float cautiousRateMultiplier;
    [SerializeField] private float cautiousRateAngle;

    private void Awake()
    {
        cc = GetComponent<CarController>();
        waypoints = FindAnyObjectByType<Waypoints>();

        foreach (Transform point in waypoints.GetTrack())
            aiTargets.Enqueue(point.position);
        foreach (Transform point in waypoints.GetTrack())
            aiTargets.Enqueue(point.position);
        foreach (Transform point in waypoints.GetTrack())
            aiTargets.Enqueue(point.position);
    }

    private void Update()
    {
        if (!currentTarget.HasValue)
        {
            currentTarget = aiTargets.Dequeue();
            try
            {
                nextTarget = aiTargets.Peek();
            }
            catch (Exception)
            {
                nextTarget = null;
            }

        }
        cc.isDriving = currentTarget.HasValue;
        if (!currentTarget.HasValue)
            return;
        distanceToTarget = Vector3.Distance(transform.position, currentTarget.Value);

        if (distanceToTarget < targetTriggerDistance)
        {
            currentTarget = null;
            return;
        }

        // Slowing down is next turn is hard
        if (currentTarget.HasValue && nextTarget.HasValue)
        {
            Vector3 first = currentTarget.Value - transform.position;
            Vector3 second = nextTarget.Value - currentTarget.Value;
            first.y = 0f;
            second.y = 0f;
            cautiousRateAngle = Mathf.Abs(Vector3.SignedAngle(first, second, Vector3.up));
            Debug.DrawLine(currentTarget.Value, currentTarget.Value + (currentTarget.Value - transform.position), Color.green);
            Debug.DrawLine(currentTarget.Value, currentTarget.Value + (nextTarget.Value - currentTarget.Value), Color.blue);
        }
        else
            cautiousRateAngle = 0f;
        cautiousRate = cautiousDistance.Evaluate(distanceToTarget / cc.Speed);
        cautiousRateMultiplier = Map(Mathf.Abs(cautiousRateAngle), cautiousAngleMin, cautiousAngleMax, 0f, cautiousRate);
        cautiousRateMultiplier = Mathf.Clamp(cautiousRateMultiplier, 0f, 0.9f);


        // Calculating final speed
        desiredSpeed = maxSpeed * (1f - cautiousRateMultiplier);
        speedDerivate = (desiredSpeed - cc.Speed) / 3f;
        if (speedDerivate < 0 && speedDerivate > -3f)
            speedDerivate = 0;
        cc.throttleValue = Mathf.Clamp(speedDerivate, -1f, 1f);
        _currentSpeed = cc.Speed;

        // Streering
        _steerAngle = Vector3.SignedAngle(transform.forward, currentTarget.Value - transform.position, Vector3.up);
        _steerAngle = Mathf.Clamp(_steerAngle, -maxSteerinAngle, maxSteerinAngle);
        cc.steerValue = _steerAngle / cc.maxSteerAngle;

    }

    private float Map(object p, float cautiousAngleMin, float cautiousAngleMax, float cautiousRate, float v)
    {
        throw new NotImplementedException();
    }

    private static float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

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
        Gizmos.color = Color.blue;
        if (currentTarget.HasValue)
            Gizmos.DrawLine(transform.position, currentTarget.Value);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.rotation * Quaternion.Euler(0f, _steerAngle, 0f) * Vector3.forward);
    }
}
