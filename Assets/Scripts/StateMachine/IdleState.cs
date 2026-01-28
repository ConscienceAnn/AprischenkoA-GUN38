using UnityEngine;

public class IdleState : IAIState
{
    public AIStateType StateType => AIStateType.Idle;

    public void Enter(AIAgent agent)
    {
        agent.IdleTimer = 0f;
        agent.NavAgent.isStopped = true;
        Debug.Log("Idle: Стою на месте");
    }

    public void Update(AIAgent agent)
    {
        agent.IdleTimer += Time.deltaTime;

        // Проверяем, есть ли еще предметы на сцене
        if (!AreThereAnyItemsLeft())
        {
            // Если предметов нет - остаемся в Idle
            Debug.Log("Idle: Предметов больше нет, остаюсь в ожидании");
            return;
        }

        if (agent.IdleTimer >= agent.IdleTime)
        {
            agent.ChangeState(new SearchState());
        }
    }

    public void Exit(AIAgent agent)
    {
        Debug.Log("Idle: Начинаю движение");
    }

    // Метод для проверки наличия предметов на сцене
    private bool AreThereAnyItemsLeft()
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Collectible");
        bool hasItems = items.Length > 0;

        if (Time.frameCount % 60 == 0) // Проверяем раз в секунду
        {
            Debug.Log($"Осталось предметов: {items.Length}");
        }

        return hasItems;
    }

}