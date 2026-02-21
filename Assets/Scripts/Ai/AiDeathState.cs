using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDeathState : AiState
{
    public Vector3 direction;

    public AiStateId GetId() {
        return AiStateId.Death;
    }

    public void Enter(AiAgent agent) {
        agent.ragdoll.ActivateRagdoll();
        direction.y = 1;
        agent.ragdoll.ApplyForce(direction * agent.config.dieForce);
        agent.ui?.gameObject.SetActive(false);
        agent.mesh.updateWhenOffscreen = true;

        // ПРОВЕРКА: для обычных врагов со стрелковым оружием
        if (agent.weapons != null)
        {
            agent.weapons.DropWeapon();
            agent.weapons.SetTarget(null);
        }

        // Для MeleeEnemy - удаляем нож
        MeleeWeapon meleeWeapon = agent.GetComponentInChildren<MeleeWeapon>();
        if (meleeWeapon != null)
        {
            GameObject.Destroy(meleeWeapon.gameObject);
        }

        // Отключаем MeleeCombat
        MeleeCombat meleeCombat = agent.GetComponent<MeleeCombat>();
        if (meleeCombat != null)
        {
            meleeCombat.enabled = false;
        }

        agent.navMeshAgent.enabled = false;
    }

    public void Update(AiAgent agent) {
    }

    public void Exit(AiAgent agent) {
        agent.navMeshAgent.enabled = true;
    }
}
