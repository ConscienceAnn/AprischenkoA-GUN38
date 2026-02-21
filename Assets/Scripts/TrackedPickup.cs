using UnityEngine;

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