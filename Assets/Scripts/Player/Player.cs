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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (groundLayers == 0)
        {
            groundLayers = Physics2D.AllLayers;
        }

        moveAction = InputSystem.actions.FindAction("Move");
        interactAction = InputSystem.actions.FindAction("Interact");
        jumpAction = InputSystem.actions.FindAction("Jump");
        dropAction = InputSystem.actions.FindAction("Drop");

        groundChecker = new GroundChecker(groundCheck, col, groundCheckRadius, groundLayers);
        movementController = new PlayerMovementController(rb);
        interactionHandler = new PlayerInteractionHandler(this, rb, gameObject);
        platformHandler = new PlayerPlatformHandler(col, groundChecker.IsGrounded);
    }
    private void Update()
    {
        movementController.SetMoveInput(moveAction.ReadValue<float>());

        if (interactAction.WasPressedThisFrame())
        {
            interactionHandler.HandleInteract();
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

    public bool IsGrounded()
    {
        return groundChecker != null && groundChecker.IsGrounded();
    }

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        interactionHandler.SetInteractable(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        interactionHandler.ClearInteractable(other);
    }

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
}

public interface IPlayerInteractable
{
    void Interact(Player player);
}
