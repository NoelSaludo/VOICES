using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DoorLock : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private string keyId;
    [SerializeField] private bool consumeKey = true;
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer doorRenderer;

    private bool unlocked;

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
            return;
        }

        if (!inventory.HasKey(keyId))
        {
            return;
        }

        if (consumeKey)
        {
            inventory.ConsumeKey(keyId);
        }

        Unlock();
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
