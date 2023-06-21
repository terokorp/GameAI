using UnityEngine;

public class CarAiControl : CarControl
{
    public enum BrakeCondition
    {
        NeverBrake,                                                                          // the car simply accelerates at full throttle all the time.
        TargetDirectionDifference,                                                           // the car will brake according to the upcoming change in direction of the target. Useful for route-based AI, slowing for corners.
        TargetDistance,                                                                      // the car will brake as it approaches its target, regardless of the target's direction. Useful if you want the car to
    }

    internal enum AiState
    {
        Driving, Recover, Respawn
    }

    [Header("Driving settings")]
    [SerializeField] [Range(0, 1)] private float cautiousSpeedFactor = 0.05f;                // percentage of max speed to use when being maximally cautious
    [SerializeField] [Range(0, 180)] private float cautiousMaxAngle = 50f;                   // angle of approaching corner to treat as warranting maximum caution
    [SerializeField] private float cautiousMaxDistance = 100f;                               // distance at which distance-based cautiousness begins
    [SerializeField] private float cautiousAngularVelocityFactor = 30f;                      // how cautious the AI should be when considering its own current angular velocity (i.e. easing off acceleration if spinning!)
    [SerializeField] private float steerSensitivity = 0.05f;                                 // how sensitively the AI uses steering input to turn to the desired direction
    [SerializeField] private float accelSensitivity = 0.04f;                                 // How sensitively the AI uses the accelerator to reach the current desired speed
    [SerializeField] private float brakeSensitivity = 1f;                                    // How sensitively the AI uses the brake to reach the current desired speed
    [SerializeField] private float lateralWanderDistance = 3f;                               // how far the car will wander laterally towards its target
    [SerializeField] private float lateralWanderSpeed = 0.1f;                                // how fast the lateral wandering will fluctuate
    [SerializeField] [Range(0, 1)] private float accelWanderAmount = 0.1f;                   // how much the cars acceleration will wander
    [SerializeField] private float accelWanderSpeed = 0.1f;                                  // how fast the cars acceleration wandering will fluctuate
    [SerializeField] private BrakeCondition brakeCondition = BrakeCondition.TargetDistance;  // what should the AI consider when accelerating/braking?
    [SerializeField] public bool driving;                                                    // whether the AI is currently actively driving or stopped.
    [SerializeField] private Transform target;                                               // 'target' the target object to aim for.
    [SerializeField] private bool stopWhenTargetReached;                                     // should we stop driving when we reach the target?
    [SerializeField] private float reachTargetThreshold = 2;                                 // proximity to target to consider we 'reached' it, and stop driving.

    private float randomPerlin;                                                              // A random value for the car to base its wander on (so that AI cars don't all wander in the same pattern)
    private float avoidOtherCarTime;                                                         // time until which to avoid the car we recently collided with
    private float avoidOtherCarSlowdown;                                                     // how much to slow down due to colliding with another car, whilst avoiding
    private float avoidPathOffset;                                                           // direction (-1 or 1) in which to offset path to avoid other car, whilst avoiding

    [Header("Other car detection")]
    [SerializeField] float aiSensorRadius = 20f;
    [SerializeField] LayerMask aiSensorMask;

    [Header("Recovering system")]
    [SerializeField] private AiState state;
    [SerializeField] private float stuckTime = 2f; // Max stuck time until switcing to recover mode

    [SerializeField] private float recoverTime = 2f; // Max time on recover mode
    [SerializeField] private float recoverStuckTime = 2f; // Max stuck time until respawn on recover mode

    private AISensor aiSensor; // Detect other cars
    private float stuckTimer;  // Timer that counts how long car has been stuck
    private float recoverTimer;
    #region UnityCallbacks
    internal override void Awake()
    {
        base.Awake();
        if (aiSensor == null)
            CreateSensor();
        randomPerlin = Random.value * 100;
    }

    private void OnEnable()
    {
        SetTarget(waypointTracker.target);
    }

