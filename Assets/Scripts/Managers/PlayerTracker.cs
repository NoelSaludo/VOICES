using UnityEngine;

public class PlayerTracker
{
    private Transform player;
    private readonly string playerTag;
    private readonly float positionEpsilon;
    private Vector3 lastPlayerPosition;

    public PlayerTracker(string playerTag, float positionEpsilon)
    {
        this.playerTag = playerTag;
        this.positionEpsilon = positionEpsilon;
    }

    public Transform Player => player;
    public Vector3 LastPosition => lastPlayerPosition;

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
    }

    public bool TryAutoFind(bool autoFind)
    {
        if (!autoFind || player != null || string.IsNullOrEmpty(playerTag))
        {
            return false;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject == null)
        {
            return false;
        }

        player = playerObject.transform;
        lastPlayerPosition = player.position;
        return true;
    }

    public bool UpdatePosition()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 pos = player.position;
        if ((pos - lastPlayerPosition).sqrMagnitude > positionEpsilon)
        {
            lastPlayerPosition = pos;
            return true;
        }

        return false;
    }
}
