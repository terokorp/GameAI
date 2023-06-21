using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private const float msToKmh = 3.60f;
    private const float msToMph = 2.23693629f;
    internal enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }
    internal enum SpeedType
    {
        MPH,
        KPH
    }

    [System.Serializable]
    internal struct Wheel
    {
        [SerializeField] internal WheelCollider collider;
        [SerializeField] internal GameObject mesh;
        [SerializeField] internal WheelEffects effect;
    }

    [Header("Car settings")]
    [SerializeField] private CarDriveType carDriveType = CarDriveType.FourWheelDrive;
    [SerializeField] private Wheel[] wheels = new Wheel[4];

    [SerializeField] private Vector3 centreOfMassOffset;
    [SerializeField] private float maximumSteerAngle = 30f;
    [SerializeField] private float steerCoefficient = 3f;
    [Tooltip("0 is raw physics , 1 the car will grip in the direction it is facing")]
    [Range(0, 1)] [SerializeField] public float steerHelper = .7f;
    [Tooltip("0 is no traction control, 1 is full interference")]
    [Range(0, 1)] [SerializeField] public float tractionControl = 1f;
    [SerializeField] public float fullTorqueOverAllWheels = 500;
    [SerializeField] private float reverseTorque = 150;
    [SerializeField] private float downforce = 100f;
    [SerializeField] private SpeedType speedType = SpeedType.KPH;
    [SerializeField] public float topspeed = 200;
    [SerializeField] private int noOfGears = 5;
    [SerializeField] private float revRangeBoundary = 1f;
    [SerializeField] public float slipLimit = .6f;
    [SerializeField] private float brakeTorque = 20000f;
    [SerializeField] internal bool reverse = false;

    private float maxHandbrakeTorque = float.MaxValue;
    private float steerAngle;
    private int gearNum;
    private float gearFactor;
    private float oldRotation;
    private float currentTorque;
    private new Rigidbody rigidbody;

    public bool Skidding { get; private set; }
    public float BrakeInput { get; private set; }
    public float CurrentSteerAngle { get { return steerAngle; } }
    public float CurrentSpeed { get { return rigidbody.velocity.magnitude * msToKmh; } }
    public float CurrentXSpeed { get { return transform.InverseTransformDirection(rigidbody.velocity * msToKmh).z; } }
    public float MaxSpeed { get { return topspeed; } set { topspeed = value; } }
    public float Revs { get; private set; }
    public float AccelInput { get; private set; }

    #region UnityCallbacks
    private void Awake()
    {

    }

    private void Start()
    {
        wheels[0].collider.attachedRigidbody.centerOfMass = centreOfMassOffset;
        maxHandbrakeTorque = float.MaxValue;
        rigidbody = GetComponent<Rigidbody>();
        currentTorque = fullTorqueOverAllWheels - (tractionControl * fullTorqueOverAllWheels);
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    private void Reset()
    {
        WheelCollider[] foundWheels = transform.GetComponentsInChildren<WheelCollider>();
        for (int i = 0; i < foundWheels.Length; i++)
        {
            if (!foundWheels[i])
                return;
            wheels[i].collider = foundWheels[i];
            //wheels[i].mesh = foundWheels[i].gameObject;
            wheels[i].effect = foundWheels[i].GetComponent<WheelEffects>();
        }
    }

    private void OnValidate()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i].collider != null && wheels[i].mesh != null && wheels[i].collider.transform == wheels[i].mesh.transform)
            {
                Debug.LogError("Wheel collider is same as it's mesh.");
                wheels[i].mesh = null;
            }
        }
    }
    #endregion

    public void Move(float steering, float accel, float footbrake, float handbrake)
    {
        if (!enabled)
            return;
        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 position;
            wheels[i].collider.GetWorldPose(out position, out quat);
            if (wheels[i].mesh == null)
                continue;
            wheels[i].mesh.transform.SetPositionAndRotation(position, quat);
        }

        //clamp input values
        steering = Mathf.Clamp(steering, -1, 1);
        AccelInput = accel = Mathf.Clamp(accel, 0, 1);
        BrakeInput = Mathf.Clamp(footbrake, 0, 1);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        //Set the steer on the front wheels.
        //Assuming that wheels 0 and 1 are the front wheels.
        steerAngle = (steerCoefficient * (1 / Mathf.Max(1, CurrentSpeed))) * steering * maximumSteerAngle;
        wheels[0].collider.steerAngle = steerAngle;
        wheels[1].collider.steerAngle = steerAngle;

        SteerHelper();
        ApplyDrive(accel, BrakeInput);
        CapSpeed();

        //Set the handbrake.
        //Assuming that wheels 2 and 3 are the rear wheels.
        if (handbrake > 0f)
        {
            var hbTorque = handbrake * maxHandbrakeTorque;
            wheels[2].collider.brakeTorque = hbTorque;
            wheels[3].collider.brakeTorque = hbTorque;
        }

        CalculateRevs();
        GearChanging();

        AddDownForce();
        CheckForWheelSpin();
        TractionControl();
    }

    private void SteerHelper()
    {
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelhit;
            wheels[i].collider.GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return; // wheels arent on the ground so dont realign the rigidbody velocity
        }

        // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(oldRotation - transform.eulerAngles.y) < 10f)
        {
            var turnadjust = (transform.eulerAngles.y - oldRotation) * steerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            rigidbody.velocity = velRotation * rigidbody.velocity;
        }
        oldRotation = transform.eulerAngles.y;
    }

    private void ApplyDrive(float accel, float brakeInput)
    {
        float reverseTorque;
        float thrustTorque;
        switch (carDriveType)
        {
            case CarDriveType.FourWheelDrive:
                thrustTorque = accel * (currentTorque / 4f);
                reverseTorque = accel * (currentTorque / 8f);
                for (int i = 0; i < 4; i++)
                {
                    wheels[i].collider.motorTorque = reverse ? -reverseTorque : thrustTorque;
                }
                break;

            case CarDriveType.FrontWheelDrive:
                thrustTorque = accel * (currentTorque / 2f);
                reverseTorque = accel * (currentTorque / 4f);
                wheels[0].collider.motorTorque = wheels[1].collider.motorTorque = reverse ? -reverseTorque : thrustTorque;
                break;

            case CarDriveType.RearWheelDrive:
                thrustTorque = accel * (currentTorque / 2f);
                reverseTorque = accel * (currentTorque / 4f);
                wheels[2].collider.motorTorque = wheels[3].collider.motorTorque = reverse ? -reverseTorque : thrustTorque;
                break;
        }

        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(transform.position, transform.position + rigidbody.velocity, Color.red);
            Debug.DrawLine(transform.position, transform.position + transform.forward, Color.white);

            wheels[i].collider.brakeTorque = brakeTorque * brakeInput;
            //if (CurrentSpeed < 5 && Vector3.Angle(transform.forward,rigidbody.velocity) < 50f)
            //{
            //    wheels[i].collider.brakeTorque = brakeTorque * brakeInput;
            //}
            //// Regular brake
            //else
            //if (thrustTorque < 0)
            //{
            //    wheels[i].collider.brakeTorque = 0f;
            //    wheels[i].collider.motorTorque = reverseTorque;
            //}
        }
    }

    private void CapSpeed()
    {
        float speed = rigidbody.velocity.magnitude * msToKmh;
        if (speed > topspeed)
            rigidbody.velocity = (topspeed / msToKmh) * rigidbody.velocity.normalized;
    }

    private void AddDownForce()
    {
        wheels[0].collider.attachedRigidbody.AddForce(-transform.up * downforce * wheels[0].collider.attachedRigidbody.velocity.magnitude);
    }

    private void CheckForWheelSpin()
    {
        // loop through all wheels
        for (int i = 0; i < 4; i++)
        {

            if (wheels[i].effect == null)
                continue;

            WheelHit wheelHit;
            wheels[i].collider.GetGroundHit(out wheelHit);

            //DebugGraph.MultiLog("WheelSlip Forward", DebugGraph.GetUniqueColor(i), wheelHit.forwardSlip, "Wheel " + i);
            //DebugGraph.MultiLog("WheelSlip Sideways", DebugGraph.GetUniqueColor(i), wheelHit.sidewaysSlip, "Wheel " + i);

            // is the tire slipping above the given threshhold
            if (Mathf.Abs(wheelHit.forwardSlip) >= slipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= slipLimit)
            {
                wheels[i].effect.EmitTyreSmoke();

                // avoiding all four tires screeching at the same time
                // if they do it can lead to some strange audio artefacts
                if (!wheels.Any(o => o.effect.PlayingAudio))
                {
                    wheels[i].effect.PlayAudio();
                }
                continue;
            }

            // if it wasnt slipping stop all the audio
            if (wheels[i].effect.PlayingAudio)
            {
                wheels[i].effect.StopAudio();
            }
            // end the trail generation
            wheels[i].effect.EndSkidTrail();
        }
    }


    private void TractionControl()
    {
        WheelHit wheelHit;
        switch (carDriveType)
        {
            case CarDriveType.FourWheelDrive:
                // loop through all wheels
                for (int i = 0; i < 4; i++)
                {
                    wheels[i].collider.GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                }
                break;

            case CarDriveType.FrontWheelDrive:
                wheels[0].collider.GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);

                wheels[1].collider.GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
                break;

            case CarDriveType.RearWheelDrive:
                wheels[2].collider.GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);

                wheels[3].collider.GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
                break;

        }
        // DebugGraph.Log("Torque", currentTorque);
    }

    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= slipLimit && currentTorque >= 0)
        {
            currentTorque -= 10 * tractionControl;
        }
        else
        {
            currentTorque += 10 * tractionControl;
            if (currentTorque > fullTorqueOverAllWheels)
            {
                currentTorque = fullTorqueOverAllWheels;
            }
        }
    }

    private void GearChanging()
    {
        float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
        float upgearlimit = (1 / (float)noOfGears) * (gearNum + 1);
        float downgearlimit = (1 / (float)noOfGears) * gearNum;

        if (gearNum > 0 && f < downgearlimit)
            gearNum--;

        if (f > upgearlimit && (gearNum < (noOfGears - 1)))
            gearNum++;
    }

    // simple function to add a curved bias towards 1 for a value in the 0-1 range
    private static float CurveFactor(float factor)
    {
        // https://www.desmos.com/calculator/knjgjk5vij
        return 1 - (1 - factor) * (1 - factor);
    }


    // unclamped version of Lerp, to allow value to exceed the from-to range
    private static float ULerp(float from, float to, float value)
    {
        return Mathf.LerpUnclamped(from, to, value);
        //return (1.0f - value) * from + value * to;
    }

    private void CalculateGearFactor()
    {
        float f = (1 / (float)noOfGears);
        // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
        // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.

        // @TODO: Check will this work because changed to KMH
        var targetGearFactor = Mathf.InverseLerp(f * gearNum, f * (gearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
        gearFactor = Mathf.Lerp(gearFactor, targetGearFactor, Time.deltaTime * 5f);
    }

    private void CalculateRevs()
    {
        // calculate engine revs (for display / sound)
        // (this is done in retrospect - revs are not used in force/power calculations)
        CalculateGearFactor();
        var gearNumFactor = gearNum / (float)noOfGears;
        var revsRangeMin = ULerp(0f, revRangeBoundary, CurveFactor(gearNumFactor));
        var revsRangeMax = ULerp(revRangeBoundary, 1f, gearNumFactor);
        Revs = ULerp(revsRangeMin, revsRangeMax, gearFactor);
    }


}
