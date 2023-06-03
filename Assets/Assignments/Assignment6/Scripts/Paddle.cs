using UnityEngine;
using UnityEngine.Events;

public class Paddle : MonoBehaviour
{
    [SerializeField] UnityEvent onCollision;
    private float hitTimer = 0f;

    private void Update()
    {
        hitTimer += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponentInParent<PongBall>();
        if (ball != null)
        {
            if (hitTimer < 0.2f)
                return;
            ball.PaddleCollision();
            hitTimer = 0;
        }
    }
}
