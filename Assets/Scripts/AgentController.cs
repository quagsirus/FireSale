using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    // Aggressive flag (true for player tracking, false for chicken running)
    public bool aggressive;

    // Wander behavior configuration
    public float wanderRepositionDelay;
    public float wanderMaxDistance;

    // NavMeshAgent component (located on Awake)
    private NavMeshAgent _navMeshAgent;

    // True when player has entered area collider
    private bool _playerSpotted;

    // Assigned to the player's transform in OnPlayerEnteredArea
    private Transform _playerTransform;

    // Timer for wander repositioning
    private float _wanderTimer;

    private void Awake()
    {
        // Cache NavMeshAgent component on spawn
        _navMeshAgent = GetComponent<NavMeshAgent>();

        // Prevent unity updating our position, because it's designed for 3d it moves us on the z axis
        _navMeshAgent.updatePosition = false;
    }

    private void Update()
    {
        // Remain dormant until player has been spotted
        if (!_playerSpotted) return;

        // If agent should be moving towards player to attack
        if (aggressive)
            // Update destination to current player position
        {
            _navMeshAgent.destination = _playerTransform.position;
        }
        // If agent should chicken run instead
        else if (_wanderTimer > wanderRepositionDelay)
        {
            // Cache current position as we'll be accessing it lots in this loop
            var position = transform.position;

            // Keep generating random locations until we get a valid hit
            NavMeshHit hit = default;
            while (!hit.hit)
            {
                // Generate a random Vector2 within wanderMaxDistance of the agent
                var randomLocation = Random.insideUnitCircle * wanderMaxDistance + new Vector2(position.x, position.y);
                // Check if this is a valid location on the NavMesh
                NavMesh.SamplePosition(new Vector3(randomLocation.x, randomLocation.y, 0), out hit, 0.5f, -1);
            }

            // Update target destination to new random hit
            _navMeshAgent.destination = hit.position;

            // Reset the timer
            _wanderTimer = 0;
        }
        else
            // Increment timer for wander repositioning
        {
            _wanderTimer += Time.deltaTime;
        }

        // Update current agent position to calculated next location, without z axis
        var nextPosition = _navMeshAgent.nextPosition;
        transform.position = new Vector3(nextPosition.x, nextPosition.y, 0);
    }

    // Subscribed to PlayerEnteredArea event in AreaManager
    public void OnPlayerEnteredArea(object sender, AreaManager.PlayerEnteredAreaArgs e)
    {
        _playerSpotted = true;
        _playerTransform = e.TargetPlayer.transform;
    }
}