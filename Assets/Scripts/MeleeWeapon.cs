using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public float damage = 20f;
    public float attackRange = 2f;
    public float attackRate = 1f;
    public Transform attackPoint;
    public LayerMask targetLayer;

    private float nextAttackTime = 0f;
    private AiAgent aiAgent;

    void Start()
    {
        aiAgent = GetComponentInParent<AiAgent>();
    }

    public void Attack()
    {
        if (Time.time < nextAttackTime) return;

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange, targetLayer);

        foreach (var hit in hits)
        {
            // Пытаемся получить компонент Health (игрок)
            var playerHealth = hit.GetComponent<Health>();
            if (playerHealth != null)
            {
                // Вычисляем направление от оружия к цели
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(damage, direction);
                Debug.Log($"Попадание по игроку! Урон: {damage}, Направление: {direction}");

                // Добавить эффект попадания (можно потом)
                // Instantiate(hitEffect, hit.transform.position, Quaternion.identity);
            }

            // Если у врагов тоже есть здоровье (AiHealth)
            var aiHealth = hit.GetComponent<AiHealth>();
            if (aiHealth != null && aiHealth != GetComponentInParent<AiHealth>()) // Не нанести урон самому себе
            {
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                aiHealth.TakeDamage(damage, direction);
            }
        }

        nextAttackTime = Time.time + 1f / attackRate;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}