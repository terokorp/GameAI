using UnityEngine;

public class CarPlayerControl : CarControl
{
    public enum Direction { Forward, Backward }

    bool canSwitchReverse;
    Direction desiredDirection = Direction.Forward;

    private void FixedUpdate()
    {
        if (!enabled)
            return;
        float steering = 0;
        float accel = 0;
        float footbrake = 0;
        float handbrake = 1;

        if(canSwitchReverse)
        {
            if (accel > footbrake) desiredDirection = Direction.Forward;
            else if (accel < footbrake) desiredDirection = Direction.Backward;
        }
        canSwitchReverse = Mathf.Abs(carController.CurrentSpeed) < 0.1f && accel < 0.01f && footbrake < 0.01f;

        carController.Move(steering, accel, footbrake, handbrake);
    }
}
