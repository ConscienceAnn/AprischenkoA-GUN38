using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiIdleState : AiState
{
    public AiStateId GetId() {
        return AiStateId.Idle;
    }

    public void Enter(AiAgent agent) {
        if (agent.weapons != null && agent.weapons.HasWeapon())
        {
            agent.weapons.DeactivateWeapon();
        }
        agent.navMeshAgent.ResetPath();
    }

    public void Update(AiAgent agent) {

        // Проверяем, есть ли ссылка на игрока
        if (agent.playerTransform == null)
        {
            Debug.LogError($"{agent.name}: playerTransform is NULL!");
            return;
        }

        // Всегда пытаемся найти игрока, даже если не видим
        if (agent.weapons.Count() > 0)
        {
            // Если есть оружие - ищем игрока
            Debug.Log($"{agent.name} has weapon, switching to FindTarget");
            agent.stateMachine.ChangeState(AiStateId.FindTarget);
        }
        else
        {
            // Если нет оружия - сначала ищем оружие
            Debug.Log($"{agent.name} no weapon, switching to FindWeapon");
            agent.stateMachine.ChangeState(AiStateId.FindWeapon);
        }
    }

    public void Exit(AiAgent agent) {
    }
}
