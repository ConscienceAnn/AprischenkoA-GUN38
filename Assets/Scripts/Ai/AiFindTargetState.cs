using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiFindTargetState : AiState
{
    public AiStateId GetId() {
        return AiStateId.FindTarget;
    }

    public void Enter(AiAgent agent) {
        agent.navMeshAgent.speed = agent.config.findTargetSpeed;
    }

    public void Update(AiAgent agent) {
        // Wander
        if (!agent.navMeshAgent.hasPath)
        {
            WorldBounds worldBounds = GameObject.FindObjectOfType<WorldBounds>();
            if (worldBounds != null)
            {
                agent.navMeshAgent.destination = worldBounds.RandomPosition();
            }
        }

        if (agent.targeting.HasTarget)
        {
            // Определяем тип врага
            bool isMelee = agent.GetComponent<MeleeCombat>() != null;

            if (isMelee)
            {
                // Для ближнего боя - переходим в MeleeAttackTarget
                agent.stateMachine.ChangeState(AiStateId.MeleeAttackTarget);
                Debug.Log($"{agent.name}: Found target, switching to MeleeAttackTarget");
            }
            else
            {
                // Для стрелков - переходим в обычный AttackTarget
                agent.stateMachine.ChangeState(AiStateId.AttackTarget);
            }
        }
    }

    public void Exit(AiAgent agent) {
    }
}
