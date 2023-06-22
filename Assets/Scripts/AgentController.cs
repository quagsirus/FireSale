using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AgentController : MonoBehaviour
{
    // Aggressive flag (true for player tracking, false for chicken running)
    public bool aggressive;
    public float aggressiveAttackDelay;
    public float aggressiveStoppingDistance;

    // Movement speeds
    public float wanderSpeed;
    public float activeSpeed;

    // Wander behavior configuration
    public float wanderRepositionDelay;
    public float wanderMaxDistance;
    public float chickenRepositionDelay;
    public float chickenMaxDistance;

    // AreaManager that agent spawned in
    public AreaManager assignedAreaManager;

    // Location that ammo is spawned relative to pivot
    public Vector2[] ammoOffsetVectors;

    // The object instantiated on fire
    public GameObject ammoType;

    // AnimationStateController (instantiated on Awake)
    private AnimationStateController _animationStateController;

    // Timer for attack delay
    private float _attackTimer;

    // True when dead, stops most functions in Update
    private bool _dead;

    // NavMeshAgent component (located on Awake)
    private NavMeshAgent _navMeshAgent;

    // True for one frame when direction changed outside Update to allow animator to work
    private bool _pauseAntiLockup;

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
        Destroy(gameObject);
        throw new Exception("Invalid spawn location");
    }

    private void Update()
    {
        // Stop animator locking up
        if (_pauseAntiLockup)
            _pauseAntiLockup = false;
        else
            _animationStateController.FixDirectionLockup();

        if (_dead)
            return;
        if (!_playerTransform && _playerSpotted)
        {
            _playerSpotted = false;
            _animationStateController.SetHoldingState(false);
        }


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
        else if (aggressive && _playerSpotted)
        {
            // Update animation movement direction
            _animationStateController.SetMovementDirection(_playerTransform.position - position);
            // Increment timer
            _attackTimer += Time.deltaTime;
            if (_attackTimer > aggressiveAttackDelay)
            {
                // Get future start location of bullet
                var ammoInstancePosition = ammoOffsetVectors[(int)_animationStateController.CurrentDirection] +
                                           (Vector2)transform.position;
                // Get angle in degrees towards player
                var angleToTarget = Vector2.SignedAngle(Vector2.right,
                    Vector2.ClampMagnitude(_navMeshAgent.destination - position, 1));
                // Spawn bullet
                Instantiate(ammoType, ammoInstancePosition,
                    Quaternion.Euler(0, 0, angleToTarget));
                // Reset timer
                _attackTimer = 0;
            }
        }

        // Update running/idle state based on if stopping distance has been reached
        _animationStateController.SetRunningState(isMoving, !_playerSpotted && !aggressive);
    }

    // Subscribed to PlayerEnteredArea event in AreaManager
    public void OnPlayerEnteredArea(object sender, AreaManager.PlayerEnteredAreaArgs e)
    {
        // Switch NavMeshAgent parameters to aggressive variants
        if (aggressive)
            _navMeshAgent.stoppingDistance = aggressiveStoppingDistance;
        _navMeshAgent.speed = activeSpeed;
        // True for Update loop
        _playerSpotted = true;
        // Store target Transform
        _playerTransform = e.TargetPlayer.transform;
        // Change animation variant to weapon type
        _animationStateController.SetHoldingState(true);
    }

    public void OnShot()
    {
        // Unsubscribe from AreaManager event
        assignedAreaManager.PlayerEnteredArea -= OnPlayerEnteredArea;
        // Ensures animation will switch correctly
        _pauseAntiLockup = true;
        // Stops majority of Update loop code
        _dead = true;
        // Play smoke animation
        _animationStateController.Die();

        Destroy(gameObject, 1f);
    }
}