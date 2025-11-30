using UnityEngine;

public class PulsingAnim : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float pulseIncrease = 0.2f;
    public float pulseSpeed = 3f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float scale = Mathf.Sin(Time.time * pulseSpeed) * pulseIncrease;
        transform.localScale = originalScale * scale;
    }
}
