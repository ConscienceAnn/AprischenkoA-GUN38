using UnityEngine;

public class CollectState : IAIState
{
    public AIStateType StateType => AIStateType.Collect;
    private float collectTimer = 0f;

    public void Enter(AIAgent agent)
    {
        if (agent.TargetItem == null)
        {
            Debug.LogError("Collect: Нет предмета для сбора!");
            agent.ChangeState(new IdleState());
            return;
        }

        // ОСТАНАВЛИВАЕМСЯ для сбора
        agent.NavAgent.isStopped = true;
        agent.NavAgent.velocity = Vector3.zero;

        Debug.Log($"Collect: Начинаю сбор предмета {agent.TargetItem.name}");
        collectTimer = 0f;
    }

    public void Update(AIAgent agent)
    {
        if (agent.TargetItem == null)
        {
            Debug.Log("Collect: Предмет пропал");
            agent.ChangeState(new IdleState());
            return;
        }

        collectTimer += Time.deltaTime;

        // Анимация сбора 
        if (collectTimer >= 0.6f)
        {
            CollectItem(agent);
            agent.ChangeState(new IdleState());
        }
        //else
        //{
            
        //    Debug.Log($"Collect: Собираю... {collectTimer:F1}/3.0 сек");
        //}
    }

    public void Exit(AIAgent agent)
    {
        Debug.Log("Collect: Заканчиваю сбор");
        agent.NavAgent.isStopped = false;
    }

    private void CollectItem(AIAgent agent)
    {
        Debug.Log($"СОБРАЛ: {agent.TargetItem.name}!");
        GameObject.Destroy(agent.TargetItem);
        agent.TargetItem = null;
    }
}