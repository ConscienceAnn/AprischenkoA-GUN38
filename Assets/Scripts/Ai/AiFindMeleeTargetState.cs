using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiFindMeleeTargetState : AiState
{
    public AiStateId GetId()
    {
        return AiStateId.FindTarget; // »спользуем существующий ID
    }

    public void Enter(AiAgent agent)
    {
        agent.navMeshAgent.speed = agent.config.findTargetSpeed;
        Debug.Log($"{agent.name}: Entered FindTarget State (Melee)");
    }

    public void Update(AiAgent agent)
    {
        // Wander - идем в случайную точку если нет цели
        if (!agent.navMeshAgent.hasPath)
        {
            WorldBounds worldBounds = GameObject.FindObjectOfType<WorldBounds>();
            if (worldBounds != null)
            {
                agent.navMeshAgent.destination = worldBounds.RandomPosition();
            }
        }

        // ≈сли есть цель - переходим в атаку
        if (agent.targeting.HasTarget)
        {
            agent.stateMachine.ChangeState(AiStateId.MeleeAttackTarget);
        }
    }

    public void Exit(AiAgent agent)
    {
    }
}