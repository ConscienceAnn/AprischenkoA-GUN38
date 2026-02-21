using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MeleeCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 1f; // Уменьшил до 1
    public float attackCooldown = 1f;
    public float detectionRange = 15f;
    public Transform attackPoint;
    public LayerMask targetLayer;

    [Header("Targeting")]
    public bool prioritizeClosest = true;
    public bool requireLineOfSight = true;
    public bool debugMode = true;

    private float nextAttackTime = 0f;
    private Animator animator;
    private AiAgent aiAgent;
    private MeleeWeapon weapon;
    private AiSensor sensor;

    private GameObject currentTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
        aiAgent = GetComponent<AiAgent>();
        sensor = GetComponent<AiSensor>();

        // Не ищем оружие здесь - оно появится позже
        if (attackPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: AttackPoint not assigned");
        }
    }

    void Update()
    {
        if (aiAgent == null || aiAgent.targeting == null) return;

        if (aiAgent.targeting.HasTarget)
        {
            currentTarget = aiAgent.targeting.Target.gameObject;

            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (debugMode)
            {
                Debug.Log($"{gameObject.name}: Distance to target: {distance:F2}, AttackRange: {attackRange}, Can attack: {distance <= attackRange && Time.time >= nextAttackTime}");
            }

            if (distance <= attackRange && Time.time >= nextAttackTime)
            {
                Attack();
            }
        }
        else
        {
            currentTarget = null;
        }
    }

    void Attack()
    {
        if (animator != null)
        {
            Debug.Log($"{gameObject.name}: Setting Attack trigger");
            animator.SetTrigger("Attack");
            nextAttackTime = Time.time + attackCooldown;
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Animator is NULL!");
        }
    }

    // Вызывается из анимации
    public void DealDamage()
    {
        Debug.Log($"{gameObject.name}: DealDamage CALLED!");

        // ИЩЕМ ОРУЖИЕ КАЖДЫЙ РАЗ!
        weapon = GetComponentInChildren<MeleeWeapon>();

        if (weapon != null && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            Debug.Log($"{gameObject.name}: DealDamage - distance: {distance:F2}, attackRange: {attackRange}");

            if (distance <= attackRange)
            {
                weapon.Attack();
                Debug.Log($"{gameObject.name}: Dealt damage to {currentTarget.name}");
            }
            else
            {
                Debug.Log($"{gameObject.name}: Target too far for damage");
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: DealDamage - weapon null: {weapon == null}, target null: {currentTarget == null}");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(attackPoint.position, 0.1f);
        }
    }
}