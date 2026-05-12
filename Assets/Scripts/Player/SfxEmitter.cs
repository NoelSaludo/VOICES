using UnityEngine;

public static class SfxEmitter
{
    public static void PlayWithLocalFallback(AudioSource localSource, AudioClip clip)
    {
        if (localSource != null)
        {
            float volume = 1f;
            if (SoundManager.Instance != null)
            {
                volume = SoundManager.Instance.GetSFXVolume();
            }

            localSource.PlayOneShot(clip, volume);
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(clip);
        }
    }

    public static void PlayViaManager(AudioClip clip)
    {
        SoundManager.Instance.PlaySFX(clip);
    }
}
