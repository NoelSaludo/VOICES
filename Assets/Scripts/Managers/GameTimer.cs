using UnityEngine;

public class GameTimer
{
    private float remainingTime;

    public float RemainingTime => remainingTime;
    public bool IsComplete => remainingTime <= 0f;

    public void Tick(float deltaTime)
    {
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            return;
        }

        remainingTime = Mathf.Max(0f, remainingTime - deltaTime);
    }

    public void Reset(float startingTime)
    {
        remainingTime = Mathf.Max(0f, startingTime);
    }
}
