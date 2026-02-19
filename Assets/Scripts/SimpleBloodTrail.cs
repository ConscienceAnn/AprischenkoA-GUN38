using UnityEngine;

public class SimpleBloodTrail : MonoBehaviour
{
    public Sprite bloodIcon;

    [Header("Настройки")]
    public float healthThreshold = 0.4f;

    [Header("Рандомные интервалы")]
    public float minSpawnInterval = 0.3f;
    public float maxSpawnInterval = 1.2f;

    private AiHealth healthSystem;
    private float nextSpawnTime;
    private bool isBleeding = false;

    void Start()
    {
        healthSystem = GetComponent<AiHealth>();
    }

    void Update()
    {
        float healthPercent = (float)healthSystem.currentHealth / healthSystem.maxHealth;
        bool shouldBleed = healthPercent <= healthThreshold && healthPercent > 0;

        if (shouldBleed != isBleeding)
        {
            isBleeding = shouldBleed;
            if (isBleeding)
                nextSpawnTime = Time.time + Random.Range(0.1f, 0.5f);
        }

        if (isBleeding && Time.time >= nextSpawnTime)
        {
            SpawnBlood();
            nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    void SpawnBlood()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            GameObject blood = new GameObject("Blood");
            SpriteRenderer sr = blood.AddComponent<SpriteRenderer>();
            sr.sprite = bloodIcon;
            sr.color = Color.red;

            float size = Random.Range(0.02f, 0.1f);
            blood.transform.localScale = new Vector3(size, size, 1);

            // Небольшое случайное смещение
            Vector3 randomOffset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
            blood.transform.position = hit.point + Vector3.up * 0.01f + randomOffset;

            blood.transform.rotation = Quaternion.Euler(90, Random.Range(0, 360), 0);

            float lifetime = Random.Range(4f, 7f);
            Destroy(blood, lifetime);
        }
    }
}