    private void FixedUpdate()
    {
        if (!enabled)
            return;

        if (target == null || !driving)
        {
            carController.Move(0f, 0f, 1f, 1f);
            return;
        }

        switch (state)
        {
            case AiState.Driving:
                DoDriving();
                break;
            case AiState.Recover:
                DoRecover();
                break;
            case AiState.Respawn:
                Respawn();
                state = AiState.Driving;
                break;
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        if (collision.rigidbody == null)
            return;

        var othercar = collision.rigidbody.GetComponent<CarController>();
        if (othercar == null)
            return;

        // we'll take evasive action for 1 second
        avoidOtherCarTime = Time.time + 1;

        // but who's in front?...
        if (Vector3.Angle(transform.forward, othercar.transform.position - transform.position) < 90)
        {
            // the other ai is in front, so it is only good manners that we ought to brake...
            avoidOtherCarSlowdown = 0.5f;
        }
        else
        {
            // we're in front! ain't slowing down for anybody...
            avoidOtherCarSlowdown = 1;
        }

        // both cars should take evasive action by driving along an offset from the path centre,
        // away from the other car
        var otherCarLocalDelta = transform.InverseTransformPoint(othercar.transform.position);
        float otherCarAngle = Mathf.Atan2(otherCarLocalDelta.x, otherCarLocalDelta.z);
        avoidPathOffset = lateralWanderDistance * -Mathf.Sign(otherCarAngle);
    }
    #endregion

    private void DoDriving()
    {
        float steering = 0;
        float accel = 0;
        float footbrake = 0f;
        float handbrake = 0f;

        Vector3 fwd = transform.forward;
        if (rigidbody.velocity.magnitude > carController.MaxSpeed * 0.1f)
        {
            fwd = rigidbody.velocity;

        }

        float desiredSpeed = carController.MaxSpeed;

        switch (brakeCondition)
        {
            case BrakeCondition.TargetDistance:
                desiredSpeed = CalculateDesiredSpeedFromTargetDistance(fwd);
                break;
            case BrakeCondition.TargetDirectionDifference:
                desiredSpeed = CalculateDesiredSpeedFromTargetDirectionDifference();
                break;
            case BrakeCondition.NeverBrake:
                break;
        }

        //DebugGraph.Log("Desired Speed", desiredSpeed);

        // Evasive action due to collision with other cars:

        // our target position starts off as the 'real' target position
        Vector3 offsetTargetPos = target.position;

        // if are we currently taking evasive action to prevent being stuck against another car:
        if (Time.time < avoidOtherCarTime)
        {
            // slow down if necessary (if we were behind the other car when collision occured)
            desiredSpeed *= avoidOtherCarSlowdown;

            // and veer towards the side of our path-to-target that is away from the other car
            offsetTargetPos += target.right * avoidPathOffset;
        }
        else
        {
            // no need for evasive action, we can just wander across the path-to-target in a random way,
            // which can help prevent AI from seeming too uniform and robotic in their driving
            offsetTargetPos += target.right * (Mathf.PerlinNoise(Time.time * lateralWanderSpeed, randomPerlin) * 2 - 1) * lateralWanderDistance;
        }

        // use different sensitivity depending on whether accelerating or braking:
        float accelBrakeSensitivity = (desiredSpeed < carController.CurrentSpeed) ? brakeSensitivity : accelSensitivity;

        // decide the actual amount of accel/brake input to achieve desired speed.
        accel = Mathf.Clamp((desiredSpeed - carController.CurrentSpeed) * accelBrakeSensitivity, -1, 1);

        // add acceleration 'wander', which also prevents AI from seeming too uniform and robotic in their driving
        // i.e. increasing the accel wander amount can introduce jostling and bumps between AI cars in a race
        accel *= (1 - accelWanderAmount) + (Mathf.PerlinNoise(Time.time * accelWanderSpeed, randomPerlin) * accelWanderAmount);

        // calculate the local-relative position of the target, to steer towards
        Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);

        // work out the local angle towards the target
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        // get the amount of steering needed to aim the car towards the target
        steering = Mathf.Clamp(targetAngle * steerSensitivity, -1, 1) * Mathf.Sign(carController.CurrentSpeed);

        // feed input to the car controller.
        carController.reverse = false;
        carController.Move(steering, accel, footbrake, handbrake);

        // if appropriate, stop driving when we're close enough to the target.
        if (stopWhenTargetReached && localTarget.magnitude < reachTargetThreshold)
        {
            driving = false;
        }

        // Checking if AI is stuck
        if (carController.CurrentSpeed < 1f && driving)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > stuckTime)
            {
                state = AiState.Recover;
                stuckTimer = 0f;
            }
        }
        else
            stuckTimer = 0f;

        //DebugGraph.Log("Stuck timer", stuckTimer);
    }

    private void DoRecover()
    {
        float steering = 0;
        float accel = 0;
        float footbrake = 0f;
        float handbrake = 0f;

        float desiredSpeed = 30f;
        float targetAngle = 0;

        // use different sensitivity depending on whether accelerating or braking:
        float accelBrakeSensitivity = (desiredSpeed < carController.CurrentSpeed) ? brakeSensitivity : accelSensitivity;

        // decide the actual amount of accel/brake input to achieve desired speed.
        accel = Mathf.Clamp((desiredSpeed - carController.CurrentSpeed) * accelBrakeSensitivity, -1, 1);

        // add acceleration 'wander', which also prevents AI from seeming too uniform and robotic in their driving
        // i.e. increasing the accel wander amount can introduce jostling and bumps between AI cars in a race
        accel *= (1 - accelWanderAmount) + (Mathf.PerlinNoise(Time.time * accelWanderSpeed, randomPerlin) * accelWanderAmount);

        // get the amount of steering needed to aim the car towards the target
        steering = Mathf.Clamp(targetAngle * steerSensitivity, -1, 1) * Mathf.Sign(carController.CurrentSpeed);
        carController.reverse = true;
        carController.Move(steering, accel, footbrake, handbrake);


        // Checking if AI is stuck
        if (carController.CurrentSpeed < 1f && driving)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer > recoverStuckTime)
            {
                state = AiState.Respawn;
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
            recoverTimer += Time.fixedDeltaTime;
            if(recoverTimer > recoverTime)
            {
                state = AiState.Driving;
                recoverTimer = 0f;
            }
        }
    }

    private float CalculateDesiredSpeedFromTargetDistance(Vector3 fwd)
    {
        // the car will brake according to the upcoming change in direction of the target. Useful for route-based AI, slowing for corners.

        // check out the angle of our target compared to the current direction of the car
        float approachingCornerAngle = Vector3.Angle(target.forward, fwd);

        // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle
        float spinningAngle = rigidbody.angularVelocity.magnitude * cautiousAngularVelocityFactor;

        // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount
        float cautiousnessRequired = Mathf.InverseLerp(0, cautiousMaxAngle, Mathf.Max(spinningAngle, approachingCornerAngle));

        return Mathf.Lerp(carController.MaxSpeed, carController.MaxSpeed * cautiousSpeedFactor, cautiousnessRequired);
    }
    private float CalculateDesiredSpeedFromTargetDirectionDifference()
    {
        // the car will brake as it approaches its target, regardless of the target's direction. Useful if you want the car to
        // head for a stationary target and come to rest when it arrives there.

        // check out the distance to target
        Vector3 delta = target.position - transform.position;
        float distanceCautiousFactor = Mathf.InverseLerp(cautiousMaxDistance, 0, delta.magnitude);

        // also consider the current amount we're turning, multiplied up and then compared in the same way as an upcoming corner angle
        float spinningAngle = rigidbody.angularVelocity.magnitude * cautiousAngularVelocityFactor;

        // if it's different to our current angle, we need to be cautious (i.e. slow down) a certain amount
        float cautiousnessRequired = Mathf.Max(Mathf.InverseLerp(0, cautiousMaxAngle, spinningAngle), distanceCautiousFactor);

        return Mathf.Lerp(carController.MaxSpeed, carController.MaxSpeed * cautiousSpeedFactor, cautiousnessRequired);
    }
    public void SetTarget(Transform target)
    {
        this.target = target;
        if (target != null)
            driving = true;
    }
    private void CreateSensor()
    {
        var g = new GameObject("Sensor Trigger");
        g.transform.SetParent(transform);
        g.transform.localPosition = Vector3.zero;
        g.transform.rotation = Quaternion.identity;
        g.gameObject.layer = gameObject.layer;
        SphereCollider sensorCollider = g.AddComponent<SphereCollider>();
        sensorCollider.isTrigger = true;
        sensorCollider.radius = aiSensorRadius;
        aiSensor = g.AddComponent<AISensor>();
        aiSensor.layerMask = aiSensorMask;
    }
}
