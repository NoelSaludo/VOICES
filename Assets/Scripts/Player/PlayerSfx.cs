using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerSfx : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private float walkingSfxInterval = 0.4f;
    [SerializeField] private float draggingSfxInterval = 0.8f;

    private Player player;
    private PlayerSfxLogic sfxLogic;

    private void Awake()
    {
        player = GetComponent<Player>();
        sfxLogic = new PlayerSfxLogic();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        if (player.State == PlayerState.MovingObject) walkingSfxInterval = 0.8f;
        else walkingSfxInterval = 0.4f;

        if (!sfxLogic.TryScheduleWalking(player, walkingSfxInterval))
        {
            return;
        }

        PlayWalkingSfx();
        PlayDraggingSfx();
    }

    private void PlayWalkingSfx()
    {
        if (SoundAsset.Instance == null || SoundAsset.Instance.Walking == null)
        {
            return;
        }

        SfxEmitter.PlayWithLocalFallback(playerAudioSource, SoundAsset.Instance.Walking);
    }

    private void PlayDraggingSfx()
    {
        if (!sfxLogic.ShouldPlayDragging(player, draggingSfxInterval))
        {
            return;
        }

        SfxEmitter.PlayViaManager(SoundAsset.Instance.Dragging);
    }
}
