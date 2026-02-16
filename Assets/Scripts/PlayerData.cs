using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float health;
    public int activeWeaponIndex;

    public int primaryAmmo;
    public int primaryClip;
    public int secondaryAmmo;
    public int secondaryClip;
    public bool hasPrimaryWeapon;
    public bool hasSecondaryWeapon;

    public string primaryWeaponName;
    public string secondaryWeaponName;

    public PlayerData(GameObject player)
    {
        Debug.Log("=== PLAYERDATA: НАЧАЛО СОХРАНЕНИЯ ===");

        // Сохраняем здоровье
        Health healthComp = player.GetComponent<Health>();
        if (healthComp != null)
        {
            health = healthComp.currentHealth;
            Debug.Log($"Сохранено здоровье: {health}");
        }

        // Сохраняем оружие
        ActiveWeapon activeWeapon = player.GetComponent<ActiveWeapon>();
        if (activeWeapon != null)
        {
            activeWeaponIndex = activeWeapon.activeWeaponIndex;
            Debug.Log($"Сохранен activeWeaponIndex: {activeWeaponIndex}");

            RaycastWeapon primary = activeWeapon.GetWeapon(0);
            RaycastWeapon secondary = activeWeapon.GetWeapon(1);

            hasPrimaryWeapon = primary != null;
            hasSecondaryWeapon = secondary != null;

            Debug.Log($"hasPrimaryWeapon: {hasPrimaryWeapon}, hasSecondaryWeapon: {hasSecondaryWeapon}");

            if (primary != null)
            {
                primaryWeaponName = primary.weaponName;
                primaryAmmo = primary.ammoCount;
                primaryClip = primary.clipCount;
                Debug.Log($"Сохранено PRIMARY оружие: {primaryWeaponName}, ammo: {primaryAmmo}, clip: {primaryClip}");
            }
            else
            {
                Debug.Log("PRIMARY оружие отсутствует");
            }

            if (secondary != null)
            {
                secondaryWeaponName = secondary.weaponName;
                secondaryAmmo = secondary.ammoCount;
                secondaryClip = secondary.clipCount;
                Debug.Log($"Сохранено SECONDARY оружие: {secondaryWeaponName}, ammo: {secondaryAmmo}, clip: {secondaryClip}");
            }
            else
            {
                Debug.Log("SECONDARY оружие отсутствует");
            }
        }
        else
        {
            Debug.LogError("ActiveWeapon компонент не найден!");
        }

        Debug.Log("=== PLAYERDATA: КОНЕЦ СОХРАНЕНИЯ ===");
    }

    public void ApplyToPlayer(GameObject player)
    {
        Debug.Log("=== PLAYERDATA: НАЧАЛО ЗАГРУЗКИ ===");
        Debug.Log($"Загружаемые данные: health={health}, activeWeaponIndex={activeWeaponIndex}");
        Debug.Log($"Primary: has={hasPrimaryWeapon}, name={primaryWeaponName}, ammo={primaryAmmo}, clip={primaryClip}");
        Debug.Log($"Secondary: has={hasSecondaryWeapon}, name={secondaryWeaponName}, ammo={secondaryAmmo}, clip={secondaryClip}");

        // Восстанавливаем здоровье
        Health healthComp = player.GetComponent<Health>();
        if (healthComp != null)
        {
            healthComp.currentHealth = health;
            UIHealthBar healthBar = player.GetComponentInChildren<UIHealthBar>();
            if (healthBar != null)
            {
                healthBar.SetHealthBarPercentage(health / healthComp.maxHealth);
            }
            Debug.Log($"Здоровье восстановлено: {health}");
        }

        // Восстанавливаем оружие
        ActiveWeapon activeWeapon = player.GetComponent<ActiveWeapon>();
        if (activeWeapon != null)
        {
            // Удаляем текущее оружие (если есть)
            if (activeWeapon.GetActiveWeapon() != null)
            {
                activeWeapon.DropWeapon();
                Debug.Log("Текущее оружие удалено");
            }

            // Загружаем PRIMARY оружие
            if (hasPrimaryWeapon && !string.IsNullOrEmpty(primaryWeaponName))
            {
                string weaponName = primaryWeaponName;

                // Добавляем префикс "Weapon_" если его нет
                if (!weaponName.StartsWith("Weapon_"))
                {
                    weaponName = "Weapon_" + weaponName;
                }

                Debug.Log($"Загрузка PRIMARY: Weapons/{weaponName}");
                GameObject primaryPrefab = Resources.Load<GameObject>($"Weapons/{weaponName}");

                if (primaryPrefab != null)
                {
                    GameObject weaponObj = Object.Instantiate(primaryPrefab);
                    RaycastWeapon weapon = weaponObj.GetComponent<RaycastWeapon>();
                    weapon.ammoCount = primaryAmmo;
                    weapon.clipCount = primaryClip;
                    activeWeapon.Equip(weapon);
                    Debug.Log($"PRIMARY оружие загружено: {weaponName}");
                }
                else
                {
                    Debug.LogError($"PRIMARY prefab не найден: Weapons/{weaponName}");
                    hasPrimaryWeapon = false;
                }
            }

            // Загружаем SECONDARY оружие
            if (hasSecondaryWeapon && !string.IsNullOrEmpty(secondaryWeaponName))
            {
                string weaponName = secondaryWeaponName;

                if (!weaponName.StartsWith("Weapon_"))
                {
                    weaponName = "Weapon_" + weaponName;
                }

                Debug.Log($"Загрузка SECONDARY: Weapons/{weaponName}");
                GameObject secondaryPrefab = Resources.Load<GameObject>($"Weapons/{weaponName}");

                if (secondaryPrefab != null)
                {
                    GameObject weaponObj = Object.Instantiate(secondaryPrefab);
                    RaycastWeapon weapon = weaponObj.GetComponent<RaycastWeapon>();
                    weapon.ammoCount = secondaryAmmo;
                    weapon.clipCount = secondaryClip;
                    activeWeapon.Equip(weapon);
                    Debug.Log($"SECONDARY оружие загружено: {weaponName}");
                }
                else
                {
                    Debug.LogError($"SECONDARY prefab не найден: Weapons/{weaponName}");
                    hasSecondaryWeapon = false;
                }
            }

            // Устанавливаем активное оружие
            Debug.Log($"Попытка установить активное оружие с индексом: {activeWeaponIndex}");
            Debug.Log($"hasPrimaryWeapon: {hasPrimaryWeapon}, hasSecondaryWeapon: {hasSecondaryWeapon}");

            if (activeWeaponIndex >= 0)
            {
                if ((activeWeaponIndex == 0 && hasPrimaryWeapon) ||
                    (activeWeaponIndex == 1 && hasSecondaryWeapon))
                {
                    activeWeapon.SetActiveWeaponIndex(activeWeaponIndex);
                    Debug.Log($"Активное оружие установлено на индекс: {activeWeaponIndex}");
                }
                else
                {
                    Debug.Log($"Индекс {activeWeaponIndex} недоступен, ищем альтернативу");

                    if (hasPrimaryWeapon)
                    {
                        activeWeapon.SetActiveWeaponIndex(0);
                        Debug.Log("Установлено PRIMARY как активное");
                    }
                    else if (hasSecondaryWeapon)
                    {
                        activeWeapon.SetActiveWeaponIndex(1);
                        Debug.Log("Установлено SECONDARY как активное");
                    }
                }
            }

            // Проверяем, что получилось
            RaycastWeapon activeNow = activeWeapon.GetActiveWeapon();
            Debug.Log($"Текущее активное оружие: {(activeNow != null ? activeNow.weaponName : "NULL")}");
        }
        else
        {
            Debug.LogError("ActiveWeapon компонент не найден при загрузке!");
        }

        Debug.Log("=== PLAYERDATA: КОНЕЦ ЗАГРУЗКИ ===");
    }
}