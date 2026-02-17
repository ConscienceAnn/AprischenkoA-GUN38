using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Настройки мерцания")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float flickerSpeed = 10f;

    [Header("Случайные значения")]
    public bool useRandomSpeed = true;
    public float minSpeed = 5f;
    public float maxSpeed = 15f;

    private Light lightSource;
    private float baseIntensity;
    private float randomOffset;
    private float currentSpeed;

    void Start()
    {
        lightSource = GetComponent<Light>();
        if (lightSource == null)
        {
            Debug.LogError("LightFlicker требует компонент Light!");
            enabled = false;
            return;
        }

        baseIntensity = lightSource.intensity;
        randomOffset = Random.Range(0f, 100f);

        if (useRandomSpeed)
            currentSpeed = Random.Range(minSpeed, maxSpeed);
        else
            currentSpeed = flickerSpeed;
    }

    void Update()
    {
        // Используем Perlin Noise для плавного мерцания
        float noise = Mathf.PerlinNoise(Time.time * currentSpeed + randomOffset, 0f);
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        lightSource.intensity = baseIntensity * intensity;
    }
}