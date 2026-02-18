using UnityEngine;

public class SmoothDayNight : MonoBehaviour
{
    public float cycleDuration = 60f; // Длительность полного цикла в секундах
    public float maxIntensity = 1f;    // Максимальная яркость солнца (день)
    public float minIntensity = 0f;    // Минимальная яркость солнца (ночь)

    // Цвета для дня и ночи
    public Color daySkyColor = new Color(0.5f, 0.7f, 1f);    // Голубой
    public Color nightSkyColor = new Color(0.1f, 0.1f, 0.2f); // Темно-синий

    // Настройки для движения солнца
    [Header("Sun Movement")]
    public float sunriseAngle = -90f;   // Угол на восходе (восток)
    public float sunsetAngle = 90f;      // Угол на закате (запад)
    public float sunHeight = 60f;        // Максимальная высота солнца в зените

    // Дополнительные настройки для ночного освещения
    [Header("Night Settings")]
    public Color nightAmbientColor = new Color(0.05f, 0.05f, 0.1f); // Цвет окружающего света ночью
    public float nightAmbientIntensity = 0.1f; // Интенсивность окружающего света ночью

    private Light directionalLight;
    private float timeOfDay = 0f;

    void Start()
    {
        directionalLight = GetComponent<Light>();
        if (directionalLight == null)
        {
            directionalLight = FindObjectOfType<Light>();
        }
    }

    void Update()
    {
        // Увеличиваем время суток
        timeOfDay += Time.deltaTime / cycleDuration;

        if (timeOfDay > 1f)
            timeOfDay = 0f;

        // Вычисляем прогресс дня (0 = ночь, 0.5 = полдень, 1 = ночь)
        float dayProgress = Mathf.Sin(timeOfDay * Mathf.PI);

        // 1. ВРАЩАЕМ СОЛНЦЕ
        RotateSun(timeOfDay);

        // 2. Меняем интенсивность Directional Light
        directionalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, dayProgress);

        // 3. Меняем цвет неба
        Color skyColor = Color.Lerp(nightSkyColor, daySkyColor, dayProgress);
        RenderSettings.skybox.SetColor("_SkyTint", skyColor);

        // 4. Меняем окружающее освещение
        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, Color.white, dayProgress);
        RenderSettings.ambientIntensity = Mathf.Lerp(nightAmbientIntensity, 1f, dayProgress);
    }

    void RotateSun(float time)
    {
        // Конвертируем время (0-1) в угол поворота
        // 0 = полночь (солнце внизу)
        // 0.25 = восход (восток)
        // 0.5 = полдень (зенит)
        // 0.75 = закат (запад)
        // 1 = полночь

        float sunRotation;

        if (time < 0.25f) // Ночь -> Восход
        {
            // Солнце поднимается с востока
            float t = time / 0.25f; // 0-1
            sunRotation = Mathf.Lerp(0f, 45f, t);
        }
        else if (time < 0.5f) // Восход -> Полдень
        {
            // Солнце поднимается к зениту
            float t = (time - 0.25f) / 0.25f; // 0-1
            sunRotation = Mathf.Lerp(45f, 90f, t);
        }
        else if (time < 0.75f) // Полдень -> Закат
        {
            // Солнце опускается к западу
            float t = (time - 0.5f) / 0.25f; // 0-1
            sunRotation = Mathf.Lerp(90f, 135f, t);
        }
        else // Закат -> Ночь
        {
            // Солнце уходит за горизонт
            float t = (time - 0.75f) / 0.25f; // 0-1
            sunRotation = Mathf.Lerp(135f, 180f, t);
        }

        // Применяем вращение (вращаем по оси X для движения по небу)
        directionalLight.transform.rotation = Quaternion.Euler(sunRotation, -30f, 0f);
    }
}