using System;
using UnityEngine;

public class GameOverZone : MonoBehaviour
{
    public static event Action OnPlayerDeath;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerDeath?.Invoke();
        }
    }
}