using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeyPickup : MonoBehaviour
{
    [SerializeField] private string keyId;
    [SerializeField] private bool destroyOnPickup = true;

    private Collider2D col;
    private bool pickedUp;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryPickup(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryPickup(collision.collider);
    }

    private void TryPickup(Collider2D other)
    {
        if (pickedUp)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(keyId))
        {
            return;
        }

        PlayerKeyInventory inventory = other.GetComponentInParent<PlayerKeyInventory>();
        if (inventory == null)
        {
            return;
        }

        inventory.AddKey(keyId);
        pickedUp = true;

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
