using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Crate : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private float attachPadding = 0.05f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Player attachedPlayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void Interact(Player player)
    {
        if (player == null)
        {
            return;
        }

        if (attachedPlayer == player)
        {
            Detach();
            return;
        }

        if (attachedPlayer == null)
        {
            Attach(player);
        }
    }

    private void Attach(Player player)
    {
        attachedPlayer = player;
        SetPhysicsState(true);
        Vector2 attachPosition = GetAttachPosition(player);
        attachedPlayer.AttachToCrate(this, attachPosition);
    }

    private void Detach()
    {
        if (attachedPlayer == null)
        {
            return;
        }

        attachedPlayer.DetachFromCrate(this);
        attachedPlayer = null;
        SetPhysicsState(false);
    }

    private Vector2 GetAttachPosition(Player player)
    {
        Bounds crateBounds = col != null ? col.bounds : new Bounds(transform.position, Vector3.one);
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        Bounds playerBounds = playerCollider != null ? playerCollider.bounds : new Bounds(player.transform.position, Vector3.one);

        float direction = player.transform.position.x < crateBounds.center.x ? -1f : 1f;
        float x = crateBounds.center.x + direction * (crateBounds.extents.x + playerBounds.extents.x + attachPadding);
        float y = player.transform.position.y;

        return new Vector2(x, y);
    }

    private void OnDisable()
    {
        if (attachedPlayer != null)
        {
            attachedPlayer.DetachFromCrate(this);
            attachedPlayer = null;
            SetPhysicsState(false);
        }
    }

    private void SetPhysicsState(bool isAttached)
    {
        if (col != null)
        {
            col.isTrigger = !isAttached;
        }

        if (rb != null)
        {
            rb.bodyType = isAttached ? RigidbodyType2D.Dynamic : RigidbodyType2D.Static;
        }
    }
}
