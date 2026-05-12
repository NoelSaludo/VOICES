using System;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Paused,
    Dialogue,
    Ended
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<GameState> OnStateChanged;
    public event Action<float> OnTimerChanged;
    public event Action<Vector3> OnPlayerPositionChanged;

    [Header("State")]
    [SerializeField] private GameState initialState = GameState.Playing;
    [SerializeField] private bool freezeTimeWhenPaused = true;
    [SerializeField] private bool freezeTimeWhenEnded = true;
    [SerializeField] private bool freezeTimeWhenDialogue = true;

    [Header("Timer")]
    [SerializeField] private bool runTimer = true;

    [Header("Player Tracking")]
    [SerializeField] private Transform player;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float positionEpsilon = 0.0001f;

    private float elapsedTime;
    private GameState state;
    private Vector3 lastPlayerPosition;
    private float previousTimeScale = 1f;
    private UIManager uiManager;

    public GameState State => state;
    public float ElapsedTime => elapsedTime;
    public Vector3 PlayerPosition => lastPlayerPosition;
    public bool IsPlaying => state == GameState.Playing;
    public bool IsPaused => state == GameState.Paused;
    public bool IsEnded => state == GameState.Ended;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (autoFindPlayer && player == null && !string.IsNullOrEmpty(playerTag))
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player != null)
        {
            lastPlayerPosition = player.position;
        }

        SetState(initialState, true);
        uiManager = UIManager.Instance();
        uiManager.Initialize(this);
        uiManager.PauseRequested += PauseGame;
        uiManager.ContinueRequested += ResumeGame;
        // TODO: Remove this before release
        string intro = "event 1";
        DialogueManager.Instance.AddLine(intro, "Line 1", 5f);
        DialogueManager.Instance.AddLine(intro, "Line 2", 4f);
        
        SoundManager.Instance.PlayMusic(SoundAsset.Instance.BackgroundMusic, true);
    }

    // TODO: Remove this before release
    [ContextMenu("Play Event 1")]
    public void PlayEvent1()
    {
        DialogueManager.Instance.Play("event 1");
        DialogueTime();
        
    }

    private void Update()
    {
        if (state == GameState.Playing && runTimer)
        {
            elapsedTime += Time.deltaTime;
            OnTimerChanged?.Invoke(elapsedTime);
        }

        if (player != null)
        {
            Vector3 pos = player.position;
            if ((pos - lastPlayerPosition).sqrMagnitude > positionEpsilon)
            {
                lastPlayerPosition = pos;
                OnPlayerPositionChanged?.Invoke(lastPlayerPosition);
            }
        }
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        if (player != null)
        {
            lastPlayerPosition = player.position;
            OnPlayerPositionChanged?.Invoke(lastPlayerPosition);
        }
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        OnTimerChanged?.Invoke(elapsedTime);
    }

    public void StartGame(bool resetTimer = true)
    {
        if (resetTimer)
        {
            ResetTimer();
        }

        SetState(GameState.Playing, false);
        UIManager.Instance().SetState(GameState.Playing);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseGame()
    {
        SetState(GameState.Paused, false);
    }

    public void ResumeGame()
    {
        SetState(GameState.Playing, false);
    }

    public void DialogueTime()
    {
        SetState(GameState.Dialogue, false);
    }

    public void EndGame()
    {
        SetState(GameState.Ended, false);
    }

    public string GetTimerString()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void SetState(GameState newState, bool force)
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
            UIManager.Instance().SetState(GameState.Ended);
        }
        else if (state == GameState.Dialogue && freezeTimeWhenDialogue)
        {
            CacheTimeScale();
            Time.timeScale = 0f;
        }
        else if (state == GameState.Playing)
        {
            UIManager.Instance().SetState(GameState.Playing);
            Time.timeScale = Mathf.Approximately(previousTimeScale, 0f) ? 1f : previousTimeScale;
        }

        OnStateChanged?.Invoke(state);
    }

    private void CacheTimeScale()
    {
        if (Time.timeScale > 0f)
        {
            previousTimeScale = Time.timeScale;
        }
    }

    private void OnDestroy()
    {
        if (uiManager != null)
        {
            uiManager.PauseRequested -= PauseGame;
            uiManager.ContinueRequested -= ResumeGame;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
