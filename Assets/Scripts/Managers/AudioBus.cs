using UnityEngine;

public class AudioBus
{
    private readonly AudioSource source;

    public AudioBus(AudioSource source)
    {
        this.source = source;
    }

    public void ApplyVolume(float volume)
    {
        source.volume = volume;
    }

    public void PlayMusic(AudioClip clip, bool loop)
    {
        if (source.clip == clip)
        {
            return;
        }

        source.clip = clip;
        source.loop = loop;
        source.Play();
    }

    public void Stop()
    {
        source.Stop();
    }

    public void PlayOneShot(AudioClip clip, float volume)
    {
        source.PlayOneShot(clip, volume);
    }

    public void PlayAtPoint(AudioClip clip, Vector3 position, float volume)
    {
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}
