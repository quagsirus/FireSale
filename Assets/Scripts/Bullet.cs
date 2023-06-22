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
        Debug.Log(other.gameObject);
        if (other.transform.parent != null)
        {
            if (other.transform.parent.TryGetComponent<Player>(out var otherPlayer))
                otherPlayer.OnShot();
            else if (other.transform.parent.TryGetComponent<AgentController>(out var otherAgent)) otherAgent.OnShot();
        }

        Destroy(gameObject);
    }
}