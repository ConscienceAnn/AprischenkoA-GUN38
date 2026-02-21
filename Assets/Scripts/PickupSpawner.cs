using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PickupSpawner : MonoBehaviour
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
        public int maxCount = 2;
        public int currentCount = 0;
        public bool isMelee = false;
    }

    [Header("Префабы пикапов")]
    public PickupConfig healthPickup = new PickupConfig { name = "Аптечка", maxCount = 3 };
    public PickupConfig ammoPickup = new PickupConfig { name = "Патроны", maxCount = 3 };

    [Header("Оружие")]
    public WeaponConfig[] weapons = new WeaponConfig[]
    {
        new WeaponConfig { name = "Пистолет", maxCount = 2, isMelee = false },
        new WeaponConfig { name = "Дробовик", maxCount = 2, isMelee = false },
        new WeaponConfig { name = "Винтовка", maxCount = 1, isMelee = false },
        new WeaponConfig { name = "Нож", maxCount = 2, isMelee = true }
    };

    [Header("Границы мира")]
    public WorldBounds worldBounds;

    [Header("Слои")]
    public LayerMask groundLayer = 1; // Слой пола
    public LayerMask buildingLayer; // ТОЛЬКО здания
    public LayerMask obstacleLayer; // Другие препятствия (камни, ящики)

    [Header("Настройки")]
    public float checkInterval = 0.5f;
    public int maxAttempts = 100;
    public float spawnHeight = 2f;
    public float itemRadius = 0.8f;
    public float minDistanceBetweenItems = 2f;

    private List<GameObject> spawnedItems = new List<GameObject>();

    [Header("Миникарта")]
    public StaticMinimap minimap;

    void Start()
    {
        if (worldBounds == null)
            worldBounds = FindObjectOfType<WorldBounds>();

        // Загружаем префабы
        LoadPrefabs();

        StartCoroutine(SpawnLoop());
    }

    void LoadPrefabs()
    {
        if (healthPickup.prefab == null)
            healthPickup.prefab = Resources.Load<GameObject>("Pickups/HealthPickup");
        if (ammoPickup.prefab == null)
            ammoPickup.prefab = Resources.Load<GameObject>("Pickups/AmmoPickup");

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].prefab == null)
            {
                string path = "Pickups/Weapon_" + weapons[i].name;
                weapons[i].prefab = Resources.Load<GameObject>(path);

                if (weapons[i].prefab == null)
                    weapons[i].prefab = Resources.Load<GameObject>("Pickups/" + weapons[i].name);

                if (weapons[i].isMelee && weapons[i].prefab == null)
                {
                    weapons[i].prefab = Resources.Load<GameObject>("Pickups/MeleeWeapon");
                }
            }
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (healthPickup.currentCount < healthPickup.maxCount)
                TrySpawnPickup(healthPickup);

            if (ammoPickup.currentCount < ammoPickup.maxCount)
                TrySpawnPickup(ammoPickup);

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

                if (config.name.Contains("Аптечка") || config.prefab.name.Contains("Health"))
                {
                    newItem.tag = "Health";
                }
                else
                {
                    newItem.tag = "Ammo";
                }
                newItem.layer = LayerMask.NameToLayer("Pickup");

                TrackedPickup tracker = newItem.AddComponent<TrackedPickup>();
                tracker.spawner = this;
                tracker.pickupConfig = config;
                tracker.weaponConfig = null;

                if (minimap != null)
                {
                    StaticMinimap.PickupType type = config.name.Contains("Аптечка") ?
                        StaticMinimap.PickupType.Health : StaticMinimap.PickupType.Ammo;
                    minimap.RegisterPickup(newItem, type);
                }

                config.currentCount++;
                spawnedItems.Add(newItem);
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

                newItem.tag = config.isMelee ? "MeleeWeapon" : "Weapon";
                newItem.layer = LayerMask.NameToLayer("Pickup");

                TrackedPickup tracker = newItem.AddComponent<TrackedPickup>();
                tracker.spawner = this;
                tracker.pickupConfig = null;
                tracker.weaponConfig = config;

                if (minimap != null)
                    minimap.RegisterPickup(newItem, StaticMinimap.PickupType.Weapon);

                config.currentCount++;
                spawnedItems.Add(newItem);

                Debug.Log($"Spawned weapon {newItem.name}");
                return;
            }
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPos = worldBounds.RandomPosition();
        spawnPos.y = spawnHeight;

        // 1. Проверка пола (ОБЯЗАТЕЛЬНО)
        RaycastHit groundHit;
        if (!Physics.Raycast(spawnPos, Vector3.down, out groundHit, 5f, groundLayer))
        {
            Debug.Log($"Нет пола в {spawnPos}");
            return Vector3.zero;
        }

        // 2. Проверка зданий (ТОЛЬКО здания)
        if (buildingLayer != 0 && Physics.CheckSphere(spawnPos, itemRadius, buildingLayer))
        {
            Debug.Log($"Мешает здание в {spawnPos}");
            return Vector3.zero;
        }

        // 3. Проверка других препятствий (камни, ящики)
        if (obstacleLayer != 0 && Physics.CheckSphere(spawnPos, itemRadius, obstacleLayer))
        {
            Debug.Log($"Мешает препятствие в {spawnPos}");
            return Vector3.zero;
        }

        // 4. Проверка расстояния до других пикапов
        foreach (var item in spawnedItems)
        {
            if (item != null)
            {
                float dist = Vector3.Distance(spawnPos, item.transform.position);
                if (dist < minDistanceBetweenItems)
                {
                    Debug.Log($"Слишком близко к пикапу {item.name}: {dist}");
                    return Vector3.zero;
                }
            }
        }

        // ВСЁ ХОРОШО - МОЖНО СПАВНИТЬ
        Debug.Log($"Найдено место: {spawnPos}");
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
            config.currentCount--;
    }

    public void OnWeaponCollected(WeaponConfig config)
    {
        if (config != null)
            config.currentCount--;
    }

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