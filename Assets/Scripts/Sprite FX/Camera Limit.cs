using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;

    [Header("Camera Limits")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        if (target == null) return;

        // Follow target
        Vector3 desiredPos = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        // Clamp camera inside borders
        desiredPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);
        desiredPos.y = Mathf.Clamp(desiredPos.y, minY, maxY);

        // Smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );
    }
}