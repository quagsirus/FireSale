using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Constant modifiers
    private const float Speed = 1f;

    // Animator and parameters
    private static readonly int AnimatorCurrentDirection = Animator.StringToHash("CurrentDirection");
    private static readonly int AnimatorDirection = Animator.StringToHash("Direction");
    private static readonly int AnimatorHolding = Animator.StringToHash("Holding");
    private static readonly int AnimatorRunning = Animator.StringToHash("Running");

    private Animator _animator;

    // Parameters for use in animation
    private Direction _direction;

    // Rigidbody for solid collision detection and movement
    private Rigidbody2D _rigidbody2D;
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
        _animator = gameObject.GetComponent<Animator>();
        _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Stop animator locking up
        _animator.SetInteger(AnimatorCurrentDirection, _animator.GetInteger(AnimatorDirection));

        // Get player inputs and create vector
        var inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // Clamp to length of 1 otherwise diagonal input would be 40% faster
        var finalMovement = Vector2.ClampMagnitude(inputDirection, 1.0f);

        // Check if moving and send to animator state
        var isMoving = finalMovement != Vector2.zero;
        CurrentState = isMoving ? CurrentState | State.Running : CurrentState & ~State.Running;

        // Avoid unnecessary calculations
        if (isMoving)
        {
            // Get facing direction based on angle of movement
            CurrentDirection = Vector2.SignedAngle(Vector2.up, finalMovement) switch
            {
                < -135 or > 135 => Direction.Down,
                >= 45 => Direction.Right,
                <= -45 => Direction.Left,
                _ => Direction.Up
            };

            // Actually move, accounting for frame times and walk speed
            _rigidbody2D.MovePosition(_rigidbody2D.position + finalMovement * (Time.deltaTime * Speed));
        }
    }

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