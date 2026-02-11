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

    public PlayerData(GameObject player)
    {
        // Сохраняем здоровье
        Health healthComp = player.GetComponent<Health>();
        if (healthComp != null)
        {
            health = healthComp.currentHealth;
        }

        // Сохраняем оружие
        ActiveWeapon activeWeapon = player.GetComponent<ActiveWeapon>();
        if (activeWeapon != null)
        {
            activeWeaponIndex = activeWeapon.activeWeaponIndex;

            RaycastWeapon primary = activeWeapon.GetWeapon(0);
            RaycastWeapon secondary = activeWeapon.GetWeapon(1);

            hasPrimaryWeapon = primary != null;
            hasSecondaryWeapon = secondary != null;

            if (primary != null)
            {
                primaryAmmo = primary.ammoCount;
                primaryClip = primary.clipCount;
            }

            if (secondary != null)
            {
                secondaryAmmo = secondary.ammoCount;
                secondaryClip = secondary.clipCount;
            }
        }
    }

    public void ApplyToPlayer(GameObject player)
    {
        // Восстанавливаем здоровье
        Health healthComp = player.GetComponent<Health>();
        if (healthComp != null)
        {
            healthComp.currentHealth = health;
            // Обновляем UI здоровья
            UIHealthBar healthBar = player.GetComponentInChildren<UIHealthBar>();
            if (healthBar != null)
            {
                healthBar.SetHealthBarPercentage(health / healthComp.maxHealth);
            }
        }

        // Восстанавливаем оружие
        ActiveWeapon activeWeapon = player.GetComponent<ActiveWeapon>();
        if (activeWeapon != null)
        {
            // Удаляем текущее оружие
            activeWeapon.DropWeapon();

            // Создаем оружие заново (нужно настроить под ваш проект)
            // Здесь предполагается, что у вас есть префабы оружия в Resources
            if (hasPrimaryWeapon)
            {
                GameObject primaryPrefab = Resources.Load<GameObject>("Weapons/Pistol");
                if (primaryPrefab != null)
                {
                    RaycastWeapon weapon = Object.Instantiate(primaryPrefab).GetComponent<RaycastWeapon>();
                    weapon.ammoCount = primaryAmmo;
                    weapon.clipCount = primaryClip;
                    activeWeapon.Equip(weapon);
                }
            }

            if (hasSecondaryWeapon)
            {
                GameObject secondaryPrefab = Resources.Load<GameObject>("Weapons/Shotgun");
                if (secondaryPrefab != null)
                {
                    RaycastWeapon weapon = Object.Instantiate(secondaryPrefab).GetComponent<RaycastWeapon>();
                    weapon.ammoCount = secondaryAmmo;
                    weapon.clipCount = secondaryClip;
                    activeWeapon.Equip(weapon);
                }
            }

            // Устанавливаем активное оружие
            activeWeapon.SetActiveWeaponIndex(activeWeaponIndex);
        }
    }
}