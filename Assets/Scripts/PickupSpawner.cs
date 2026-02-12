using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimplePickupSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PickupConfig
    {
        public string name;
        public GameObject prefab;
        public int maxCount = 3;
        public int currentCount = 0;
    }

    [System.Serializable]
    public class WeaponConfig
    {
        public string name;
        public GameObject prefab;
        public int maxCount = 2; // Максимум каждого вида оружия
        public int currentCount = 0;
    }

    [Header("Префабы пикапов")]
    public PickupConfig healthPickup = new PickupConfig { name = "Аптечка", maxCount = 3 };
    public PickupConfig ammoPickup = new PickupConfig { name = "Патроны", maxCount = 3 };

    [Header("Оружие (можно добавить несколько)")]
    public WeaponConfig[] weapons = new WeaponConfig[]
    {
        new WeaponConfig { name = "Пистолет", maxCount = 2 },
        new WeaponConfig { name = "Дробовик", maxCount = 2 },
        new WeaponConfig { name = "Винтовка", maxCount = 1 }
    };

    [Header("Границы мира")]
    public WorldBounds worldBounds;

    [Header("Препятствия")]
    public LayerMask obstacleLayers;
    public float itemRadius = 0.8f;

    [Header("Настройки")]
    public float checkInterval = 2f;
    public int maxAttempts = 100;
    public float spawnHeight = 2f;

    private List<GameObject> spawnedItems = new List<GameObject>();

    void Start()
    {
        // Находим WorldBounds
        if (worldBounds == null)
            worldBounds = FindObjectOfType<WorldBounds>();

        // Загружаем префабы
        if (healthPickup.prefab == null)
            healthPickup.prefab = Resources.Load<GameObject>("Pickups/HealthPickup");
        if (ammoPickup.prefab == null)
            ammoPickup.prefab = Resources.Load<GameObject>("Pickups/AmmoPickup");

        // Загружаем префабы оружия
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].prefab == null)
            {
                // Пытаемся загрузить по имени
                string path = "Pickups/Weapon_" + weapons[i].name;
                weapons[i].prefab = Resources.Load<GameObject>(path);

                // Если не нашли, пробуем без префикса
                if (weapons[i].prefab == null)
                    weapons[i].prefab = Resources.Load<GameObject>("Pickups/" + weapons[i].name);
            }
        }

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Спавним аптечки
            if (healthPickup.currentCount < healthPickup.maxCount)
                TrySpawnPickup(healthPickup);

            // Спавним патроны
            if (ammoPickup.currentCount < ammoPickup.maxCount)
                TrySpawnPickup(ammoPickup);

            // Спавним оружие (каждый вид по отдельности)
            foreach (var weapon in weapons)
            {
                if (weapon.currentCount < weapon.maxCount)
                    TrySpawnWeapon(weapon);
            }

            CleanupDestroyed();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void TrySpawnPickup(PickupConfig config)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 spawnPos = GetValidSpawnPosition();
            if (spawnPos != Vector3.zero)
            {
                GameObject newItem = Instantiate(config.prefab, spawnPos, Quaternion.identity);

                TrackedPickup tracker = newItem.AddComponent<TrackedPickup>();
                tracker.spawner = this;
                tracker.pickupConfig = config;
                tracker.weaponConfig = null;

                config.currentCount++;
                spawnedItems.Add(newItem);

                Debug.Log($"Заспавнен {config.name}");
                return;
            }
        }
    }

    void TrySpawnWeapon(WeaponConfig config)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 spawnPos = GetValidSpawnPosition();
            if (spawnPos != Vector3.zero)
            {
                GameObject newItem = Instantiate(config.prefab, spawnPos, Quaternion.identity);

                TrackedPickup tracker = newItem.AddComponent<TrackedPickup>();
                tracker.spawner = this;
                tracker.pickupConfig = null;
                tracker.weaponConfig = config;

                config.currentCount++;
                spawnedItems.Add(newItem);

                Debug.Log($"Заспавнен {config.name}");
                return;
            }
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPos = worldBounds.RandomPosition();
        spawnPos.y = spawnHeight;

        // Проверка на препятствия
        Collider[] obstacles = Physics.OverlapSphere(spawnPos, itemRadius, obstacleLayers);
        if (obstacles.Length > 0)
            return Vector3.zero;

        // Проверка на другие пикапы
        foreach (var item in spawnedItems)
        {
            if (item != null && Vector3.Distance(spawnPos, item.transform.position) < itemRadius * 2)
                return Vector3.zero;
        }

        return spawnPos;
    }

    void CleanupDestroyed()
    {
        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            if (spawnedItems[i] == null)
                spawnedItems.RemoveAt(i);
        }
    }

    public void OnPickupCollected(PickupConfig config)
    {
        if (config != null)
        {
            config.currentCount--;
            Debug.Log($"Подобран {config.name}. Осталось: {config.currentCount}/{config.maxCount}");
        }
    }

    public void OnWeaponCollected(WeaponConfig config)
    {
        if (config != null)
        {
            config.currentCount--;
            Debug.Log($"Подобрано оружие {config.name}. Осталось: {config.currentCount}/{config.maxCount}");
        }
    }

    // Визуализация
    private void OnDrawGizmosSelected()
    {
        if (worldBounds != null && worldBounds.min != null && worldBounds.max != null)
        {
            Gizmos.color = Color.green;
            Vector3 center = (worldBounds.min.position + worldBounds.max.position) / 2;
            Vector3 size = worldBounds.max.position - worldBounds.min.position;
            Gizmos.DrawWireCube(center, size);
        }
    }
}

// Обновленный компонент отслеживания
public class TrackedPickup : MonoBehaviour
{
    [HideInInspector] public SimplePickupSpawner spawner;
    [HideInInspector] public SimplePickupSpawner.PickupConfig pickupConfig;
    [HideInInspector] public SimplePickupSpawner.WeaponConfig weaponConfig;

    void OnDestroy()
    {
        if (spawner != null)
        {
            if (pickupConfig != null)
                spawner.OnPickupCollected(pickupConfig);
            if (weaponConfig != null)
                spawner.OnWeaponCollected(weaponConfig);
        }
    }
}