using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Range(-1f, 1f)] public float steerValue;
    [Range(-1f, 1f)] public float throttleValue;
    public bool isDriving = false;

    [SerializeField] private List<WheelCollider> wheels;
    [SerializeField] private List<Transform> wheelVisual;
    [SerializeField] private List<Renderer> brakeLights;
    [SerializeField] private Material brakeMatOn;
    [SerializeField] private Material brakeMatOff;
    [SerializeField] internal float maxSteerAngle = 25f;
    [SerializeField] private float motorTorque = 2000f;
    [SerializeField] private float motorBreakTorque = 2000f;
    [SerializeField] private float handBreakTorque = float.MaxValue;

    private float _speed;
    private Rigidbody _rb;
    private bool _brake;
    [SerializeField] [Range(0f, 1f)] private float tractionControlRate;

    public float Speed { get => _speed; }


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateVisuals();
    }
    private void FixedUpdate()
    {
        _speed = transform.InverseTransformVector(_rb.velocity).z * 3.6f;
        if (isDriving)
            Drive(steerValue, throttleValue);
        else
            Drive(0f, -1f);
    }

    private void UpdateVisuals()
    {
        // Visual to match collider
        for (int i = 0; i < 4; i++)
        {
            wheels[i].GetWorldPose(out Vector3 pos, out Quaternion quat);
            wheelVisual[i].SetPositionAndRotation(pos, quat);
            brakeLights.All(o => o.material = _brake ? brakeMatOn : brakeMatOff);
        }
    }
    private void Drive(float steerValue, float throttleValue)
    {
        SetSteer(steerValue);
        CalculateTractionControl();
        SetThrottle(throttleValue);
    }

    private void CalculateTractionControl()
    {
        for (int i = 0; i < 4; i++)
        {
            if (wheels[i].GetGroundHit(out WheelHit hit))
            {
                if (hit.forwardSlip > .15f)
                    tractionControlRate -= Time.deltaTime / 4f;
                else
                    tractionControlRate += Time.deltaTime / 4f;
            }
        }
        tractionControlRate = Mathf.Clamp01(tractionControlRate);
    }

    private void SetSteer(float steerAngle)
    {
        steerAngle = Mathf.Clamp(steerAngle, -1f, 1f);
        for (int i = 0; i < 2; i++)
            wheels[i].steerAngle = steerAngle * maxSteerAngle;
    }
    private void SetThrottle(float throttleValue)
    {
        throttleValue = Mathf.Clamp(throttleValue, -1f, 1f);
        if (throttleValue >= 0)
        {
            for (int i = 0; i < 4; i++)
            {
                wheels[i].motorTorque = throttleValue * motorTorque * tractionControlRate;
                wheels[i].brakeTorque = 0f;
                _brake = false;
            }
        }
        else
        {
            // If speed is low enough, use real brake, otherwise use motors
            if (_speed > 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    wheels[i].motorTorque = throttleValue * motorBreakTorque * tractionControlRate;
                    wheels[i].brakeTorque = 0f;
                    _brake = true;
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    wheels[i].motorTorque = 0f;
                    wheels[i].brakeTorque = handBreakTorque;
                    _brake = false;
                }
            }

        }
    }
    private void Reset()
    {
        wheels = GetComponentsInChildren<WheelCollider>().ToList();
    }
}
