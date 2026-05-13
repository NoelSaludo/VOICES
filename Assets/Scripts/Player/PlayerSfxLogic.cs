using UnityEngine;

public class PlayerSfxLogic
{
    private float nextWalkingSfxTime;

    public bool TryScheduleWalking(Player player, float walkingInterval)
    {
        if (player.MoveInput != 0f && player.IsGrounded() && Time.time >= nextWalkingSfxTime)
        {
            nextWalkingSfxTime = Time.time + walkingInterval;
            return true;
        }

        return false;
    }

    public bool ShouldPlayDragging(Player player, float draggingInterval)
    {
        if (player.IsGrounded() &&
            player.State == PlayerState.MovingObject &&
            Time.time >= draggingInterval)
        {
            return true;
        }

        return false;
    }
}
