using System;
using UnityEngine;

public class GameStateController
{
    private float previousTimeScale = 1f;
    private GameState state;

    public GameState State => state;

    public void SetState(
        GameState newState,
        bool force,
        bool freezeTimeWhenPaused,
        bool freezeTimeWhenEnded,
        bool freezeTimeWhenDialogue,
        Action<GameState> onStateChanged,
        Action<GameState> applyUiState)
    {
        if (!force && state == newState)
        {
            return;
        }

        state = newState;

        if (state == GameState.Paused && freezeTimeWhenPaused)
        {
            CacheTimeScale();
            Time.timeScale = 0f;
        }
        else if (state == GameState.Ended && freezeTimeWhenEnded)
        {
            CacheTimeScale();
            Time.timeScale = 0f;
            applyUiState?.Invoke(GameState.Ended);
        }
        else if (state == GameState.Dialogue && freezeTimeWhenDialogue)
        {
            CacheTimeScale();
            Time.timeScale = 0f;
        }
        else if (state == GameState.Playing)
        {
            applyUiState?.Invoke(GameState.Playing);
            Time.timeScale = Mathf.Approximately(previousTimeScale, 0f) ? 1f : previousTimeScale;
        }

        onStateChanged?.Invoke(state);
    }

    private void CacheTimeScale()
    {
        if (Time.timeScale > 0f)
        {
            previousTimeScale = Time.timeScale;
        }
    }
}
