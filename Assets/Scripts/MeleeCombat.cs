using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MeleeCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float detectionRange = 10f; // Дальность обнаружения
    public Transform attackPoint;
    public LayerMask targetLayer;

    [Header("Targeting")]
    public bool prioritizeClosest = true;
    public bool requireLineOfSight = true;

    private float nextAttackTime = 0f;
    private Animator animator;
    private AiAgent aiAgent;
    private MeleeWeapon weapon;
    private AiSensor sensor;

    private GameObject currentTarget;
    private List<GameObject> targetsInRange = new List<GameObject>();

    void Start()
    {
        animator = GetComponent<Animator>();
        aiAgent = GetComponent<AiAgent>();
        weapon = GetComponentInChildren<MeleeWeapon>();
        sensor = GetComponent<AiSensor>();

        if (sensor == null)
        {
            Debug.LogWarning("AiSensor not found on " + gameObject.name);
        }
    }

    void Update()
    {
        // Обновляем список целей
        UpdateTargetList();

        // Выбираем лучшую цель
        SelectBestTarget();

        // Атакуем если есть цель
        if (currentTarget != null && Time.time >= nextAttackTime)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (distance <= attackRange)
            {
                Attack();
            }
        }
    }

    void UpdateTargetList()
    {
        targetsInRange.Clear();

        if (sensor == null) return;

        // Используем сенсор для поиска всех объектов на слое Player
        GameObject[] buffer = new GameObject[20];
        int count = sensor.Filter(buffer, "Player", "Player");

        for (int i = 0; i < count; i++)
        {
            GameObject target = buffer[i];
            float distance = Vector3.Distance(transform.position, target.transform.position);

            // Проверяем дистанцию
            if (distance <= detectionRange)
            {
                // Проверяем прямую видимость (опционально)
                if (requireLineOfSight)
                {
                    if (HasLineOfSight(target))
                    {
                        targetsInRange.Add(target);
                    }
                }
                else
                {
                    targetsInRange.Add(target);
                }
            }
        }
    }

    bool HasLineOfSight(GameObject target)
    {
        if (sensor != null)
        {
            // Используем встроенный метод сенсора
            return sensor.IsInSight(target);
        }
        else
        {
            // Простая проверка лучом
            RaycastHit hit;
            Vector3 direction = (target.transform.position - transform.position).normalized;
            Vector3 origin = transform.position + Vector3.up * 1.5f; // На уровне глаз

            if (Physics.Raycast(origin, direction, out hit, detectionRange))
            {
                return hit.transform == target.transform;
            }
            return false;
        }
    }

    void SelectBestTarget()
    {
        if (targetsInRange.Count == 0)
        {
            currentTarget = null;
            return;
        }

        if (prioritizeClosest)
        {
            // Выбираем ближайшего
            currentTarget = targetsInRange
                .OrderBy(t => Vector3.Distance(transform.position, t.transform.position))
                .First();
        }
        else
        {
            // Просто берем первого
            currentTarget = targetsInRange[0];
        }
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        nextAttackTime = Time.time + attackCooldown;

        if (debugMode)
        {
            Debug.Log($"{gameObject.name} attacks {currentTarget.name}");
        }
    }

    public void DealDamage()
    {
        if (weapon != null && currentTarget != null)
        {
            // Проверяем, все еще ли цель в радиусе
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= attackRange)
            {
                weapon.Attack();

                if (debugMode)
                {
                    Debug.Log($"{gameObject.name} dealt damage to {currentTarget.name}");
                }
            }
        }
    }

    public bool debugMode = true;

    void OnDrawGizmosSelected()
    {
        // Радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Радиус обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Точка атаки
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(attackPoint.position, 0.1f);
        }

        // Текущая цель
        if (Application.isPlaying && currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);

            // Рисуем красную сферу вокруг цели если в радиусе атаки
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= attackRange)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(currentTarget.transform.position, 0.5f);
            }
        }

        // Все потенциальные цели
        if (Application.isPlaying && debugMode)
        {
            Gizmos.color = Color.cyan;
            foreach (var target in targetsInRange)
            {
                if (target != null && target != currentTarget)
                {
                    Gizmos.DrawLine(transform.position, target.transform.position);
                }
            }
        }
    }
}