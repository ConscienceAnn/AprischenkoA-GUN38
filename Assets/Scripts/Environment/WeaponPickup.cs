using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public RaycastWeapon weaponFab;

    private void OnTriggerEnter(Collider other)
    {
        ActiveWeapon activeWeapon = other.gameObject.GetComponent<ActiveWeapon>();
        if (activeWeapon)
        {
            RaycastWeapon newWeapon = Instantiate(weaponFab);
            activeWeapon.Equip(newWeapon);

            // ЕДИНСТВЕННАЯ ДОБАВЛЕННАЯ СТРОКА
            GetComponent<PickupSound>()?.PlayPickupSound();

            Destroy(gameObject);
            return;
        }

        AiWeapons aiWeapons = other.gameObject.GetComponent<AiWeapons>();
        if (aiWeapons)
        {
            RaycastWeapon newWeapon = Instantiate(weaponFab);
            aiWeapons.Equip(newWeapon);

            // ЕДИНСТВЕННАЯ ДОБАВЛЕННАЯ СТРОКА
            GetComponent<PickupSound>()?.PlayPickupSound();

            Destroy(gameObject);
        }
    }
}