using System;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static event Action OnTimerEnd;

    [SerializeField] private float timeLimit = 60f;

    private float currentTime;
    private bool running = true;

    public float CurrentTime => currentTime;

    private void Start()
    {
        currentTime = timeLimit;
    }

    private void Update()
    {
        if (!running) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            running = false;

            OnTimerEnd?.Invoke();
        }
    }
}