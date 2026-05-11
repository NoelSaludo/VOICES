using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Grounding")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayers;

    private Rigidbody2D rb;
    private Collider2D col;
    private float moveInput;
    private bool jumpQueued;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (groundLayers == 0)
        {
            groundLayers = Physics2D.AllLayers;
        }
    }
    private void Update()
    {
        moveInput = 0f;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                moveInput = -1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                moveInput = 1f;
            }

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                jumpQueued = true;
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;
        rb.linearVelocity = velocity;

        if (jumpQueued && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        jumpQueued = false;
    }

    private bool IsGrounded()
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryInteract(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryInteract(collision.gameObject);
    }

    private void TryInteract(GameObject other)
    {
        IPlayerInteractable interactable = other.GetComponent<IPlayerInteractable>();
        if (interactable != null)
        {
            interactable.Interact(this);
        }
    }
}

public interface IPlayerInteractable
{
    void Interact(Player player);
}
