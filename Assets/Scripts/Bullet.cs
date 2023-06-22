using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Velocity of the bullet
    public float travelSpeed;

    private void Start()
    {
        // Give initial velocity on spawn
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.right * travelSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.TryGetComponent<Player>(out var otherPlayer))
        {
            otherPlayer.OnShot();
        }
        else if (other.transform.TryGetComponent<AgentController>(out var otherAgent))
        {
            otherAgent.OnShot();
        }
        Destroy(gameObject);
    }
}
