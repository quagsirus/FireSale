using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Constant modifiers
    private const float Speed = 5f;

    // Actions wrapper instance field (instantiated on Enable)
    private GameActions _actions;

    // Animator component (instantiated on Awake)
    private AnimationStateController _animationStateController;

    // Rigidbody for solid collision detection and movement
    private Rigidbody2D _rigidbody2D;

    private void Awake()
    {
        // Cache player components on spawn
        _animationStateController = new AnimationStateController(gameObject.GetComponent<Animator>());
        _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();

        // Instantiate game actions wrapper class
        _actions = new GameActions();
        // Bind interact function to event
        _actions.gameplay.interact.performed += OnInteract;
    }

    private void Update()
    {
        // Stop animator locking up
        _animationStateController.FixDirectionLockup();

        // Get player inputs and create vector
        // Input System clamps magnitude to 1 otherwise diagonal input would be 40% faster
        var movementVector2 = _actions.gameplay.movement.ReadValue<Vector2>();

        // Check if moving and send to animation state controller
        var isMoving = movementVector2 != Vector2.zero;
        _animationStateController.SetRunningState(isMoving);

        // Avoid unnecessary calculations when no change to position needs to be made
        if (!isMoving) return;

        // Update animation movement direction
        _animationStateController.SetMovementDirection(movementVector2);
        // Actually move, accounting for frame times and walk speed
        _rigidbody2D.MovePosition(_rigidbody2D.position + movementVector2 * (Time.deltaTime * Speed));
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
}