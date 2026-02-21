using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int clipAmount = 2;

    private void OnTriggerEnter(Collider other)
    {
        ActiveWeapon playerWeapon = other.GetComponent<ActiveWeapon>();
        if (playerWeapon)
        {
            playerWeapon.RefillAmmo(clipAmount);

            // едхмярбеммюъ днаюбкеммюъ ярпнйю
            GetComponent<PickupSound>()?.PlayPickupSound();

            Destroy(gameObject);
            return;
        }

        AiWeapons aiWeapons = other.GetComponent<AiWeapons>();
        if (aiWeapons && aiWeapons.IsLowAmmo())
        {
            aiWeapons.RefillAmmo(clipAmount);

            // едхмярбеммюъ днаюбкеммюъ ярпнйю
            GetComponent<PickupSound>()?.PlayPickupSound();

            Destroy(gameObject);
        }
    }
}