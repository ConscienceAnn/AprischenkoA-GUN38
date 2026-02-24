using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MeleeCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 1f;
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
    private AttachMeleeWeapon attachMeleeWeapon; // Добавляем ссылку

    private GameObject currentTarget;
    private bool isInitialized = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        aiAgent = GetComponent<AiAgent>();
        sensor = GetComponent<AiSensor>();
        attachMeleeWeapon = GetComponent<AttachMeleeWeapon>(); // Получаем компонент

        if (attackPoint == null && debugMode)
        {
            Debug.Log($"{gameObject.name}: AttackPoint not assigned (waiting for weapon attachment)");
        }
    }

    void Update()
    {
        // Ждем инициализации
        if (!isInitialized)
        {
            CheckInitialization();
            return;
        }

        // Если оружия еще нет, пытаемся найти его
        if (weapon == null)
        {
            FindWeapon();
        }

        if (aiAgent == null || aiAgent.targeting == null) return;

        if (aiAgent.targeting.HasTarget)
        {
            currentTarget = aiAgent.targeting.Target.gameObject;

            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (debugMode && Time.frameCount % 60 == 0) // Логируем реже
            {
                Debug.Log($"{gameObject.name}: Distance to target: {distance:F2}, AttackRange: {attackRange}, Can attack: {distance <= attackRange && Time.time >= nextAttackTime && weapon != null}");
            }

            // Проверяем наличие оружия перед атакой
            if (distance <= attackRange && Time.time >= nextAttackTime && weapon != null)
            {
                Attack();
            }
        }
        else
        {
            currentTarget = null;
        }
    }

    void CheckInitialization()
    {
        // Проверяем, все ли компоненты готовы
        if (animator != null && aiAgent != null)
        {
            isInitialized = true;
            if (debugMode)
            {
                Debug.Log($"{gameObject.name}: MeleeCombat initialized");
            }
        }
    }

    void FindWeapon()
    {
        // Ищем оружие только если еще не искали или потеряли ссылку
        if (weapon == null)
        {
            weapon = GetComponentInChildren<MeleeWeapon>();
            if (weapon != null && debugMode)
            {
                Debug.Log($"{gameObject.name}: MeleeWeapon found and cached!");
            }
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

        // Обновляем ссылку на оружие
        if (weapon == null)
        {
            weapon = GetComponentInChildren<MeleeWeapon>();
        }

        if (weapon != null && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (debugMode)
            {
                Debug.Log($"{gameObject.name}: DealDamage - distance: {distance:F2}, attackRange: {attackRange}");
            }

            if (distance <= attackRange)
            {
                weapon.Attack();
                Debug.Log($"{gameObject.name}: Dealt damage to {currentTarget.name}");
            }
            else
            {
                if (debugMode)
                {
                    Debug.Log($"{gameObject.name}: Target too far for damage");
                }
            }
        }
        else
        {
            if (weapon == null)
            {
                // Проверяем, есть ли вообще компонент AttachMeleeWeapon
                if (attachMeleeWeapon != null && !attachMeleeWeapon.HasWeapon())
                {
                    Debug.Log($"{gameObject.name}: Still waiting for weapon to be attached...");
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name}: DealDamage - weapon is null! Target exists: {currentTarget != null}");
                }
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: DealDamage - target is null!");
            }
        }
    }

    // Публичный метод для внешнего оповещения о появлении оружия
    public void OnWeaponAttached(MeleeWeapon newWeapon)
    {
        weapon = newWeapon;
        Debug.Log($"{gameObject.name}: Weapon attached and cached! Weapon component: {(weapon != null ? "OK" : "NULL")}");
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