using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private bool facingRight = true;

    private void Awake()
    {
        // Auto-find references if missing
        if (player == null)
            player = GetComponent<Player>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (player == null) return;

        HandleFacing();
        HandleAnimations();
    }

    // ---------------------------
    // ANIMATIONS
    // ---------------------------
    private void HandleAnimations()
    {
        if (animator == null) return;

        bool grounded = player.IsGrounded();
        bool isMoving = Mathf.Abs(player.MoveInput) > 0.1f;

        // RUN
        animator.SetBool("isRunning", isMoving && grounded);

        // JUMP
        animator.SetBool("isJumping", !grounded);
    }

    // ---------------------------
    // SPRITE FLIP
    // ---------------------------
    private void HandleFacing()
    {
        float moveInput = player.MoveInput;

        if (moveInput > 0.01f)
            facingRight = true;
        else if (moveInput < -0.01f)
            facingRight = false;

        if (spriteRenderer == null) return;

        // Sprite image default faces LEFT
        // So:
        // flipX = true  → RIGHT
        // flipX = false → LEFT
        spriteRenderer.flipX = facingRight;
    }
}