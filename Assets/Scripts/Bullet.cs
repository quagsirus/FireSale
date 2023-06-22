using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Velocity of the bullet
    public float travelSpeed;

    private void Start()
    {
        var rb = gameObject.GetComponent<Rigidbody2D>();
        var fRotation = rb.rotation * Mathf.Deg2Rad;
        var fX = Mathf.Sin(fRotation);
        var fY = Mathf.Cos(fRotation);
        // Give initial velocity on spawn
        rb.velocity = new Vector2(fY, fX) * travelSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.parent.TryGetComponent<Player>(out var otherPlayer))
            otherPlayer.OnShot();
        else if (other.transform.parent.TryGetComponent<AgentController>(out var otherAgent)) otherAgent.OnShot();
        Debug.Log(other.gameObject);
        Destroy(gameObject);
    }
}