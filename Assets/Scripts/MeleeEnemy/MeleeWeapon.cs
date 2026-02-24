using UnityEngine;
using System.Collections.Generic;

public class MeleeWeapon : MonoBehaviour
{
    public float damage = 20f;
    public float attackRange = 2f;
    public float attackRate = 1f;
    public Transform attackPoint;
    public LayerMask targetLayer;

    private float nextAttackTime = 0f;
    private AiAgent aiAgent;
    private GameObject owner; // Кто владеет оружием

    // Для защиты от множественных попаданий
    private List<Health> alreadyHit = new List<Health>();

    void Start()
    {
        aiAgent = GetComponentInParent<AiAgent>();
        owner = GetComponentInParent<AiAgent>()?.gameObject; // Владелец оружия

        if (owner == null)
        {
            owner = transform.root.gameObject; // Запасной вариант
        }
    }

    public void Attack()
    {
        if (Time.time < nextAttackTime) return;

        alreadyHit.Clear();

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange, targetLayer);
        Debug.Log($"{gameObject.name}: Found {hits.Length} hits");

        foreach (var hit in hits)
        {
            // Пропускаем по тегу "AI"
            if (hit.CompareTag("Agent")) continue;

            var playerHealth = hit.GetComponent<Health>();
            if (playerHealth != null && !alreadyHit.Contains(playerHealth))
            {
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                playerHealth.TakeDamage(damage, direction);
                alreadyHit.Add(playerHealth);
                Debug.Log($"{gameObject.name}: Hit player! Damage: {damage}");
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