using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    // Просто маркер для поиска точки спавна
    // Весь код перемещен в GameManager

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 1f);
    }
}