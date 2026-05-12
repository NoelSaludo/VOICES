using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerSfx : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private float walkingSfxInterval = 0.4f;
    [SerializeField] private float draggingSfxInterval = 0.8f;

    private Player player;
    private float nextWalkingSfxTime;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        if (player.State == PlayerState.MovingObject) walkingSfxInterval = 0.8f;

        if (player.MoveInput != 0f && player.IsGrounded() && Time.time >= nextWalkingSfxTime)
        {
            PlayWalkingSfx();
            PlayingPushingSFX();
            nextWalkingSfxTime = Time.time + walkingSfxInterval;
        }
    }

    private void PlayWalkingSfx()
    {
        if (SoundAsset.Instance == null || SoundAsset.Instance.Walking == null)
        {
            return;
        }

        if (playerAudioSource != null)
        {
            float volume = 1f;
            if (SoundManager.Instance != null)
            {
                volume = SoundManager.Instance.GetSFXVolume();
            }

            playerAudioSource.PlayOneShot(SoundAsset.Instance.Walking, volume);
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundAsset.Instance.Walking);
        }
    }

    private void PlayingPushingSFX()
    {
        if (player.IsGrounded() &&
            player.State == PlayerState.MovingObject &&
            Time.time >= draggingSfxInterval)
        {
            SoundManager.Instance.PlaySFX(SoundAsset.Instance.Dragging);
        }
    }
}
