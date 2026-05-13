using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Paused,
    Dialogue,
    Stalker,
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
    [SerializeField] private float startingCountdownSeconds = 10f;

    [Header("Stalker")]
    [SerializeField] private Stalker stalkerPrefab;
    [SerializeField] private bool findSceneStalkerIfPrefabMissing = true;

    [Header("Player Tracking")]
    [SerializeField] private Transform player;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float positionEpsilon = 0.0001f;

    private GameTimer timer;
    private PlayerTracker playerTracker;
    private GameStateController stateController;
    private UIManager uiManager;
    private Stalker spawnedStalker;
    private bool hasSpawnedStalker;

    public GameState State => stateController.State;
    public float RemainingTime => timer.RemainingTime;
    public Vector3 PlayerPosition => playerTracker.LastPosition;
    public bool IsPlaying => State == GameState.Playing;
    public bool IsPaused => State == GameState.Paused;
    public bool IsStalker => State == GameState.Stalker;
    public bool IsEnded => State == GameState.Ended;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        timer = new GameTimer();
        playerTracker = new PlayerTracker(playerTag, positionEpsilon);
        stateController = new GameStateController();
    }

    private void Start()
    {
        if (player != null)
        {
            playerTracker.SetPlayer(player);
        }
        else
        {
            playerTracker.TryAutoFind(autoFindPlayer);
            player = playerTracker.Player;
        }

        if (SoundAsset.Instance != null || SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMusic(SoundAsset.Instance.BackgroundMusic, true);
        }

        SetState(initialState, true);
        BindUIManager();
        // TODO: Remove this before release
        string intro = "event 1";
        DialogueManager.Instance.AddLine(intro, "Line 1", 5f);
        DialogueManager.Instance.AddLine(intro, "Line 2", 4f);
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
        if (State == GameState.Playing && runTimer)
        {
            timer.Tick(Time.deltaTime);
            OnTimerChanged?.Invoke(timer.RemainingTime);
            if (!hasSpawnedStalker && timer.IsComplete)
            {
                SpawnStalker();
            }
        }

        if (playerTracker.UpdatePosition())
        {
            OnPlayerPositionChanged?.Invoke(playerTracker.LastPosition);
        }
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        playerTracker.SetPlayer(playerTransform);
        if (player != null)
        {
            OnPlayerPositionChanged?.Invoke(playerTracker.LastPosition);
        }
    }

    public void RegisterPlayer(Player playerComponent)
    {
        if (playerComponent == null)
        {
            return;
        }

        SetPlayer(playerComponent.transform);
    }

    public void ResetTimer()
    {
        timer.Reset(startingCountdownSeconds);
        hasSpawnedStalker = false;
        OnTimerChanged?.Invoke(timer.RemainingTime);
    }

    public void StartGame(bool resetTimer = true)
    {
        if (resetTimer)
        {
            ResetTimer();
        }

        SetState(GameState.Playing, false);
        UIManager.Instance.SetState(GameState.Playing);
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
        int minutes = Mathf.FloorToInt(timer.RemainingTime / 60f);
        int seconds = Mathf.FloorToInt(timer.RemainingTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void SpawnStalker()
    {
        hasSpawnedStalker = true;
        SetState(GameState.Stalker, false);

        if (spawnedStalker != null)
        {
            if (!spawnedStalker.gameObject.activeSelf)
            {
                spawnedStalker.gameObject.SetActive(true);
            }
            return;
        }

        if (stalkerPrefab != null)
        {
            spawnedStalker = Instantiate(stalkerPrefab);
            return;
        }

        if (findSceneStalkerIfPrefabMissing)
        {
            spawnedStalker = FindAnyObjectByType<Stalker>(FindObjectsInactive.Include);
            if (spawnedStalker != null && !spawnedStalker.gameObject.activeSelf)
            {
                spawnedStalker.gameObject.SetActive(true);
            }
        }
    }

    private void SetState(GameState newState, bool force)
    {
        stateController.SetState(
            newState,
            force,
            freezeTimeWhenPaused,
            freezeTimeWhenEnded,
            freezeTimeWhenDialogue,
            OnStateChanged,
            state => UIManager.Instance.SetState(state));
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        ResolvePlayer();
        BindUIManager();
        if (runTimer)
        {
            ResetTimer();
        }
        SetState(initialState, true);

        if (SoundAsset.Instance != null || SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMusic(SoundAsset.Instance.BackgroundMusic, true);
        }

    }

    private void ResolvePlayer()
    {
        if (player != null)
        {
            return;
        }

        Player playerComponent = FindAnyObjectByType<Player>();
        if (playerComponent != null)
        {
            SetPlayer(playerComponent.transform);
            return;
        }

        playerTracker.TryAutoFind(autoFindPlayer);
        player = playerTracker.Player;
    }

    private void BindUIManager()
    {
        UIManager instance = UIManager.Instance;
        if (instance == null)
        {
            return;
        }

        if (uiManager == instance)
        {
            return;
        }

        UnbindUIManager();
        uiManager = instance;
        uiManager.Initialize(this);
        uiManager.PauseRequested += PauseGame;
        uiManager.ContinueRequested += ResumeGame;
    }

    private void UnbindUIManager()
    {
        if (uiManager == null)
        {
            return;
        }

        uiManager.PauseRequested -= PauseGame;
        uiManager.ContinueRequested -= ResumeGame;
        uiManager = null;
    }

    private void OnDestroy()
    {
        UnbindUIManager();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    internal void LoadScene(string nextSceneName)
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
