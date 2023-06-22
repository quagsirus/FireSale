using System;
using UnityEngine;

public class AnimationStateController
{
    // Animator parameter id caches
    private static readonly int AnimatorCurrentDirection = Animator.StringToHash("CurrentDirection");
    private static readonly int AnimatorDirection = Animator.StringToHash("Direction");
    private static readonly int AnimatorHolding = Animator.StringToHash("Holding");
    private static readonly int AnimatorRunning = Animator.StringToHash("Running");
    private static readonly int AnimatorWalking = Animator.StringToHash("Walking");

    // Animator component on GameObject
    private readonly Animator _animator;

    // Current movement direction (4 way)
    private Direction _direction;

    // Animation state
    private State _state;

    // Class constructor
    public AnimationStateController(Animator animator)
    {
        // Requires above parameters to be present
        _animator = animator;
    }

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
            if (changedFlags.HasFlag(State.Walking)) _animator.SetBool(AnimatorWalking, value.HasFlag(State.Walking));
            // Save the new State
            _state = value;
        }
    }

    public void FixDirectionLockup()
    {
        // Stop animator locking up
        _animator.SetInteger(AnimatorCurrentDirection, _animator.GetInteger(AnimatorDirection));
    }

    public void SetHoldingState(bool holding)
    {
        // Enable / disable Running flag based on input bool
        CurrentState = holding ? CurrentState | State.Holding : CurrentState & ~State.Holding;
    }

    public void SetRunningState(bool running, bool slow = false)
    {
        // Switch between Walking, Running or neither
        CurrentState = running
            ? slow
                ? (CurrentState | State.Walking) & ~State.Running
                : CurrentState | (State.Running & ~State.Walking)
            : CurrentState & ~(State.Walking | State.Running);
    }

    public void SetMovementDirection(Vector2 movementVector2)
    {
        // Get facing direction based on angle of movement
        CurrentDirection = Vector2.SignedAngle(Vector2.up, movementVector2) switch
        {
            < -135 or > 135 => Direction.Down,
            >= 45 => Direction.Left,
            <= -45 => Direction.Right,
            _ => Direction.Up
        };
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
        Running = 1,
        Holding = 2,
        Walking = 4
    }
}