using UnityEngine;

public class Stalker : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Spawn")]
    [SerializeField] private float minSpawnDistance = 1.5f;
    [SerializeField] private float maxSpawnDistance = 3f;

    [Header("Behavior")]
    [SerializeField] private float lingerDuration = 1.2f;

    private Rigidbody2D rb;
    private float timer;
    private bool hasTriggered;
    private bool initialized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        ResetState();
        ResolvePlayer();
    }

    private void Start()
    {
        TryPlaceNearPlayer();
    }

    private void ResetState()
    {
        timer = 0f;
        hasTriggered = false;
        initialized = false;
    }

    private void Update()
    {
        if (hasTriggered)
        {
            return;
        }

        if (!EnsureTarget())
        {
            return;
        }

        if (!initialized)
        {
            TryPlaceNearPlayer();
        }

        timer += Time.deltaTime;
        if (timer >= lingerDuration)
        {
            TriggerGameOver();
        }
    }

    private bool EnsureTarget()
    {
        if (player != null || GameManager.Instance != null)
        {
            return true;
        }

        if (!autoFindPlayer)
        {
            return false;
        }

        ResolvePlayer();
        return player != null || GameManager.Instance != null;
    }

    private void ResolvePlayer()
    {
        if (player != null || !autoFindPlayer)
        {
            return;
        }

        Player playerComponent = FindAnyObjectByType<Player>();
        if (playerComponent != null)
        {
            player = playerComponent.transform;
            return;
        }

        if (!string.IsNullOrEmpty(playerTag))
        {
            GameObject tagged = GameObject.FindGameObjectWithTag(playerTag);
            if (tagged != null)
            {
                player = tagged.transform;
            }
        }
    }

    private Vector3 GetTargetPosition()
    {
        if (player != null)
        {
            return player.position;
        }

        if (GameManager.Instance != null)
        {
            return GameManager.Instance.PlayerPosition;
        }

        return transform.position;
    }

    private void TryPlaceNearPlayer()
    {
        if (initialized || !EnsureTarget())
        {
            return;
        }

        float clampedMin = Mathf.Max(0f, minSpawnDistance);
        float clampedMax = Mathf.Max(clampedMin, maxSpawnDistance);
        float distance = Random.Range(clampedMin, clampedMax);

        float horizontalDirection = Random.value < 0.5f ? -1f : 1f;
        Vector3 targetPosition = GetTargetPosition();
        Vector3 spawnPosition = new Vector3(
            targetPosition.x + horizontalDirection * distance,
            targetPosition.y,
            targetPosition.z);

        if (rb != null)
        {
            rb.position = spawnPosition;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            transform.position = spawnPosition;
        }

        initialized = true;
    }

    private void TriggerGameOver()
    {
        if (hasTriggered)
        {
            return;
        }

        hasTriggered = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame();
        }
    }

}
