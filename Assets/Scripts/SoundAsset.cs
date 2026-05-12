using UnityEngine;

public class SoundAsset : MonoBehaviour
{
    public static SoundAsset Instance { get; private set; }
    [Header("SFX")]
    public AudioClip Walking;
    public AudioClip keyPickup;

    public AudioClip doorUnlock;

    public AudioClip landJump;
    public AudioClip Dragging;


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
