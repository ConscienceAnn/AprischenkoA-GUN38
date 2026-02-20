using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Настройки")]
    public float damage = 50f;
    public float explosionRadius = 3f;
    public GameObject explosionEffect; // Префаб эффекта взрыва

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что наступил игрок
        if (other.CompareTag("Player"))
        {
            Explode();
        }
    }

    void Explode()
    {
        // Находим все объекты в радиусе взрыва
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider col in colliders)
        {
            // Проверяем, есть ли у объекта компонент Health (родительский класс)
            Health healthComponent = col.GetComponentInParent<Health>();

            if (healthComponent != null)
            {
                // Проверяем, является ли это игроком (опционально)
                PlayerHealth playerHealth = healthComponent as PlayerHealth;

                if (playerHealth != null)
                {
                    // Используем специальный метод для ловушки
                    playerHealth.TakeDamageFromTrap(damage);
                    Debug.Log($"Бомба нанесла {damage} урона игроку");
                }
                else
                {
                    // Для других объектов с Health (враги и т.д.)
                    Vector3 direction = (col.transform.position - transform.position).normalized;
                    healthComponent.TakeDamage(damage, direction);
                }
            }
        }

        // Эффект взрыва
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Уничтожаем бомбу
        Destroy(gameObject);
    }

    // Визуализация радиуса в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}