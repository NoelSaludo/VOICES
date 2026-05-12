using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DialogueLine
{
    public string Line;
    public float Timestamp;

    public DialogueLine(string line, float timestamp)
    {
        Line = line ?? string.Empty;
        Timestamp = Mathf.Max(0f, timestamp);
    }
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Playback")]
    [SerializeField] private bool clearDialogueAfterPlayback = true;
    [SerializeField] private bool useUnscaledTime = true;

    private readonly Dictionary<string, List<DialogueLine>> dialogueEvents = new Dictionary<string, List<DialogueLine>>();
    private Coroutine playbackRoutine;
    private UIManager cachedUiManager;

    public Dictionary<string, List<DialogueLine>> DialogueEvents => dialogueEvents;

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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddLine(string eventId, string line, float timestamp = 0f)
    {
        AddLine(eventId, new DialogueLine(line, timestamp));
    }

    public void AddLine(string eventId, DialogueLine line)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return;
        }

        if (!dialogueEvents.TryGetValue(eventId, out List<DialogueLine> lines))
        {
            lines = new List<DialogueLine>();
            dialogueEvents[eventId] = lines;
        }

        lines.Add(line);
    }

    public void SetLines(string eventId, List<DialogueLine> lines)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return;
        }

        dialogueEvents[eventId] = lines ?? new List<DialogueLine>();
    }

    public bool TryGetLines(string eventId, out List<DialogueLine> lines)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            lines = null;
            return false;
        }

        return dialogueEvents.TryGetValue(eventId, out lines);
    }

    public void RemoveEvent(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return;
        }

        dialogueEvents.Remove(eventId);
    }

    public void ClearAll()
    {
        dialogueEvents.Clear();
    }

    public void Play(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return;
        }

        Stop(false);
        playbackRoutine = StartCoroutine(PlayRoutine(eventId));
    }

    public void Stop(bool clearDialogue = true)
    {
        if (playbackRoutine != null)
        {
            StopCoroutine(playbackRoutine);
            playbackRoutine = null;
        }

        if (clearDialogue)
        {
            ClearDialogue();
        }
    }

    public void ShowDialogue(string text)
    {
        UIManager uiManager = GetUIManager();
        if (uiManager == null)
        {
            return;
        }

        uiManager.ShowDialogue(text);
    }

    public void HideDialogue()
    {
        UIManager uiManager = GetUIManager();
        if (uiManager == null)
        {
            return;
        }

        uiManager.HideDialogue();
    }

    public void ClearDialogue()
    {
        UIManager uiManager = GetUIManager();
        if (uiManager == null)
        {
            return;
        }

        uiManager.ClearDialogue();
    }

    private UIManager GetUIManager()
    {
        if (cachedUiManager != null)
        {
            return cachedUiManager;
        }

        cachedUiManager = GameObject.Find("UI Manager").GetComponent<UIManager>();
        return cachedUiManager;
    }

    private IEnumerator PlayRoutine(string eventId)
    {
        if (!dialogueEvents.TryGetValue(eventId, out List<DialogueLine> lines) || lines == null || lines.Count == 0)
        {
            if (clearDialogueAfterPlayback)
            {
                ClearDialogue();
            }

            ResumeGameIfDialogueActive();
            playbackRoutine = null;
            yield break;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            DialogueLine line = lines[i];
            ShowDialogue(line.Line);

            float duration = Mathf.Max(0f, line.Timestamp);
            if (duration > 0f)
            {
                if (useUnscaledTime)
                {
                    yield return new WaitForSecondsRealtime(duration);
                }
                else
                {
                    yield return new WaitForSeconds(duration);
                }
            }
            else
            {
                yield return null;
            }
        }

        playbackRoutine = null;

        if (clearDialogueAfterPlayback)
        {
            ClearDialogue();
        }

        ResumeGameIfDialogueActive();
    }

    private void ResumeGameIfDialogueActive()
    {
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Dialogue)
        {
            GameManager.Instance.ResumeGame();
        }
    }
}
