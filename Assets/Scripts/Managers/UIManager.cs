using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance {get; private set;}

    [Header("Screens")]
    [SerializeField] private GameObject playingScreen;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject gameOverScreen;

    [Header("Playing Screen")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Button pauseButton;

    [Header("Pause Screen")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button exitButton;

    [Header("Dialogue")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private bool hideDialogueWhenEmpty = true;

    [Header("Interaction Prompt")]
    [SerializeField] private GameObject interactionPromptRoot;
    [SerializeField] private TMP_Text interactionPromptText;
    [SerializeField] private Vector3 interactionPromptOffset = new Vector3(0f, 1.2f, 0f);

    public event Action PauseRequested;
    public event Action ContinueRequested;
    public event Action ExitRequested;

    private GameManager boundManager;
    private Transform currentPromptAnchor;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        RebindUIReferences();

    }

    private void LateUpdate()
    {
        if (interactionPromptRoot == null || currentPromptAnchor == null)
        {
            return;
        }

        interactionPromptRoot.transform.position = currentPromptAnchor.position + interactionPromptOffset;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        BindButtonEvents();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindButtonEvents();
        Unbind();
    }

    public void Initialize(GameManager manager)
    {
        if (manager == null)
        {
            return;
        }

        if (boundManager == manager)
        {
            return;
        }

        Unbind();
        boundManager = manager;
        boundManager.OnStateChanged += SetState;
        boundManager.OnTimerChanged += SetTimer;

        SetState(boundManager.State);
        SetTimer(boundManager.ElapsedTime);
    }

    public void Unbind()
    {
        if (boundManager == null)
        {
            return;
        }

        boundManager.OnStateChanged -= SetState;
        boundManager.OnTimerChanged -= SetTimer;
        boundManager = null;
    }

    public void SetState(GameState state)
    {
        bool isPaused = state == GameState.Paused;
        bool isPlaying = state == GameState.Playing;
        bool isEnded = state == GameState.Ended;

        if (playingScreen != null)
        {
            playingScreen.SetActive(isPlaying);
        }

        if (pauseScreen != null)
        {
            pauseScreen.SetActive(isPaused);
        }

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(isEnded);
        }
    }

    public void SetTimer(float time)
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text = FormatTime(time);
    }

    public void SetTimerText(string text)
    {
        if (timerText == null)
        {
            return;
        }

        timerText.text = text ?? string.Empty;
    }

    public void ShowDialogue(string text)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (dialogueText != null)
        {
            dialogueText.text = text ?? string.Empty;
        }
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void ClearDialogue()
    {
        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }

        if (hideDialogueWhenEmpty && dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void ShowInteractionPrompt(string text, Transform anchor)
    {
        if (interactionPromptRoot != null)
        {
            interactionPromptRoot.SetActive(true);
        }

        if (interactionPromptText != null)
        {
            interactionPromptText.text = text ?? string.Empty;
        }

        currentPromptAnchor = anchor;

        if (interactionPromptRoot != null && currentPromptAnchor != null)
        {
            interactionPromptRoot.transform.position = currentPromptAnchor.position + interactionPromptOffset;
        }
    }

    public void HideInteractionPrompt()
    {
        if (interactionPromptRoot != null)
        {
            interactionPromptRoot.SetActive(false);
        }

        if (interactionPromptText != null)
        {
            interactionPromptText.text = string.Empty;
        }

        currentPromptAnchor = null;
    }

    public void PauseButtonPressed()
    {
        PauseRequested?.Invoke();
    }

    public void ContinueButtonPressed()
    {
        ContinueRequested?.Invoke();
    }

    public void ExitButtonPressed()
    {
        ExitRequested?.Invoke();
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindUIReferences();
        UnbindButtonEvents();
        BindButtonEvents();
        Initialize(GameManager.Instance);
        currentPromptAnchor = null;
        HideInteractionPrompt();
        ClearDialogue();
        if (boundManager != null)
        {
            SetState(boundManager.State);
            SetTimer(boundManager.ElapsedTime);
        }
    }

    private void RebindUIReferences()
    {
        var screenCanvas = GameObject.Find("Screen Space");
        if (screenCanvas != null)
        {
            var screenRoot = screenCanvas.transform;
            playingScreen = FindChild(screenRoot, "Playing Screen");
            pauseScreen = FindChild(screenRoot, "Pause Screen");
            dialoguePanel = FindChild(screenRoot, "Dialogue Panel");

            var timerObject = FindChild(screenRoot, "Timer");
            timerText = timerObject != null ? timerObject.GetComponent<TMP_Text>() : null;

            var pauseButtonObject = FindChild(screenRoot, "Playing Screen/Pause Btn");
            pauseButton = pauseButtonObject != null ? pauseButtonObject.GetComponent<Button>() : null;

            var dialogueTextObject = FindChild(screenRoot, "Dialogue Panel/Dialogue Text");
            dialogueText = dialogueTextObject != null ? dialogueTextObject.GetComponent<TMP_Text>() : null;

            var continueButtonObject = FindChild(screenRoot, "Pause Screen/Pause Panel/Continue Btn");
            continueButton = continueButtonObject != null ? continueButtonObject.GetComponent<Button>() : null;

            var exitButtonObject = FindChild(screenRoot, "Pause Screen/Pause Panel/Exit Btn");
            exitButton = exitButtonObject != null ? exitButtonObject.GetComponent<Button>() : null;
        }

        var worldCanvas = GameObject.Find("World Space Canvas");
        if (worldCanvas != null)
        {
            var worldRoot = worldCanvas.transform;
            interactionPromptRoot = FindChild(worldRoot, "Interaction Prompt");

            var promptTextObject = FindChild(worldRoot, "Interaction Prompt/Interaction Text");
            interactionPromptText = promptTextObject != null ? promptTextObject.GetComponent<TMP_Text>() : null;
        }
    }

    private static GameObject FindChild(Transform root, string path)
    {
        if (root == null)
        {
            return null;
        }

        var child = root.Find(path);
        return child != null ? child.gameObject : null;
    }

    private void BindButtonEvents()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseButtonPressed);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueButtonPressed);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitButtonPressed);
        }
    }

    private void UnbindButtonEvents()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(PauseButtonPressed);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(ContinueButtonPressed);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(ExitButtonPressed);
        }
    }

}