using UnityEngine;

public class SoundAsset : MonoBehaviour
{
    public static SoundAsset Instance { get; private set; }
    [Header("SFX")]
    public AudioClip walking;
    public AudioClip keyPickup;

    public AudioClip doorUnlock;

    public AudioClip landJump;


    [Header("Music")]
    public AudioClip BackgroundMusic;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = GameObject.Find("Sound Asset").GetComponent<SoundAsset>();
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
}
