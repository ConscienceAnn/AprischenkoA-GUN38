using UnityEngine;

public class RobotMoving : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.5f;

    [Header("Размер робота")]
    [SerializeField] private float robotSize = 1f;

    [Header("Уборка мусора")]
    [SerializeField] private LayerMask dirtLayer;
    [SerializeField] private float vacuumRadius = 0.8f;

    [Header("Лимит уборки")]
    [SerializeField] private int maxFailedLeftTurns = 7; // Максимум неудачных попыток налево подряд
    [SerializeField] private float returnToStartDistance = 0.3f; // На каком расстоянии считать что вернулись

    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private bool isStuck = false;
    private bool cleaningCompleted = false; // Уборка завершена
    private bool isReturningToStart = false; // Возвращаемся на старт

    // Для движения
    private Vector2 mainDirection = Vector2.up;
    private Vector2 sideDirection = Vector2.left;
    private bool isChangingLane = false;
    private float laneChangeProgress = 0f;

    // Для поворота налево
    private int consecutiveFailedLeftTurns = 0; // Подряд неудачных попыток налево
    private int leftTurnAttempts = 0;
    private const int maxLeftTurnAttempts = 3;

    // Для возврата
    private Vector2 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentDirection = Vector2.up;
        sideDirection = Vector2.left;
        startPosition = transform.position;

        Debug.Log($"Робот запущен. Максимум неудачных попыток налево: {maxFailedLeftTurns}");
    }

    void Update()
    {
        if (cleaningCompleted)
        {
            //// Полностью отключаем всё
            //rb.velocity = Vector2.zero;
            //rb.angularVelocity = 0f;
            return; 
        }

        if (isReturningToStart)
        {
            // Возвращаемся на старт
            ReturnToStart();
            return;
        }

        // Проверяем мусор
        CheckForDirt();

        // Проверяем препятствия
        if (!isChangingLane)
        {
            CheckForObstacles();
        }
        else
        {
            UpdateLaneChange();
        }

        // Проверяем лимит неудачных попыток
        if (consecutiveFailedLeftTurns >= maxFailedLeftTurns)
        {
            Debug.Log($"Достигнут лимит в {maxFailedLeftTurns} неудачных попыток налево подряд. Уборка завершена!");
            StartReturnToStart();
        }
    }

    void FixedUpdate()
    {
        if (cleaningCompleted)
        {
            //rb.velocity = Vector2.zero;
            //rb.angularVelocity = 0f;
            return;
        }

        if (!isStuck && !isReturningToStart)
        {
            Move();
        }
    }

    void Move()
    {
        Vector2 movement = currentDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        if (isChangingLane)
        {
            laneChangeProgress += movement.magnitude;
        }

        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;

        Debug.DrawRay(transform.position, currentDirection * 0.5f, Color.green);
    }

    void CheckForObstacles()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            currentDirection,
            stopDistance * 1.5f,
            LayerMask.GetMask("Room")
        );

        if (hit.collider != null)
        {
            Debug.DrawRay(transform.position, currentDirection * hit.distance, Color.red);

            if (hit.distance < stopDistance)
            {
                Debug.Log("Обнаружено препятствие на расстоянии: " + hit.distance);
                StartLaneChange();
            }
        }
        else
        {
            Debug.DrawRay(transform.position, currentDirection * stopDistance * 1.5f, Color.green);
        }
    }

    void StartLaneChange()
    {
        isStuck = true;

        // Пробуем повернуть НАЛЕВО (приоритетное направление)
        Vector2 leftDirection = sideDirection;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            leftDirection,
            robotSize * 1.2f,
            LayerMask.GetMask("Room")
        );

        if (hit.collider == null)
        {
            // УСПЕШНЫЙ поворот налево - сбрасываем счетчик неудач
            consecutiveFailedLeftTurns = 0;
            leftTurnAttempts = 0;
            StartLaneChangeWithDirection(leftDirection);
        }
        else
        {
            // НЕУДАЧНАЯ попытка налево - увеличиваем счетчик
            consecutiveFailedLeftTurns++;
            Debug.Log($"Неудачная попытка налево #{consecutiveFailedLeftTurns}/{maxFailedLeftTurns}");

            // Нельзя налево - пробуем стратегию
            Debug.Log("Нельзя повернуть налево, пробуем стратегию 'назад-налево'");
            TryBackAndLeftStrategy();
        }
    }

    void StartReturnToStart()
    {
        Debug.Log("Начинаем возврат на стартовую позицию...");
        isReturningToStart = true;

        // Разворачиваемся в направлении старта
        Vector2 directionToStart = (startPosition - (Vector2)transform.position).normalized;
        currentDirection = directionToStart;

        // Останавливаем смену полосы если она была
        isChangingLane = false;
        isStuck = false;

        Debug.Log("Двигаемся к старту: " + currentDirection);
    }

    void ReturnToStart()
    {
        // Двигаемся к стартовой позиции
        Vector2 directionToStart = (startPosition - (Vector2)transform.position).normalized;
        currentDirection = directionToStart;

        // Двигаемся
        Vector2 movement = currentDirection * moveSpeed * Time.fixedDeltaTime;

        // ОГРАНИЧИВАЕМ движение, чтобы не проскочить старт
        float distanceToStart = Vector2.Distance(transform.position, startPosition);
        if (distanceToStart <= returnToStartDistance)
        {
            // Вернулись на старт!
            cleaningCompleted = true;
            isReturningToStart = false;
            Debug.Log("Вернулись на старт! Уборка завершена.");

            // Останавливаем робота
            currentDirection = Vector2.zero;

            // НЕ делаем rb.bodyType = RigidbodyType2D.Static;
            // Просто останавливаем
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.position = startPosition;
        }

        rb.MovePosition(rb.position + movement);

        // Поворачиваем
        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;

        // Проверяем препятствия на пути к старту
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            currentDirection,
            stopDistance,
            LayerMask.GetMask("Room")
        );

        if (hit.collider != null && hit.distance < stopDistance)
        {
            // Объезжаем препятствие на пути к старту
            AvoidObstacleOnReturn();
        }

        // Проверяем, достигли ли старта
        if (distanceToStart <= returnToStartDistance)
        {
            // Вернулись на старт!
            CompleteCleaning();
        }

        // Визуализация
        Debug.DrawRay(transform.position, currentDirection * 0.5f, Color.magenta);
        Debug.DrawLine(transform.position, startPosition, Color.yellow);
    }

    void CompleteCleaning()
    {
        Debug.Log("Вернулись на старт! Уборка завершена.");

        // ТОЧНО устанавливаем позицию на старте
        transform.position = startPosition;

        // Устанавливаем флаги
        cleaningCompleted = true;
        isReturningToStart = false;

        // Останавливаем ВСЁ
        currentDirection = Vector2.zero;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isStuck = false;
        isChangingLane = false;

        // Отключаем Rigidbody
        rb.bodyType = RigidbodyType2D.Static;

        // ДОПОЛНИТЕЛЬНО: отключаем коллайдер если нужно
        // GetComponent<Collider2D>().enabled = false;

        Debug.Log($"Финальная позиция: {transform.position}");
    }


    void AvoidObstacleOnReturn()
    {
        // Простой алгоритм объезда при возврате
        // Пробуем разные направления

        Vector2[] testDirections = {
            RotateVector(currentDirection, 90f),   // Налево
            RotateVector(currentDirection, -90f),  // Направо
            RotateVector(currentDirection, 45f),   // Диагональ налево
            RotateVector(currentDirection, -45f)   // Диагональ направо
        };

        foreach (Vector2 dir in testDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                dir,
                robotSize * 1.2f,
                LayerMask.GetMask("Room")
            );

            if (hit.collider == null)
            {
                currentDirection = dir;
                Debug.Log("Объезжаем препятствие, новое направление: " + dir);
                return;
            }
        }

        // Если все направления закрыты - идем назад
        currentDirection = -currentDirection;
        Debug.Log("Все направления закрыты, идем назад");
    }

    void TryBackAndLeftStrategy()
    {
        leftTurnAttempts++;

        if (leftTurnAttempts >= maxLeftTurnAttempts)
        {
            // Слишком много попыток - пробуем направо
            Debug.Log("Слишком много неудачных попыток налево, пробуем направо");
            TryRightTurn();
            return;
        }

        // Шаг 1: Немного отъезжаем назад
        Vector2 backDirection = -currentDirection;

        RaycastHit2D hitBack = Physics2D.Raycast(
            transform.position,
            backDirection,
            robotSize * 0.5f,
            LayerMask.GetMask("Room")
        );

        if (hitBack.collider != null)
        {
            // Нельзя отъехать назад - пробуем направо
            Debug.Log("Нельзя отъехать назад, пробуем направо");
            TryRightTurn();
            return;
        }

        // Отъезжаем назад
        Debug.Log("Отъезжаем назад для разворота");
        currentDirection = backDirection;
        isStuck = false;

        // Ждем один кадр, затем проверяем снова
        StartCoroutine(TryLeftAfterBacking());
    }

    System.Collections.IEnumerator TryLeftAfterBacking()
    {
        // Ждем пока отъедем назад
        yield return new WaitForSeconds(0.3f);

        // Снова пробуем налево
        Vector2 leftDirection = sideDirection;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            leftDirection,
            robotSize * 1.2f,
            LayerMask.GetMask("Room")
        );

        if (hit.collider == null)
        {
            // Теперь можно налево! Сбрасываем счетчик неудач
            consecutiveFailedLeftTurns = 0;
            StartLaneChangeWithDirection(leftDirection);
        }
        else
        {
            // Все равно нельзя - увеличиваем счетчик неудач
            consecutiveFailedLeftTurns++;
            Debug.Log($"После отъезда все равно нельзя налево. Неудачи: {consecutiveFailedLeftTurns}/{maxFailedLeftTurns}");
            Debug.Log("Пробуем направо");
            TryRightTurn();
        }
    }

    void TryRightTurn()
    {
        Vector2 rightDirection = -sideDirection;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            rightDirection,
            robotSize * 1.2f,
            LayerMask.GetMask("Room")
        );

        if (hit.collider == null)
        {
            // Можно направо
            StartLaneChangeWithDirection(rightDirection);
        }
        else
        {
            // Все стороны закрыты - разворачиваемся
            Debug.Log("Все стороны закрыты, разворачиваемся");
            mainDirection = -mainDirection;
            currentDirection = mainDirection;
            isStuck = false;
            leftTurnAttempts = 0;

            // При развороте тоже считаем как неудачу налево
            consecutiveFailedLeftTurns++;
            Debug.Log($"Разворот. Неудачи налево: {consecutiveFailedLeftTurns}/{maxFailedLeftTurns}");
        }
    }

    void StartLaneChangeWithDirection(Vector2 direction)
    {
        currentDirection = direction;
        isChangingLane = true;
        laneChangeProgress = 0f;
        isStuck = false;

        Debug.Log("Начинаем смену полосы. Движемся: " + currentDirection);
    }

    void UpdateLaneChange()
    {
        if (laneChangeProgress >= robotSize * 0.9f)
        {
            CompleteLaneChange();
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            currentDirection,
            stopDistance,
            LayerMask.GetMask("Room")
        );

        if (hit.collider != null && hit.distance < stopDistance)
        {
            Debug.Log("Препятствие при смене полосы!");

            if (currentDirection == sideDirection)
            {
                // Пытались налево - пробуем направо
                TryRightTurn();
            }
            else
            {
                // Пытались направо - пробуем отъехать и налево
                TryBackAndLeftStrategy();
            }
        }
    }

    void CompleteLaneChange()
    {
        // Меняем основное направление на противоположное
        mainDirection = -mainDirection;
        currentDirection = mainDirection;

        // Сбрасываем флаги
        isChangingLane = false;
        laneChangeProgress = 0f;
        leftTurnAttempts = 0;

        Debug.Log("Смена полосы завершена. Новое направление: " + currentDirection);
    }

    void CheckForDirt()
    {
        if (isReturningToStart || cleaningCompleted) return;

        Collider2D[] dirtColliders = Physics2D.OverlapCircleAll(
            transform.position,
            vacuumRadius,
            dirtLayer
        );

        foreach (Collider2D dirt in dirtColliders)
        {
            CleanDirt(dirt.gameObject);
        }
    }

    void CleanDirt(GameObject dirtObject)
    {
        Destroy(dirtObject);
        Debug.Log("Убран мусор: " + dirtObject.name);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & dirtLayer) != 0)
        {
            CleanDirt(other.gameObject);
        }
    }

    Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

}
