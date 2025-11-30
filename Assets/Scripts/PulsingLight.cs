using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PulsingLight : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private Light2D light2d;
    [SerializeField] float puslingStr = 3f;
    [SerializeField] float pulseIncrease = 1f;
    float originalIntensity;
    void Start()
    {
        light2d = GetComponent<Light2D>();
        originalIntensity = light2d.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        if (light2d == null) return;
        float amount = Mathf.Sin(Time.time * puslingStr) * pulseIncrease;
        light2d.intensity = originalIntensity * amount;
    }
}
