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
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float hitDistance = 0.2f;

    private Rigidbody2D rb;
    private float timer;
    private bool isDashing;
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
        TryPlaceNearPlayer();
    }

    private void ResetState()
    {
        timer = 0f;
        isDashing = false;
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

        if (!isDashing)
        {
            timer += Time.deltaTime;
            if (timer >= lingerDuration)
            {
                isDashing = true;
            }
            return;
        }

        if (rb == null)
        {
            DashTowardsTarget(Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (hasTriggered || !isDashing || rb == null)
        {
            return;
        }

        DashTowardsTarget(Time.fixedDeltaTime);
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

        Vector2 direction = Random.insideUnitCircle;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.right;
        }
        else
        {
            direction.Normalize();
        }

        Vector3 spawnPosition = GetTargetPosition() + new Vector3(direction.x, direction.y, 0f) * distance;

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

    private void DashTowardsTarget(float deltaTime)
    {
        Vector3 targetPosition = GetTargetPosition();
        Vector3 currentPosition = rb != null ? (Vector3)rb.position : transform.position;
        Vector3 nextPosition = Vector3.MoveTowards(currentPosition, targetPosition, dashSpeed * deltaTime);

        if (rb != null)
        {
            rb.MovePosition(nextPosition);
        }
        else
        {
            transform.position = nextPosition;
        }

        if (Vector3.Distance(nextPosition, targetPosition) <= hitDistance)
        {
            TriggerGameOver();
        }
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

    private bool IsPlayerCollider(Collider2D collider2d)
    {
        if (collider2d == null)
        {
            return false;
        }

        if (collider2d.GetComponent<Player>() != null)
        {
            return true;
        }

        return !string.IsNullOrEmpty(playerTag) && collider2d.CompareTag(playerTag);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDashing || hasTriggered)
        {
            return;
        }

        if (IsPlayerCollider(other))
        {
            TriggerGameOver();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDashing || hasTriggered)
        {
            return;
        }

        if (collision != null && IsPlayerCollider(collision.collider))
        {
            TriggerGameOver();
        }
    }
}
