using UnityEngine;

public class EldritchMotion : MonoBehaviour
{
    public float driftAmount = 0.08f;
    public float driftSpeed = 0.4f;

    public float pulseAmount = 0.04f;
    public float pulseSpeed = 1.8f;

    private Vector3 startLocalPos;
    private Vector3 startScale;

    void Start()
    {
        startLocalPos = transform.localPosition;
        startScale = transform.localScale;
    }

    void Update()
    {
        // Creepy drifting
        float x = (Mathf.PerlinNoise(Time.time * driftSpeed, 0f) - 0.5f) * driftAmount;
        float y = (Mathf.PerlinNoise(0f, Time.time * driftSpeed) - 0.5f) * driftAmount;

        transform.localPosition = startLocalPos + new Vector3(x, y, 0f);

        // Flesh-like pulsing
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = startScale * pulse;
    }
}