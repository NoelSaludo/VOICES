using UnityEngine;

public class SideScrollerCamera : MonoBehaviour
{
    public Transform player;

    // Map limits (adjust later)
    public float minX = -10f;
    public float maxX = 10f;

    // Fixed Y position (you control this manually)
    public float fixedY = 0f;

    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (player == null) return;

        float targetX = Mathf.Clamp(player.position.x, minX, maxX);

        Vector3 targetPosition = new Vector3(
            targetX,
            fixedY,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}