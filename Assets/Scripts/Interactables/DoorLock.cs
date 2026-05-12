using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorLock : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private string interactionVerb = "Unlock";
    [SerializeField] private Transform promptAnchor;
    [SerializeField] private string noKeyDialogue = "MissingKeyEvent";
    [SerializeField] private string keyId;
    [SerializeField] private bool consumeKey = true;
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorRenderer;

    private bool unlocked;

    public string InteractionVerb => interactionVerb;
    public Transform PromptAnchor => promptAnchor != null ? promptAnchor : transform;

    private void Awake()
    {
        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider2D>();
        }

        if (doorRenderer == null)
        {
            doorRenderer = GetComponent<SpriteRenderer>();
        }

    }
    private void Start()
    {
        DialogueManager.Instance.AddLine(noKeyDialogue, "I need a key", 2.5f);
    }

    public void Interact(Player player)
    {
        if (unlocked || player == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(keyId))
        {
            return;
        }

        PlayerKeyInventory inventory = player.GetComponent<PlayerKeyInventory>();
        if (inventory == null)
        {
            ShowNoKeyDialogue();
            return;
        }

        if (!inventory.HasAnyKey())
        {
            ShowNoKeyDialogue();
            return;
        }

        if (!inventory.HasKey(keyId))
        {
            ShowNoKeyDialogue();
            return;
        }

        if (consumeKey)
        {
            inventory.ConsumeKey(keyId);
        }

        Unlock();
    }

    [ContextMenu("Show No Key Dialogue")]
    private void ShowNoKeyDialogue()
    {
        DialogueManager.Instance.Play(noKeyDialogue);
        GameManager.Instance.DialogueTime();
    }

    private void Unlock()
    {
        unlocked = true;
        SoundManager.Instance.PlaySFX(SoundAsset.Instance.DoorUnlock);

        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        if (doorRenderer != null)
        {
            doorRenderer.enabled = false;
        }
    }
}
