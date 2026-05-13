using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Free,
    MovingObject
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Player : MonoBehaviour
{
    private const string InteractionPromptPrefix = "E to ";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Interaction")]
    [SerializeField] private float movingObjectSpeedMultiplier = 0.5f;

    [Header("Grounding")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayers;

    private Rigidbody2D rb;
    private Collider2D col;
    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction jumpAction;
    private InputAction dropAction;
    private GroundChecker groundChecker;
    private PlayerMovementController movementController;
    private PlayerInteractionHandler interactionHandler;
    private PlayerPlatformHandler platformHandler;

    public float MoveInput => movementController != null ? movementController.MoveInput : 0f;
    public PlayerState State => interactionHandler != null ? interactionHandler.State : PlayerState.Free;

    // ---------------------------
    // INIT
    // ---------------------------
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            anim = GetComponent<Animator>();

        if (groundLayers == 0)
            groundLayers = Physics2D.AllLayers;

        moveAction = InputSystem.actions.FindAction("Move");
        interactAction = InputSystem.actions.FindAction("Interact");
        jumpAction = InputSystem.actions.FindAction("Jump");
        dropAction = InputSystem.actions.FindAction("Drop");

        groundChecker = new GroundChecker(groundCheck, col, groundCheckRadius, groundLayers);
        movementController = new PlayerMovementController(rb);
        interactionHandler = new PlayerInteractionHandler(this, rb, gameObject);
        platformHandler = new PlayerPlatformHandler(col, groundChecker.IsGrounded);
    }

    private void OnEnable()
    {
        if (interactionHandler != null)
        {
            interactionHandler.InteractableChanged += HandleInteractableChanged;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(this);
        }
    }

    private void OnDisable()
    {
        if (interactionHandler != null)
        {
            interactionHandler.InteractableChanged -= HandleInteractableChanged;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Stalker)
        {
            movementController.SetMoveInput(0f);
            movementController.ClearQueuedInputs();
            return;
        }

        movementController.SetMoveInput(moveAction.ReadValue<float>());

        if (interactAction.WasPressedThisFrame())
        {
            interactionHandler.HandleInteract();
            UpdateInteractionPrompt(interactionHandler.CurrentInteractable);
        }
        if (dropAction != null && dropAction.WasPressedThisFrame())
        {
            movementController.QueueDrop();
        }
        if (jumpAction.WasPressedThisFrame())
        {
            movementController.QueueJump();
        }
    }

    private void FixedUpdate()
    {
        movementController.Tick(
            State,
            moveSpeed,
            jumpForce,
            movingObjectSpeedMultiplier,
            groundChecker.IsGrounded,
            () => platformHandler.TryDropThroughPlatform(State));
    }

    // ---------------------------
    // ANIMATIONS
    // ---------------------------
    private void HandleAnimations()
{
    if (anim == null) return;

    bool grounded = IsGrounded();
    bool isMoving = Mathf.Abs(moveInput) > 0.1f;

    // RUN
    anim.SetBool("isRunning", isMoving && grounded);

    // JUMP (IMPORTANT FIX)
    anim.SetBool("isJumping", !grounded);
}

    // ---------------------------
    // FACING (FLIPX)
    // ---------------------------
    private void HandleFacing()
    {
        if (moveInput > 0.01f)
            facingRight = true;
        else if (moveInput < -0.01f)
            facingRight = false;

        if (sr == null) return;

        // Sprite default = RIGHT
        sr.flipX = facingRight;
    }

    // ---------------------------
    // GROUND CHECK
    // ---------------------------
    public bool IsGrounded()
    {
        return groundChecker != null && groundChecker.IsGrounded();
    }

    // ---------------------------
    // CRATE SYSTEM
    // ---------------------------
    public void AttachToCrate(Crate crate, Vector2 attachPosition)
    {
        if (interactionHandler.AttachToCrate(crate, attachPosition))
        {
            movementController.ClearJumpQueue();
        }
    }

    public void DetachFromCrate(Crate crate)
    {
        interactionHandler.DetachFromCrate(crate);
    }

    // ---------------------------
    // PLATFORM DROP
    // ---------------------------
    private void TryDropThroughPlatform()
    {
        interactionHandler.SetInteractable(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        interactionHandler.ClearInteractable(other);
    }

    // ---------------------------
    // COLLISIONS
    // ---------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        interactionHandler.SetInteractable(collision.collider);
        platformHandler.OnCollisionEnter(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        platformHandler.OnCollisionStay(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        interactionHandler.ClearInteractable(collision.collider);
        platformHandler.OnCollisionExit(collision);
    }

    private void HandleInteractableChanged(IPlayerInteractable interactable)
    {
        UpdateInteractionPrompt(interactable);
    }

    private void UpdateInteractionPrompt(IPlayerInteractable interactable)
    {
        UIManager uiManager = UIManager.Instance;
        if (uiManager == null)
        {
            return;
        }

        IPlayerInteractable promptInteractable = interactable;
        if (State == PlayerState.MovingObject && interactionHandler != null && interactionHandler.AttachedCrate != null)
        {
            promptInteractable = interactionHandler.AttachedCrate;
        }

        if (!IsInteractableValid(promptInteractable))
        {
            uiManager.HideInteractionPrompt();
            return;
        }

        string verb = promptInteractable.InteractionVerb;
        if (string.IsNullOrWhiteSpace(verb))
        {
            verb = "Interact";
        }

        uiManager.ShowInteractionPrompt($"{InteractionPromptPrefix}{verb}", promptInteractable.PromptAnchor);
    }

    // ---------------------------
    // INTERACTABLES
    // ---------------------------
    private void OnTriggerEnter2D(Collider2D other) => SetInteractable(other);

    private void OnTriggerExit2D(Collider2D other) => ClearInteractable(other);

    private bool IsInteractableValid(IPlayerInteractable interactable)
    {
        if (interactable == null)
        {
            return false;
        }

        if (interactable is Object unityObject && unityObject == null)
        {
            return false;
        }

        return true;
    }
}

// ---------------------------
// INTERFACE
// ---------------------------
public interface IPlayerInteractable
{
    void Interact(Player player);
    string InteractionVerb { get; }
    Transform PromptAnchor { get; }
}