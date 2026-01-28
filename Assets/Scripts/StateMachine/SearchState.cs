using UnityEngine;
using UnityEngine.AI;

public class SearchState : IAIState
{
    public AIStateType StateType => AIStateType.Search;
    private Vector3 randomDestination;
    private float itemCheckCooldown = 1f; // Проверка раз в секунду
    private float lastItemCheckTime = 0f;

    public void Enter(AIAgent agent)
    {
        agent.TargetItem = null;
        agent.NavAgent.isStopped = false;

        Debug.Log("Search: Начинаю поиск");

        // Если предметов нет на сцене - сразу в Idle
        if (!AreThereAnyItemsLeftOnScene())
        {
            Debug.Log("Search: Предметов нет на сцене, возвращаюсь в Idle");
            agent.ChangeState(new IdleState());
            return;
        }

        // Если уже есть цель - идем к ней
        if (agent.TargetItem != null)
        {
            agent.NavAgent.SetDestination(agent.TargetItem.transform.position);
            Debug.Log($"Search: Уже есть цель, иду к {agent.TargetItem.name}");
        }
        else
        {
            // Иначе ищем случайную точку
            SetRandomDestination(agent);
        }
    }

    public void Update(AIAgent agent)
    {
        // Проверяем предметы на сцене (с кэшированием)
        if (Time.time - lastItemCheckTime > itemCheckCooldown)
        {
            lastItemCheckTime = Time.time;

            if (!AreThereAnyItemsLeftOnScene())
            {
                Debug.Log("Search: Предметов больше нет, возвращаюсь в Idle");
                agent.ChangeState(new IdleState());
                return;
            }
        }

        // Проверяем предметы в радиусе поиска
        CheckForItemsInRadius(agent);

        // Если нашли предмет - идем к нему
        if (agent.TargetItem != null)
        {
            // Устанавливаем цель
            agent.NavAgent.SetDestination(agent.TargetItem.transform.position);

            // Проверяем расстояние
            float distance = Vector3.Distance(
                agent.transform.position,
                agent.TargetItem.transform.position
            );

            // Если близко - начинаем сбор
            if (distance < 1.5f)
            {
                Debug.Log($"Search: Дошел до предмета, начинаю сбор");
                agent.ChangeState(new CollectState());
                return;
            }
        }
        else
        {
            // Если нет цели и достигли точки - новая точка
            // НО: только если действительно есть куда идти и маршрут завершен
            if (agent.NavAgent.pathStatus == NavMeshPathStatus.PathComplete &&
                !agent.NavAgent.pathPending &&
                agent.NavAgent.remainingDistance < 0.5f)
            {
                SetRandomDestination(agent);
            }
        }
    }

    public void Exit(AIAgent agent)
    {
        Debug.Log("Search: Заканчиваю поиск");
    }

    private void SetRandomDestination(AIAgent agent)
    {
        // Более умный поиск случайной точки
        for (int i = 0; i < 5; i++) // Пробуем 5 раз найти хорошую точку
        {
            Vector3 randomDirection = Random.insideUnitSphere * 15f; // Увеличил радиус
            randomDirection += agent.transform.position;
            randomDirection.y = agent.transform.position.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 15f, NavMesh.AllAreas))
            {
                // Проверяем, чтобы точка была не слишком близко
                float distanceToPoint = Vector3.Distance(agent.transform.position, hit.position);
                if (distanceToPoint > 3f) // Минимум 3 единицы
                {
                    randomDestination = hit.position;
                    agent.NavAgent.SetDestination(randomDestination);
                    Debug.Log($"Search: Новая точка поиска: {randomDestination}");
                    return;
                }
            }
        }

        Debug.Log("Search: Не удалось найти подходящую точку для поиска");
    }

    private void CheckForItemsInRadius(AIAgent agent)
    {
        Collider[] items = Physics.OverlapSphere(
            agent.transform.position,
            agent.SearchRadius
        );

        foreach (Collider item in items)
        {
            if (item.CompareTag("Collectible") && item.gameObject.activeInHierarchy)
            {
                agent.TargetItem = item.gameObject;
                Debug.Log($"Search: Нашел предмет {item.name}");
                return; // Берем первый найденный
            }
        }
    }

    // Метод для проверки наличия предметов на всей сцене (с кэшированием)
    private bool AreThereAnyItemsLeftOnScene()
    {
      
        GameObject[] items = GameObject.FindGameObjectsWithTag("Collectible");

        if (items == null || items.Length == 0)
        {
            return false;
        }

        // Дополнительная проверка - хотя бы один активен
        foreach (var item in items)
        {
            if (item != null && item.activeInHierarchy)
                return true;
        }

        return false;
    }
}