using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
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
    private float moveInput;
    private bool jumpQueued;
    private PlayerState state = PlayerState.Free;
    private IPlayerInteractable currentInteractable;
    private Crate attachedCrate;
    private FixedJoint2D moveJoint;
    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction jumpAction;
    private InputAction dropAction;
    private bool dropQueued;
    private Platform currentPlatform;
    private Collider2D currentPlatformCollider;

    public float MoveInput => moveInput;
    public PlayerState State => state;

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
    }
    private void Update()
    {
        moveInput = moveAction.ReadValue<float>();

        if (interactAction.WasPressedThisFrame())
        {
            HandleInteract();
        }
        if (dropAction != null && dropAction.WasPressedThisFrame())
        {
            dropQueued = true;
        }
        if (jumpAction.WasPressedThisFrame())
        {
            jumpQueued = true;
        }
    }

    private void FixedUpdate()
    {
        float speed = moveSpeed;
        if (state == PlayerState.MovingObject)
        {
            speed *= movingObjectSpeedMultiplier;
        }

        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * speed;
        rb.linearVelocity = velocity;

        if (state == PlayerState.Free && jumpQueued && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (dropQueued)
        {
            TryDropThroughPlatform();
            dropQueued = false;
        }

        jumpQueued = false;
    }

    public bool IsGrounded()
    {
        Vector2 checkPos;

        if (groundCheck != null)
        {
            checkPos = groundCheck.position;
        }
        else
        {
            float y = col.bounds.min.y - 0.02f;
            checkPos = new Vector2(col.bounds.center.x, y);
        }

        return Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayers) != null;
    }

    private void HandleInteract()
    {
        if (state == PlayerState.MovingObject && attachedCrate != null)
        {
            attachedCrate.Interact(this);
            return;
        }

        if (currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }

    public void AttachToCrate(Crate crate, Vector2 attachPosition)
    {
        if (crate == null || state == PlayerState.MovingObject)
        {
            return;
        }

        attachedCrate = crate;
        state = PlayerState.MovingObject;
        rb.position = attachPosition;
        rb.linearVelocity = Vector2.zero;
        jumpQueued = false;

        if (moveJoint != null)
        {
            Destroy(moveJoint);
        }

        Rigidbody2D crateBody = crate.GetComponent<Rigidbody2D>();
        if (crateBody != null)
        {
            moveJoint = gameObject.AddComponent<FixedJoint2D>();
            moveJoint.connectedBody = crateBody;
            moveJoint.autoConfigureConnectedAnchor = true;
            moveJoint.enableCollision = false;
        }
    }

    public void DetachFromCrate(Crate crate)
    {
        if (attachedCrate != crate)
        {
            return;
        }

        attachedCrate = null;
        state = PlayerState.Free;

        if (moveJoint != null)
        {
            Destroy(moveJoint);
            moveJoint = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SetInteractable(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ClearInteractable(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        SetInteractable(collision.collider);
        UpdateCurrentPlatform(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateCurrentPlatform(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        ClearInteractable(collision.collider);
        if (currentPlatformCollider == collision.collider)
        {
            currentPlatform = null;
            currentPlatformCollider = null;
        }
    }

    private void TryDropThroughPlatform()
    {
        if (state != PlayerState.Free)
        {
            return;
        }

        if (currentPlatform == null || currentPlatformCollider == null)
        {
            return;
        }

        if (!IsGrounded())
        {
            return;
        }

        currentPlatform.RequestDrop(col);
    }

    private void UpdateCurrentPlatform(Collision2D collision)
    {
        Platform platform = collision.collider.GetComponent<Platform>();
        if (platform == null)
        {
            return;
        }

        if (IsStandingOnCollision(collision))
        {
            currentPlatform = platform;
            currentPlatformCollider = collision.collider;
        }
        else if (currentPlatformCollider == collision.collider)
        {
            currentPlatform = null;
            currentPlatformCollider = null;
        }
    }

    private bool IsStandingOnCollision(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    private void SetInteractable(Collider2D other)
    {
        IPlayerInteractable interactable = other.GetComponent<IPlayerInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
        }
    }

    private void ClearInteractable(Collider2D other)
    {
        IPlayerInteractable interactable = other.GetComponent<IPlayerInteractable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable = null;
        }
    }
}

public interface IPlayerInteractable
{
    void Interact(Player player);
}
