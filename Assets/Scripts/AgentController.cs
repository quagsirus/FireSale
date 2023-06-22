using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AgentController : MonoBehaviour
{
    // Aggressive flag (true for player tracking, false for chicken running)
    public bool aggressive;

    // Movement speeds
    public float wanderSpeed;
    public float activeSpeed;

    // Wander behavior configuration
    public float wanderRepositionDelay;
    public float wanderMaxDistance;
    public float chickenRepositionDelay;
    public float chickenMaxDistance;

    // AnimationStateController (instantiated on Awake)
    private AnimationStateController _animationStateController;

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
        // Make sure the navigation speed matches wanderSpeed by default
        _navMeshAgent.speed = wanderSpeed;

        // Instantiate AnimationStateController
        _animationStateController = new AnimationStateController(gameObject.GetComponent<Animator>());
    }

    private void Start()
    {
        // Destroy self if spawn is in invalid location
        if (_navMeshAgent.isOnNavMesh) return;
        throw new Exception("Invalid spawn location");
        Destroy(gameObject);
    }

    private void Update()
    {
        // Stop animator locking up
        _animationStateController.FixDirectionLockup();

        // Cache current position as we'll be accessing it at least once and multiple times in some cases
        var position = transform.position;

        // If agent should be moving towards player to attack (don't do this until we have a target)
        if (aggressive && _playerSpotted)
        {
            // Update destination to current player position
            _navMeshAgent.destination = _playerTransform.position;
        }
        // If agent should be randomly moving
        else if (_wanderTimer > (_playerSpotted ? chickenRepositionDelay : wanderRepositionDelay))
        {
            // Keep generating random locations until we get a valid hit
            NavMeshHit hit = default;
            while (!hit.hit)
            {
                // Generate a random Vector2 within MaxDistance of the agent
                var randomLocation =
                    Random.insideUnitCircle * (_playerSpotted ? chickenMaxDistance : wanderMaxDistance) +
                    new Vector2(position.x, position.y);
                // Check if this is a valid location on the NavMesh
                NavMesh.SamplePosition(new Vector3(randomLocation.x, randomLocation.y, 0), out hit, 0.5f, -1);
            }

            // Update target destination to new random hit
            _navMeshAgent.destination = hit.position;

            // Reset the timer
            _wanderTimer = 0;
        }
        else
        {
            // Increment timer for wander repositioning
            _wanderTimer += Time.deltaTime;
        }

        // Check if we are moving this frame
        var isMoving = _navMeshAgent.stoppingDistance < _navMeshAgent.remainingDistance;
        if (isMoving)
        {
            // Update animation movement direction
            _animationStateController.SetMovementDirection(_navMeshAgent.nextPosition - position);

            // Update current agent position to calculated next location, without z axis
            var nextPosition = _navMeshAgent.nextPosition;
            transform.position = new Vector3(nextPosition.x, nextPosition.y, 0);
        }

        // Update running/idle state based on if stopping distance has been reached
        _animationStateController.SetRunningState(isMoving, !_playerSpotted);
    }

    // Subscribed to PlayerEnteredArea event in AreaManager
    public void OnPlayerEnteredArea(object sender, AreaManager.PlayerEnteredAreaArgs e)
    {
        _playerSpotted = true;
        _playerTransform = e.TargetPlayer.transform;
        _navMeshAgent.speed = activeSpeed;
    }
}