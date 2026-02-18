using UnityEngine;

public class SimpleBloodTrail : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject bloodDecalPrefab;  // Префаб с декалью крови
    public float healthThreshold = 0.4f;  // 40% здоровья
    public float spawnInterval = 0.5f;    // Интервал между следами
    public float decalLifetime = 5f;      // Время жизни следа

    private AiHealth healthSystem;     // ваша система здоровья
    private float nextSpawnTime;
    private bool isBleeding = false;

    void Start()
    {
        healthSystem = GetComponent<AiHealth>();
    }

    void Update()
    {
        // Проверяем здоровье только раз в кадр (это быстро)
        float healthPercent = (float)healthSystem.currentHealth / healthSystem.maxHealth;
        bool shouldBleed = healthPercent <= healthThreshold && healthPercent > 0;

        // Если состояние изменилось
        if (shouldBleed != isBleeding)
        {
            isBleeding = shouldBleed;
            if (isBleeding)
                nextSpawnTime = Time.time + spawnInterval; // Сбрасываем таймер
        }

        // Спавним кровь если нужно
        if (isBleeding && Time.time >= nextSpawnTime)
        {
            SpawnBlood();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnBlood()
    {
        // Простой Raycast вниз
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 2f))
        {
            // Спавним кровь
            GameObject blood = Instantiate(bloodDecalPrefab,
                hit.point + Vector3.up * 0.01f,
                Quaternion.Euler(0, Random.Range(0, 360), 0)); // Случайный поворот

            // Уничтожаем через время
            Destroy(blood, decalLifetime);
        }
    }
}