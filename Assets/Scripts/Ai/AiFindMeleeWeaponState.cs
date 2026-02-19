using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiFindMeleeWeaponState : AiState
{
    GameObject pickup;
    GameObject[] pickups = new GameObject[3];

    public AiStateId GetId()
    {
        return AiStateId.FindMeleeWeapon;
    }

    public void Enter(AiAgent agent)
    {
        pickup = null;
        agent.navMeshAgent.speed = agent.config.findWeaponSpeed;
        agent.navMeshAgent.ResetPath();

        Debug.Log($"{agent.name}: Entered FindMeleeWeapon State");
    }

    public void Update(AiAgent agent)
    {
        // Find pickup
        if (!pickup)
        {
            pickup = FindPickup(agent);

            if (pickup)
            {
                CollectPickup(agent, pickup);
                return;
            }
        }

        // Wander
        if (!agent.navMeshAgent.hasPath && !agent.navMeshAgent.pathPending)
        {
            WorldBounds worldBounds = GameObject.FindObjectOfType<WorldBounds>();
            if (worldBounds != null)
            {
                agent.navMeshAgent.destination = worldBounds.RandomPosition();
            }
        }

        // Проверяем, есть ли уже оружие
        MeleeWeapon weapon = agent.GetComponentInChildren<MeleeWeapon>();
        if (weapon != null)
        {
            Debug.Log($"{agent.name}: Found melee weapon! Switching to FindTarget");
            agent.stateMachine.ChangeState(AiStateId.FindTarget);
        }
    }

    public void Exit(AiAgent agent)
    {
    }

    GameObject FindPickup(AiAgent agent)
    {
        // Ищем пикапы с тегом "MeleeWeapon"
        int count = agent.sensor.Filter(pickups, "Pickup", "MeleeWeapon");

        if (count > 0)
        {
            float bestAngle = float.MaxValue;
            GameObject bestPickup = pickups[0];
            for (int i = 0; i < count; ++i)
            {
                GameObject pickup = pickups[i];
                float pickupAngle = Vector3.Angle(agent.transform.forward, pickup.transform.position - agent.transform.position);
                if (pickupAngle < bestAngle)
                {
                    bestAngle = pickupAngle;
                    bestPickup = pickup;
                }
            }
            Debug.Log($"{agent.name}: Found MeleeWeapon pickup at {bestPickup.transform.position}");
            return bestPickup;
        }
        return null;
    }

    void CollectPickup(AiAgent agent, GameObject pickup)
    {
        agent.navMeshAgent.destination = pickup.transform.position;

        // Проверяем, дошли ли до пикапа
        float distanceToPickup = Vector3.Distance(agent.transform.position, pickup.transform.position);
        if (distanceToPickup < 2f)
        {
            TryPickupWeapon(agent, pickup);
        }
    }

    void TryPickupWeapon(AiAgent agent, GameObject pickup)
    {
        MeleeWeaponPickup weaponPickup = pickup.GetComponent<MeleeWeaponPickup>();

        if (weaponPickup != null)
        {
            GameObject weaponPrefab = weaponPickup.GetWeaponPrefab();

            AttachMeleeWeapon attacher = agent.GetComponent<AttachMeleeWeapon>();
            if (attacher != null)
            {
                attacher.PickupWeapon(weaponPrefab);
            }

            GameObject.Destroy(pickup);
            this.pickup = null;

            Debug.Log($"{agent.name}: Picked up melee weapon!");
        }
    }
}