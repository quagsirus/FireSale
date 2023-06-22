using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Velocity of the bullet
    public float travelSpeed;

    private void Start()
    {
        // Get rigidbody component
        var rb = gameObject.GetComponent<Rigidbody2D>();
        // Convert Z angle to radians
        var rotationRadians = rb.rotation * Mathf.Deg2Rad;
        // Give initial velocity on spawn
        rb.velocity = new Vector2(Mathf.Cos(rotationRadians), Mathf.Sin(rotationRadians)) * travelSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.TryGetComponent<Player>(out var otherPlayer))
            // Kill player if detected
            otherPlayer.OnShot();
        // Only proceed if we the collision item has a parent
        if (other.transform.parent != null)
            if (other.transform.parent.TryGetComponent<AgentController>(out var otherAgent))
                // Kill agent if detected
                otherAgent.OnShot();

        // Remove bullet
        Destroy(gameObject);
    }
}