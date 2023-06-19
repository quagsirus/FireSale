using UnityEngine;

public class Player : MonoBehaviour
{
    // Constant modifiers
    private const float Speed = 1f;

    // Animator and parameters
    private static readonly int AnimatorDirection = Animator.StringToHash("Direction");

    private Animator _animator;

    // Facing direction for use in animation
    private Direction _direction;

    private Direction CurrentDirection
    {
        get => _direction;
        set
        {
            if (_direction == value) return;
            _direction = value;
            OnDirectionChanged(value);
        }
    }

    private void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
    }

    private void Update()
    {
        // Get player inputs and create vector
        var inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // Clamp to length of 1 otherwise diagonal input would be 40% faster
        var finalMovement = Vector2.ClampMagnitude(inputDirection, 1.0f);

        // Avoid unnecessary calculations
        if (finalMovement != Vector2.zero)
            // Get facing direction based on angle of movement
            CurrentDirection = Vector2.SignedAngle(Vector2.up, finalMovement) switch
            {
                < -135 or > 135 => Direction.Down,
                >= 45 => Direction.Right,
                <= -45 => Direction.Left,
                _ => Direction.Up
            };

            // Actually move, accounting for frame times and walk speed
            transform.Translate((Vector3)finalMovement * (Time.deltaTime * Speed), Space.World);
    }

    private void OnDirectionChanged(Direction direction)
    {
        _animator.SetInteger(AnimatorDirection, (int)direction);
    }

    private enum Direction
    {
        Down,
        Up,
        Left,
        Right
    }
}