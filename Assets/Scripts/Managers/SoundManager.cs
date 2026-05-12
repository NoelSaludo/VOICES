using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private AudioBus musicBus;
    private AudioBus sfxBus;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
        musicSource = musicSource != null ? musicSource : GameObject.Find("Music Source").AddComponent<AudioSource>();
        sfxSource = sfxSource != null ? sfxSource : GameObject.Find("SFX Source").AddComponent<AudioSource>();

        musicBus = new AudioBus(musicSource);
        sfxBus = new AudioBus(sfxSource);
    }

    private void Start()
    {
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        musicBus.ApplyVolume(musicVolume);
        sfxBus.ApplyVolume(sfxVolume);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        musicBus.PlayMusic(clip, loop);
    }

    public void StopMusic()
    {
        musicBus.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxBus.PlayOneShot(clip, sfxVolume);
    }

    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        sfxBus.PlayAtPoint(clip, position, sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicBus.ApplyVolume(musicVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Awake();
    }
}
