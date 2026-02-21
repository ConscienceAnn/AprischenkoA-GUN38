using UnityEngine;

public class BombSpawner : MonoBehaviour
{
    [Header("Границы спавна (точки в углах)")]
    public Transform minPoint; // Нижний левый угол
    public Transform maxPoint; // Верхний правый угол
    public LayerMask buildingLayer;
    public LayerMask groundLayer; // Что считать полом

    [Header("Настройки бомб")]
    public GameObject bombPrefab;
    public int maxBombs = 5;
    public float spawnInterval = 3f;

    [Header("Точная настройка")]
    public float heightOffset = 0.02f; // Отступ от пола (можно менять в инспекторе)

    private int currentBombs = 0;
    private float nextSpawnTime;

    void Start()
    {
        // ПРОВЕРЯЕМ ЧТО ВСЕ НАСТРОЕНО
        if (minPoint == null || maxPoint == null)
        {
            Debug.LogError("minPoint или maxPoint не назначены в инспекторе!");
            return;
        }

        if (bombPrefab == null)
        {
            Debug.LogError("bombPrefab не назначен в инспекторе!");
            return;
        }

        Debug.Log($"Границы спавна: Min {minPoint.position}, Max {maxPoint.position}");
        Debug.Log($"Размер зоны: {maxPoint.position - minPoint.position}");

        // Первая бомба через 1 секунду
        nextSpawnTime = Time.time + 1f;
    }

    void Update()
    {
        // Проверяем можно ли спавнить
        if (Time.time >= nextSpawnTime && currentBombs < maxBombs)
        {
            TrySpawnBomb();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void TrySpawnBomb()
    {
        Debug.Log("Пытаемся заспавнить бомбу...");

        for (int attempts = 0; attempts < 30; attempts++)
        {
            // Случайная позиция ВЫСОКО над зоной
            Vector3 rayStart = new Vector3(
                Random.Range(minPoint.position.x, maxPoint.position.x),
                maxPoint.position.y + 10f,
                Random.Range(minPoint.position.z, maxPoint.position.z)
            );

            // Пускаем луч вниз чтобы найти пол
            RaycastHit hit;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, 30f))
            {
                // Добавляем отступ от пола
                Vector3 spawnPos = hit.point + Vector3.up * heightOffset;

                Debug.Log($"Попытка {attempts + 1}: найдена поверхность в {hit.point}, спавним на {spawnPos}");

                // Проверяем что место свободно (нет зданий)
                if (!Physics.CheckSphere(spawnPos, 1f, buildingLayer))
                {
                    SpawnBomb(spawnPos);
                    return;
                }
            }
        }

        Debug.LogWarning("Не удалось найти место для бомбы после 30 попыток");
    }

    void SpawnBomb(Vector3 position)
    {
        Debug.Log($"СПАВНИМ БОМБУ в позиции {position}!");

        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.Euler(90, Random.Range(0, 360), 0));
        currentBombs++;

        // Автоматически добавляем трекер если его нет
        if (bomb.GetComponent<BombTracker>() == null)
        {
            bomb.AddComponent<BombTracker>();
        }

        Debug.Log($"Теперь бомб на поле: {currentBombs}/{maxBombs}");
    }

    public void OnBombDestroyed()
    {
        currentBombs = Mathf.Max(0, currentBombs - 1);
        Debug.Log($"Бомба уничтожена. Осталось: {currentBombs}/{maxBombs}");
    }

    // Визуализация границ в редакторе
    void OnDrawGizmos()
    {
        if (minPoint != null && maxPoint != null)
        {
            // Рисуем полупрозрачный куб зоны спавна
            Vector3 center = (minPoint.position + maxPoint.position) / 2;
            Vector3 size = maxPoint.position - minPoint.position;

            Gizmos.color = new Color(1, 1, 0, 0.2f);
            Gizmos.DrawCube(center, size);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);

            // Рисуем точки углов
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(minPoint.position, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(maxPoint.position, 0.2f);
        }
    }
}