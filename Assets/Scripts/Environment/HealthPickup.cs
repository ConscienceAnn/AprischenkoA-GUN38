using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float amount = 50;

    private void OnTriggerEnter(Collider other)
    {
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            health.Heal(amount);

            // ÅÄÈÍÑÒÂÅÍÍÀß ÄÎÁÀÂËÅÍÍÀß ÑÒĞÎÊÀ
            GetComponent<PickupSound>()?.PlayPickupSound();

            Destroy(gameObject);
        }
    }
}