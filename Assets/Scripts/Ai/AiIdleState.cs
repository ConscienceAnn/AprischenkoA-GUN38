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

        // Определяем тип врага - есть ли у него MeleeCombat
        bool isMelee = agent.GetComponent<MeleeCombat>() != null;

        if (isMelee)
        {
            // ЛОГИКА ДЛЯ БЛИЖНЕГО БОЯ
            MeleeWeapon weapon = agent.GetComponentInChildren<MeleeWeapon>();

            if (weapon == null)
            {
                // Нет оружия - ищем пикап с ножом
                Debug.Log($"{agent.name}: No melee weapon, switching to FindMeleeWeapon");
                agent.stateMachine.ChangeState(AiStateId.FindMeleeWeapon);
                return;
            }
            else
            {
                // Есть оружие - ищем игрока
                if (agent.sensor != null && agent.sensor.IsInSight(agent.playerTransform.gameObject))
                {
                    Debug.Log($"{agent.name}: Has weapon and sees player, switching to ChasePlayer");
                    agent.stateMachine.ChangeState(AiStateId.FindTarget);
                    return;
                }
            }
        }
        else
        {
            // ЛОГИКА ДЛЯ СТРЕЛКОВОГО ОРУЖИЯ (старые враги)
            // Проверяем, есть ли оружие
            if (agent.weapons != null && agent.weapons.Count() > 0)
            {
                // Если есть оружие - ищем игрока
                if (agent.sensor != null && agent.sensor.IsInSight(agent.playerTransform.gameObject))
                {
                    agent.stateMachine.ChangeState(AiStateId.FindTarget);
                }
            }
            else
            {
                // Если нет оружия - ищем оружие
                agent.stateMachine.ChangeState(AiStateId.FindWeapon);
            }
        }


    }

    public void Exit(AiAgent agent) {
    }
}
