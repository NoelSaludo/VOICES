using UnityEngine;

public class PlayerInteractionHandler
{
    private readonly Player player;
    private readonly Rigidbody2D rb;
    private readonly GameObject owner;

    private IPlayerInteractable currentInteractable;
    private Crate attachedCrate;
    private FixedJoint2D moveJoint;
    private PlayerState state = PlayerState.Free;

    public PlayerInteractionHandler(Player player, Rigidbody2D rb, GameObject owner)
    {
        this.player = player;
        this.rb = rb;
        this.owner = owner;
    }

    public PlayerState State => state;
    public IPlayerInteractable CurrentInteractable => currentInteractable;
    public Crate AttachedCrate => attachedCrate;

    public event System.Action<IPlayerInteractable> InteractableChanged;

    public void HandleInteract()
    {
        if (state == PlayerState.MovingObject && attachedCrate != null)
        {
            attachedCrate.Interact(player);
            return;
        }

        if (currentInteractable != null)
        {
            currentInteractable.Interact(player);
        }
    }

    public bool AttachToCrate(Crate crate, Vector2 attachPosition)
    {
        if (crate == null || state == PlayerState.MovingObject)
        {
            return false;
        }

        attachedCrate = crate;
        state = PlayerState.MovingObject;
        rb.position = attachPosition;
        rb.linearVelocity = Vector2.zero;

        if (moveJoint != null)
        {
            Object.Destroy(moveJoint);
        }

        Rigidbody2D crateBody = crate.GetComponent<Rigidbody2D>();
        if (crateBody != null)
        {
            moveJoint = owner.AddComponent<FixedJoint2D>();
            moveJoint.connectedBody = crateBody;
            moveJoint.autoConfigureConnectedAnchor = true;
            moveJoint.enableCollision = false;
        }

        return true;
    }

    public void DetachFromCrate(Crate crate)
    {
        if (attachedCrate != crate)
        {
            return;
        }

        attachedCrate = null;
        state = PlayerState.Free;

        if (moveJoint != null)
        {
            Object.Destroy(moveJoint);
            moveJoint = null;
        }
    }

    public void SetInteractable(Collider2D other)
    {
        IPlayerInteractable interactable = other.GetComponent<IPlayerInteractable>();
        if (interactable == null || interactable == currentInteractable)
        {
            return;
        }

        currentInteractable = interactable;
        InteractableChanged?.Invoke(currentInteractable);
    }

    public void ClearInteractable(Collider2D other)
    {
        IPlayerInteractable interactable = other.GetComponent<IPlayerInteractable>();
        if (interactable == null || interactable != currentInteractable)
        {
            return;
        }

        currentInteractable = null;
        InteractableChanged?.Invoke(null);
    }
}
