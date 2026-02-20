using UnityEngine;

public class BombSpawner : MonoBehaviour
{
    [Header("Границы спавна")]
    public Transform worldBounds;
    public LayerMask buildingLayer;

    [Header("Настройки бомб")]
    public GameObject bombPrefab;
    public int maxBombs = 5;
    public float spawnInterval = 3f;
    public float spawnHeight = 0.5f;

    private int currentBombs = 0;
    private float nextSpawnTime;
    private Vector3 minBounds;
    private Vector3 maxBounds;

    void Start()
    {
        if (worldBounds != null)
        {
            Collider boundsCollider = worldBounds.GetComponent<Collider>();
            if (boundsCollider != null)
            {
                minBounds = boundsCollider.bounds.min;
                maxBounds = boundsCollider.bounds.max;
            }
        }

        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime && currentBombs < maxBombs)
        {
            TrySpawnBomb();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void TrySpawnBomb()
    {
        for (int attempts = 0; attempts < 30; attempts++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(minBounds.x, maxBounds.x),
                minBounds.y + spawnHeight,
                Random.Range(minBounds.z, maxBounds.z)
            );

            // Проверяем, что не спавним в здании
            if (!Physics.CheckSphere(spawnPos, 1f, buildingLayer))
            {
                SpawnBomb(spawnPos);
                return;
            }
        }
    }

    void SpawnBomb(Vector3 position)
    {
        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        currentBombs++;

        // Добавляем компонент для отслеживания уничтожения
        BombTracker tracker = bomb.AddComponent<BombTracker>();
        tracker.spawner = this;
    }

    public void OnBombDestroyed()
    {
        currentBombs = Mathf.Max(0, currentBombs - 1);
    }
}