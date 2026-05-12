using System;
using UnityEngine;

public class PlayerPlatformHandler
{
    private readonly Collider2D playerCollider;
    private readonly Func<bool> isGrounded;

    private Platform currentPlatform;
    private Collider2D currentPlatformCollider;

    public PlayerPlatformHandler(Collider2D playerCollider, Func<bool> isGrounded)
    {
        this.playerCollider = playerCollider;
        this.isGrounded = isGrounded;
    }

    public void OnCollisionEnter(Collision2D collision)
    {
        UpdateCurrentPlatform(collision);
    }

    public void OnCollisionStay(Collision2D collision)
    {
        UpdateCurrentPlatform(collision);
    }

    public void OnCollisionExit(Collision2D collision)
    {
        if (currentPlatformCollider == collision.collider)
        {
            currentPlatform = null;
            currentPlatformCollider = null;
        }
    }

    public void TryDropThroughPlatform(PlayerState state)
    {
        if (state != PlayerState.Free)
        {
            return;
        }

        if (currentPlatform == null || currentPlatformCollider == null)
        {
            return;
        }

        if (!isGrounded())
        {
            return;
        }

        currentPlatform.RequestDrop(playerCollider);
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
}
