using System;
using UnityEngine;

public class PlayerMovementController
{
    private readonly Rigidbody2D rb;
    private bool jumpQueued;
    private bool dropQueued;

    public float MoveInput { get; private set; }

    public PlayerMovementController(Rigidbody2D rb)
    {
        this.rb = rb;
    }

    public void SetMoveInput(float input)
    {
        MoveInput = input;
    }

    public void QueueJump()
    {
        jumpQueued = true;
    }

    public void QueueDrop()
    {
        dropQueued = true;
    }

    public void ClearJumpQueue()
    {
        jumpQueued = false;
    }

    public void ClearQueuedInputs()
    {
        jumpQueued = false;
        dropQueued = false;
    }

    public void Tick(
        PlayerState state,
        float moveSpeed,
        float jumpForce,
        float movingObjectSpeedMultiplier,
        Func<bool> isGrounded,
        Action dropAction)
    {
        float speed = moveSpeed;
        if (state == PlayerState.MovingObject)
        {
            speed *= movingObjectSpeedMultiplier;
        }

        Vector2 velocity = rb.linearVelocity;
        velocity.x = MoveInput * speed;
        rb.linearVelocity = velocity;

        if (state == PlayerState.Free && jumpQueued && isGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (dropQueued)
        {
            dropAction();
            dropQueued = false;
        }

        jumpQueued = false;
    }
}
