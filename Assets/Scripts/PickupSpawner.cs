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
    }

    [Header("Префабы пикапов")]
    public PickupConfig healthPickup = new PickupConfig { name = "Аптечка", maxCount = 3 };
    public PickupConfig ammoPickup = new PickupConfig { name = "Патроны", maxCount = 3 };

    [Header("Оружие")]
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

    [Header("Миникарта")]
    public StaticMinimap minimap;

    void Start()
    {
        if (worldBounds == null)
            worldBounds = FindObjectOfType<WorldBounds>();

        // Автоматически добавляем слой Building
        int buildingLayer = LayerMask.NameToLayer("Building");
        if (buildingLayer != -1)
            obstacleLayers |= (1 << buildingLayer);

        // Загружаем префабы
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
            }
        }

        StartCoroutine(SpawnLoop());
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

                // Назначаем тег и слой
                if (config.name.Contains("Аптечка") || config.prefab.name.Contains("Health"))
                {
                    newItem.tag = "Health";
                    newItem.layer = LayerMask.NameToLayer("Pickup");
                }
                else
                {
                    newItem.tag = "Ammo";
                    newItem.layer = LayerMask.NameToLayer("Pickup");
                }

                TrackedPickup tracker = newItem.AddComponent<TrackedPickup>();
                tracker.spawner = this;
                tracker.pickupConfig = config;
                tracker.weaponConfig = null;

                if (minimap != null)
                {
                    StaticMinimap.PickupType type;
                    if (config.name.Contains("Аптечка") || config.prefab.name.Contains("Health"))
                        type = StaticMinimap.PickupType.Health;
                    else
                        type = StaticMinimap.PickupType.Ammo;

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

                // ВАЖНО: Назначаем тег и слой
                newItem.tag = "Weapon";
                newItem.layer = LayerMask.NameToLayer("Pickup");

                TrackedPickup tracker = newItem.AddComponent<TrackedPickup>();
                tracker.spawner = this;
                tracker.pickupConfig = null;
                tracker.weaponConfig = config;

                if (minimap != null)
                    minimap.RegisterPickup(newItem, StaticMinimap.PickupType.Weapon);

                config.currentCount++;
                spawnedItems.Add(newItem);

                Debug.Log($"Spawned weapon {newItem.name} with tag: {newItem.tag}, layer: {LayerMask.LayerToName(newItem.layer)}");
                return;
            }
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        Vector3 spawnPos = worldBounds.RandomPosition();
        spawnPos.y = spawnHeight;

        // Проверка на препятствия на высоте спавна
        if (Physics.OverlapSphere(spawnPos, itemRadius, obstacleLayers).Length > 0)
            return Vector3.zero;

        // Проверка лучом сверху
        RaycastHit hit;
        if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out hit, 20f, obstacleLayers))
            return Vector3.zero;

        // Проверка пола
        if (!Physics.Raycast(spawnPos, Vector3.down, out hit, 5f, LayerMask.GetMask("Default")))
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

public class TrackedPickup : MonoBehaviour
{
    [HideInInspector] public PickupSpawner spawner;
    [HideInInspector] public PickupSpawner.PickupConfig pickupConfig;
    [HideInInspector] public PickupSpawner.WeaponConfig weaponConfig;

    void OnDestroy()
    {
        if (spawner != null)
        {
            if (spawner.minimap != null)
                spawner.minimap.UnregisterPickup(gameObject);

            if (pickupConfig != null)
                spawner.OnPickupCollected(pickupConfig);
            if (weaponConfig != null)
                spawner.OnWeaponCollected(weaponConfig);
        }
    }
}