using System;
using TMPro;
using UnityEngine;
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
}