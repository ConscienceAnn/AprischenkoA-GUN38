using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiMeleeAttackTargetState : AiState
{
    public AiStateId GetId()
    {
        return AiStateId.MeleeAttackTarget;
    }

    public void Enter(AiAgent agent)
    {
        Debug.Log($"{agent.name}: Entered MeleeAttackTarget State");

        // Настраиваем NavMesh для ближнего боя
        agent.navMeshAgent.stoppingDistance = 1.1f; // Подходим ближе
        agent.navMeshAgent.speed = agent.config.attackSpeed;
    }

    public void Update(AiAgent agent)
    {
        // Проверяем, есть ли цель
        if (!agent.targeting.HasTarget)
        {
            agent.stateMachine.ChangeState(AiStateId.FindTarget);
            return;
        }

        // Получаем позицию цели
        Vector3 targetPosition = agent.targeting.TargetPosition;
        float distanceToTarget = Vector3.Distance(agent.transform.position, targetPosition);

        // Двигаемся к цели
        agent.navMeshAgent.destination = targetPosition;

        // Поворачиваемся лицом к цели
        Vector3 direction = (targetPosition - agent.transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            agent.transform.rotation = Quaternion.RotateTowards(
                agent.transform.rotation,
                Quaternion.LookRotation(direction),
                Time.deltaTime * 360f
            );
        }

        // Атакуем, если подошли достаточно близко
        MeleeCombat melee = agent.GetComponent<MeleeCombat>();
        if (melee != null)
        {
            // MeleeCombat сам решает когда атаковать в своем Update
            // Ничего не делаем, он работает автоматически
        }

        // Проверяем низкое здоровье
        if (agent.health != null && agent.health.IsLowHealth())
        {
            agent.stateMachine.ChangeState(AiStateId.FindHealth);
            return;
        }
    }

    public void Exit(AiAgent agent)
    {
        agent.navMeshAgent.stoppingDistance = 0.0f;
        Debug.Log($"{agent.name}: Exited MeleeAttackTarget State");
    }
}