using UnityEngine;
using System.Collections.Generic;

public class AutoAim : MonoBehaviour
{
    [Header("Настройки")]
    public KeyCode toggleKey = KeyCode.F;        // Клавиша переключения
    public float maxRange = 50f;                  // Максимальная дистанция
    public float rotationSpeed = 360f;             // Скорость поворота персонажа
    public LayerMask enemyLayer;                    // Слой врагов
    public LayerMask obstacleLayer;                 // Слой препятствий

    [Header("Компоненты")]
    public Transform playerTransform;                // Корневой объект персонажа
    public ActiveWeapon activeWeapon;                 // Компонент оружия

    // Приватные переменные
    private bool autoAimEnabled = false;
    private List<GameObject> allEnemies = new List<GameObject>();
    private Transform currentTarget;
    private Camera mainCamera;

    public bool IsAutoAimEnabled => autoAimEnabled;

    void Start()
    {
        // Находим камеру
        mainCamera = Camera.main;

        // Если playerTransform не задан, используем этот объект
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        // Находим компонент ActiveWeapon на игроке
        if (activeWeapon == null)
        {
            activeWeapon = GetComponent<ActiveWeapon>();
        }

        // Находим всех врагов
        FindAllEnemies();

        Debug.Log("AutoAim инициализирован. Нажмите F для переключения режима.");
    }

    void Update()
    {
        // Переключение по клавише F
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleAutoAim();
        }

        // Обновляем список врагов (если появились новые)
        if (allEnemies.Count == 0)
        {
            FindAllEnemies();
        }

        // Работаем только если режим включен
        if (autoAimEnabled)
        {
            HandleAutoAim();
        }
    }

    void FindAllEnemies()
    {
        allEnemies.Clear();

        // Ищем по тегу "AI" или по компоненту
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Agent");
        allEnemies.AddRange(enemies);

        Debug.Log($"AutoAim: Найдено врагов: {allEnemies.Count}");
    }

    void ToggleAutoAim()
    {
        autoAimEnabled = !autoAimEnabled;

        if (autoAimEnabled)
        {
            Debug.Log("<color=green>Автоматическая стрельба: ВКЛЮЧЕНА</color>");
        }
        else
        {
            Debug.Log("<color=red>Автоматическая стрельба: ОТКЛЮЧЕНА</color>");

            // Выключаем стрельбу
            StopFiring();
            currentTarget = null;
        }
    }

    void HandleAutoAim()
    {
        // Проверяем, есть ли у игрока оружие
        if (activeWeapon == null || activeWeapon.GetActiveWeapon() == null)
        {
            // Нет оружия - ничего не делаем
            return;
        }

        // Находим ближайшего видимого врага
        currentTarget = FindClosestVisibleEnemy();

        if (currentTarget != null)
        {
            // Поворачиваем ВСЕГО ПЕРСОНАЖА к цели
            RotatePlayerToTarget();

            // Проверяем, смотрит ли персонаж на цель
            if (IsFacingTarget())
            {
                // Включаем стрельбу
                StartFiring();
            }
            else
            {
                // Выключаем стрельбу пока поворачиваемся
                StopFiring();
            }
        }
        else
        {
            // Нет цели - выключаем стрельбу
            StopFiring();
        }
    }

    void RotatePlayerToTarget()
    {
        if (currentTarget == null || playerTransform == null) return;

        Debug.Log($"AutoAim: Пытаюсь повернуть к {currentTarget.name}");

        Vector3 direction = (currentTarget.position - playerTransform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.RotateTowards(
                playerTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            Debug.Log($"AutoAim: Было: {playerTransform.rotation.eulerAngles.y}°, Стало: {newRotation.eulerAngles.y}°");

            playerTransform.rotation = newRotation;
        }
    }

    bool IsFacingTarget()
    {
        if (currentTarget == null || playerTransform == null) return false;

        // Направление от игрока к цели
        Vector3 directionToTarget = (currentTarget.position - playerTransform.position).normalized;
        directionToTarget.y = 0;

        // Направление, куда смотрит игрок
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0;

        // Вычисляем угол
        float angle = Vector3.Angle(playerForward, directionToTarget);

        // Считаем что смотрит на цель если угол меньше 10 градусов
        return angle < 10f;
    }

    Transform FindClosestVisibleEnemy()
    {
        Transform closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject enemy in allEnemies)
        {
            // Проверяем что враг существует и жив
            if (enemy == null) continue;

            // Проверяем здоровье врага
            AiHealth health = enemy.GetComponent<AiHealth>();
            if (health != null && health.IsDead()) continue;

            // Вычисляем дистанцию
            float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
            if (distance > maxRange) continue;

            // Проверяем видимость
            if (IsEnemyVisible(enemy.transform))
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
        }

        return closestEnemy;
    }

    bool IsEnemyVisible(Transform enemy)
    {
        // Направление от камеры к врагу
        Vector3 direction = (enemy.position - mainCamera.transform.position).normalized;
        float distance = Vector3.Distance(mainCamera.transform.position, enemy.position);

        // Бросаем луч от камеры
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, direction, out hit, distance, obstacleLayer))
        {
            // Если луч попал в препятствие, враг не виден
            return false;
        }

        return true;
    }

    void StartFiring()
    {
        if (activeWeapon == null) return;

        RaycastWeapon weapon = activeWeapon.GetActiveWeapon();
        if (weapon != null && !weapon.isFiring)
        {
            weapon.StartFiring();
            Debug.Log($"AutoAim: Начал стрельбу по {currentTarget?.name}");
        }
    }

    void StopFiring()
    {
        if (activeWeapon == null) return;

        RaycastWeapon weapon = activeWeapon.GetActiveWeapon();
        if (weapon != null && weapon.isFiring)
        {
            weapon.StopFiring();
        }
    }

    // Визуализация для отладки
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || playerTransform == null) return;

        // Рисуем радиус поиска
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerTransform.position, maxRange);

        // Рисуем линию к текущей цели
        if (currentTarget != null)
        {
            Gizmos.color = autoAimEnabled ? Color.green : Color.red;
            Gizmos.DrawLine(playerTransform.position, currentTarget.position);

            // Рисуем направление взгляда игрока
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(playerTransform.position, playerTransform.forward * 5f);
        }
    }
}