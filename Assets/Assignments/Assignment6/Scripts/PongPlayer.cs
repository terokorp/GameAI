using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongPlayer : MonoBehaviour
{
    public enum PlayerSide { Left, Right }

    [SerializeField] private PlayerSide side;
    [SerializeField] private Transform paddle;
    [SerializeField] private Transform ball;

    Vector2 ballOldPosition;
    Vector2 ballCurrentPosition;
    private bool aiming = false;
    private float xPosition;

    private void Start()
    {
        xPosition = paddle.position.x;
    }
    // Update is called once per frame
    void Update()
    {
        ballOldPosition = ballCurrentPosition;
        ballCurrentPosition = ball.position;

        float ballSpeed = (ballOldPosition - ballCurrentPosition).magnitude / Time.deltaTime;

        aiming = false;
        if ((ballCurrentPosition - ballOldPosition).x > 0 && side == PlayerSide.Right)
            aiming = true;

        if ((ballCurrentPosition - ballOldPosition).x < 0 && side == PlayerSide.Left)
            aiming = true;

        float yPosition = ball.position.y;
        if(aiming)
            paddle.position = new Vector3(xPosition, yPosition, 0f);

        //// Predict position
        //float timeOfFlight = (projectileSpawnpoint.position - aimTarget.position).magnitude / projectileSpeed;
        //Vector3 predictedPosition = aimTarget.position + (aimTarget.forward * targetSpeed) * timeOfFlight;
        //predictedPosition += Vector3.up * .5f; // Offset by character height
        //Debug.DrawLine(pivot.position, predictedPosition, aimming ? Color.green : Color.red, default, false);
    }
}
