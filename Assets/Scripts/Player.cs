using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Constant modifiers
    private const float Speed = 5f;

    // Animator parameter id caches
    private static readonly int AnimatorCurrentDirection = Animator.StringToHash("CurrentDirection");
    private static readonly int AnimatorDirection = Animator.StringToHash("Direction");
    private static readonly int AnimatorHolding = Animator.StringToHash("Holding");
    private static readonly int AnimatorRunning = Animator.StringToHash("Running");

    // Actions wrapper instance field (instantiated on Enable)
    private GameActions _actions;

    // Animator component (assigned on Awake)
    private Animator _animator;

    // Last movement direction (4 way)
    private Direction _direction;

    // Rigidbody for solid collision detection and movement
    private Rigidbody2D _rigidbody2D;

    // Animation state
    private State _state;

    private Direction CurrentDirection
    {
        set
        {
            // Check if the value is any different
            if (_direction == value) return;
            // Update animator with new direction
            _animator.SetInteger(AnimatorDirection, (int)value);
            // Save the new Direction
            _direction = value;
        }
    }

    private State CurrentState
    {
        get => _state;
        set
        {
            // Check if the value is any different
            if (_state == value) return;
            // Get all the flags that have been added/removed
            var changedFlags = _state ^ value;
            // Update flags in animator to reflect changes
            if (changedFlags.HasFlag(State.Holding)) _animator.SetBool(AnimatorHolding, value.HasFlag(State.Holding));
            if (changedFlags.HasFlag(State.Running)) _animator.SetBool(AnimatorRunning, value.HasFlag(State.Running));
            // Save the new State
            _state = value;
        }
    }

    private void Awake()
    {
        // Cache player components on spawn
        _animator = gameObject.GetComponent<Animator>();
        _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();

        // Instantiate game actions wrapper class
        _actions = new GameActions();
        // Bind interact function to event
        _actions.gameplay.interact.performed += OnInteract;
    }

    private void Update()
    {
        // Stop animator locking up
        _animator.SetInteger(AnimatorCurrentDirection, _animator.GetInteger(AnimatorDirection));

        // Get player inputs and create vector
        // Input System clamps magnitude to 1 otherwise diagonal input would be 40% faster
        var inputDirection = _actions.gameplay.movement.ReadValue<Vector2>();

        // Check if moving and send to animator state
        var isMoving = inputDirection != Vector2.zero;
        CurrentState = isMoving ? CurrentState | State.Running : CurrentState & ~State.Running;

        // Avoid unnecessary calculations
        if (!isMoving) return;
        // Get facing direction based on angle of movement
        CurrentDirection = Vector2.SignedAngle(Vector2.up, inputDirection) switch
        {
            < -135 or > 135 => Direction.Down,
            >= 45 => Direction.Right,
            <= -45 => Direction.Left,
            _ => Direction.Up
        };

        // Actually move, accounting for frame times and walk speed
        _rigidbody2D.MovePosition(_rigidbody2D.position + inputDirection * (Time.deltaTime * Speed));
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

    // Enumerators for controlling animator parameters
    private enum Direction
    {
        Down,
        Up,
        Left,
        Right
    }

    [Flags]
    private enum State
    {
        Idle = 0,
        Running = 1,
        Holding = 2
    }
}