using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // Constant modifiers
    private const float Speed = 5f;

    // Location that ammo is spawned relative to player pivot
    public Vector2[] ammoOffsetVectors;

    // The object instantiated on fire
    public GameObject ammoType;

    // Number of hits player can take before death
    public int lives;

    // Sprite for updating health display
    public Sprite deadIcon;

    // Where to discover life icons
    public GameObject healthPanel;

    // Game over screen to enable on death
    public GameObject gameOverPanel;

    // Win screen to show after time gun
    public GameObject winPanel;

    // LayerMask for interactions
    public LayerMask interactableLayerMask;

    // What to change the elevator's sprite renderer to after opening
    public Sprite openElevatorSprite;

    // Actions wrapper instance field (instantiated on Enable)
    private GameActions _actions;

    // AnimationStateController (instantiated on Awake)
    private AnimationStateController _animationStateController;

    private BoxCollider2D _boxCollider;

    // True when dead, stops most functions in Update
    private bool _dead;

    // True after interacted with key card on floor
    private bool _hasKeycard;

    // Stores all discovered life icon Image components
    private Image[] _healthIcons;

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

        // Cache Rigidbody2D and BoxCollider2D components on spawn
        _rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        _boxCollider = gameObject.GetComponent<BoxCollider2D>();

        // Discover health icons
        _healthIcons = healthPanel.GetComponentsInChildren<Image>();

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

    private void OnInteract(InputAction.CallbackContext context)
    {
        // Check what objects are around us
        var interactedWithObject =
            Physics2D.OverlapBox(transform.position, _boxCollider.size * 2, 0, interactableLayerMask);
        // Only continue if we are near something interactable
        if (interactedWithObject == null) return;
        if (interactedWithObject.gameObject.CompareTag("Keycard"))
        {
            _hasKeycard = true;
            Destroy(interactedWithObject.gameObject);
        }
        else if (interactedWithObject.gameObject.CompareTag("ElevatorDown") && _hasKeycard)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else if (interactedWithObject.gameObject.CompareTag("ElevatorSummon") && _hasKeycard)
        {
            var spriteRenderer = interactedWithObject.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = openElevatorSprite;
            spriteRenderer.sortingOrder += 1;
            interactedWithObject.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
        else if (interactedWithObject.gameObject.CompareTag("TimeGun"))
        {
            winPanel.SetActive(true);
            Destroy(interactedWithObject.gameObject);
        }
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
        // Take off a life
        lives--;
        _healthIcons[9 - lives].sprite = deadIcon;
        // Only die if 0 health
        if (lives > 0)
            return;
        // Show game over screen
        gameOverPanel.SetActive(true);
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