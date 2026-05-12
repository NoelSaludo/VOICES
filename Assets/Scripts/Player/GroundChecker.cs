using UnityEngine;

public class GroundChecker
{
    private readonly Transform groundCheck;
    private readonly Collider2D col;
    private readonly float groundCheckRadius;
    private readonly LayerMask groundLayers;

    public GroundChecker(Transform groundCheck, Collider2D col, float groundCheckRadius, LayerMask groundLayers)
    {
        this.groundCheck = groundCheck;
        this.col = col;
        this.groundCheckRadius = groundCheckRadius;
        this.groundLayers = groundLayers;
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
}
