using System;
using UnityEngine;

public class PongBall : MonoBehaviour
{
    [SerializeField] Rigidbody ball;
    [SerializeField] Bounds areaBounds;
    [SerializeField] Vector2 ballOffset = Vector2.zero;

    [SerializeField] [Range(0.1f, 10f)] float ballStartSpeed = 1f;
    [SerializeField] [Range(0.1f, 10f)] float speedIncrease = .5f;
    [SerializeField] PongScore score;
    public int xDirection = -1;
    public int yDirection = -1;
    private float ballRadius;
    private float ballSpeed;

    // Start is called before the first frame update
    void Start()
    {
        ballRadius = ball.GetComponentInParent<SphereCollider>().radius * Mathf.Max(ball.transform.lossyScale.x, ball.transform.lossyScale.y, ball.transform.lossyScale.z);
        ballSpeed = ballStartSpeed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 limits = CalculateLimits();

        if (limits.x < 0f) { xDirection = 1; score.AddAcore(PongPlayer.PlayerSide.Left); }
        if (limits.x > 0f) { xDirection = -1; score.AddAcore(PongPlayer.PlayerSide.Right); }
        if (limits.y < 0f) yDirection = 1;
        if (limits.y > 0f) yDirection = -1;

        ballOffset += new Vector2(xDirection, yDirection) * ballSpeed * Time.deltaTime;
        ball.MovePosition(transform.position + new Vector3(ballOffset.x, ballOffset.y, areaBounds.center.z) + new Vector3(limits.x, limits.y, 0f));
    }

    private Vector2 CalculateLimits()
    {
        Vector2 maxLimits = new Vector2(
            ballOffset.x - areaBounds.max.x + ballRadius,
            ballOffset.y - areaBounds.max.y + ballRadius
            );
        if (maxLimits.x < 0) maxLimits.x = 0;
        if (maxLimits.y < 0) maxLimits.y = 0;

        Vector2 minLimits = new Vector2(
            ballOffset.x - areaBounds.min.x - ballRadius,
            ballOffset.y - areaBounds.min.y - ballRadius
            );
        if (minLimits.x > 0) minLimits.x = 0;
        if (minLimits.y > 0) minLimits.y = 0;

        return maxLimits + minLimits;
    }

    internal void PaddleCollision()
    {
        xDirection *= -1;
        ballSpeed += speedIncrease;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + areaBounds.center, areaBounds.size);
    }
}
