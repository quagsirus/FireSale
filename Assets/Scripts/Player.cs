using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Constant modifiers
    private const float Speed = 5f;

    // Location that ammo is spawned relative to player pivot
    public Vector2[] ammoOffsetVectors;

    // The object instantiated on fire
    public GameObject ammoType;

    // Actions wrapper instance field (instantiated on Enable)
    private GameActions _actions;

    // AnimationStateController (instantiated on Awake)
    private AnimationStateController _animationStateController;

    // True when dead, stops most functions in Update
    private bool _dead;

    // Vector2 storing current movement input
    private Vector2 _movementVector2;

    // True for one frame when direction changed outside Update to allow animator to work
    private bool _pauseAntiLockup;

    // Rigidbody for solid collision detection and movement
    private Rigidbody2D _rigidbody2D;

    private void Awake()
    {
        // Instantiate AnimationStateController
        _animationStateController = new AnimationStateController(gameObject.GetComponent<Animator>());

        // Cache Rigidbody2D component on spawn
        _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();

        // Instantiate game actions wrapper class
        _actions = new GameActions();
        // Bind interact function to event
        _actions.gameplay.interact.performed += OnInteract;
        // Bind primary fire function to event
        _actions.gameplay.primaryFire.performed += OnPrimaryFire;
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

        // Get player inputs and set Vector2
        // Input System clamps magnitude to 1 otherwise diagonal input would be 40% faster
        _movementVector2 = _actions.gameplay.movement.ReadValue<Vector2>();

        // Check if moving and send to animation state controller
        var isMoving = _movementVector2 != Vector2.zero;
        _animationStateController.SetRunningState(isMoving);

        // Avoid unnecessary calculations when no change to position needs to be made
        if (!isMoving) return;

        // Update animation movement direction
        _animationStateController.SetMovementDirection(_movementVector2);
    }

    private void FixedUpdate()
    {
        // Actually move, accounting for frame times and walk speed
        _rigidbody2D.MovePosition(_rigidbody2D.position + _movementVector2 * (Time.deltaTime * Speed));
    }

    private void OnEnable()
    {
        // Start capturing input defined in gameplay map
        _actions.gameplay.Enable();
    }

    private void OnDisable()
    {
        // Stop capturing input defined in gameplay map
        _actions.gameplay.Disable();
    }

    private static void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Interact!");
    }

    private void OnPrimaryFire(InputAction.CallbackContext context)
    {
        // Don't try to shoot if dead
        if (_dead)
            return;
        // Change animation variant to weapon type
        _animationStateController.SetHoldingState(true);
        Instantiate(ammoType,
            ammoOffsetVectors[(int)_animationStateController.CurrentDirection] + (Vector2)transform.position,
            Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, _animationStateController.GetFacingVector2())));
    }

    public void OnShot()
    {
        Debug.Log(":3");
        // Ensures animation will switch correctly
        _pauseAntiLockup = true;
        // Stops majority of Update loop code
        _dead = true;
        // Stop collision checks
        _rigidbody2D.simulated = false;
        // Keep camera working
        transform.DetachChildren();
        // Play smoke animation
        _animationStateController.Die();

        Destroy(gameObject, 1);
    }
}