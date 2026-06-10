using GameECS;
using UnityEngine;
using System.Collections.Generic;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_AttackTarget : CommandState
    {
        private EcsPool<AttackTarget> attackPool;
        private EcsPool<HitRequest> hitRequestPool;
        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<HitPointsComponent> hitPointsPool;
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<CombatComponent> combatPool;
        private EcsPool<TeamComponent> teamPool;
        private EcsPool<GameObjectComponent> gameObjectPool; // ДОБАВИТЬ ЭТУ СТРОКУ

        private EcsWorld world;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.ATTACK_TARGET;
        }

        public override void Enter(int entity, object args)
        {
            if (args is not Entity targetEntity)
            {
                Debug.LogError($"[AttackTarget] Entity {entity}: args is not Entity! Type: {args?.GetType()}");
                this.Complete(entity);
                return;
            }

            Debug.Log($"[AttackTarget] Enter: Attacker={entity}, Target={targetEntity.Id} ({targetEntity.name})");

            if (teamPool.HasComponent(entity))
            {
                ref var myTeam = ref teamPool.GetComponent(entity);
                Debug.Log($"[AttackTarget] Attacker {entity} Team={myTeam.playerId}");
            }
            else
            {
                Debug.LogError($"[AttackTarget] Attacker {entity} has NO TeamComponent!");
                this.Complete(entity);
                return;
            }

            if (teamPool.HasComponent(targetEntity.Id))
            {
                ref var targetTeam = ref teamPool.GetComponent(targetEntity.Id);
                Debug.Log($"[AttackTarget] Target {targetEntity.Id} Team={targetTeam.playerId}");

                ref var myTeam = ref teamPool.GetComponent(entity);
                if (myTeam.playerId == targetTeam.playerId)
                {
                    Debug.LogWarning($"[AttackTarget] Entity {entity} (Team {myTeam.playerId}) tried to attack ally {targetEntity.Id} (Team {targetTeam.playerId})! Cancelling.");
                    this.Complete(entity);
                    return;
                }
            }
            else
            {
                Debug.LogWarning($"[AttackTarget] Target {targetEntity.Id} has NO TeamComponent!");
            }

            this.attackPool.SetComponent(entity, new AttackTarget
            {
                targetId = targetEntity.Id
            });

            Debug.Log($"[AttackTarget] Attack command accepted: {entity} → {targetEntity.Id}");
        }

        public override void Exit(int entity)
        {
            this.attackPool.RemoveComponent(entity);
            this.hitRequestPool.RemoveComponent(entity);
            this.moveToPositionPool.RemoveComponent(entity);
        }

        public override void Update(int entity)
        {
            // Проверяем существование текущей цели
            if (!IsTargetExists(entity))
            {
                Debug.Log($"CommandState_AttackTarget: Current target died or doesn't exist");

                int newTargetId = FindNearestEnemy(entity);

                if (newTargetId != -1)
                {
                    attackPool.SetComponent(entity, new AttackTarget { targetId = newTargetId });
                    Debug.Log($"CommandState_AttackTarget: Switching to new enemy target {newTargetId}");
                }
                else
                {
                    Debug.Log($"CommandState_AttackTarget: No more enemies found, completing attack command");
                    this.Complete(entity);
                    return;
                }
            }

            // Получаем ID цели (ПЕРЕМЕСТИТЬ СЮДА, ДО ИСПОЛЬЗОВАНИЯ)
            if (!attackPool.HasComponent(entity))
            {
                this.Complete(entity);
                return;
            }

            ref var attackData = ref this.attackPool.GetComponent(entity);
            int targetId = attackData.targetId;

            // Проверяем GameObject цели (теперь targetId объявлен)
            if (gameObjectPool.HasComponent(targetId))
            {
                ref var targetGo = ref gameObjectPool.GetComponent(targetId);
                if (targetGo.value == null || !targetGo.value.activeInHierarchy)
                {
                    Debug.Log($"Target {targetId} GameObject is destroyed");
                    this.Complete(entity);
                    return;
                }
            }

            // Дополнительная проверка: не атакуем ли союзника
            if (teamPool.HasComponent(entity) && teamPool.HasComponent(targetId))
            {
                ref var myTeam = ref teamPool.GetComponent(entity);
                ref var targetTeam = ref teamPool.GetComponent(targetId);

                if (myTeam.playerId == targetTeam.playerId)
                {
                    Debug.LogWarning($"Entity {entity} is trying to attack ally {targetId}! Stopping.");
                    this.Complete(entity);
                    return;
                }
            }

            // Проверяем наличие компонентов для расчёта дистанции
            if (!transformPool.HasComponent(entity) || !transformPool.HasComponent(targetId))
            {
                return;
            }

            if (!combatPool.HasComponent(entity))
            {
                Debug.LogWarning($"Entity {entity} has no CombatComponent!");
                return;
            }

            ref var myTransform = ref transformPool.GetComponent(entity);
            ref var targetTransform = ref transformPool.GetComponent(targetId);
            ref var combat = ref combatPool.GetComponent(entity);

            float distance = Vector3.Distance(myTransform.value.position, targetTransform.value.position);

            // Если на дистанции атаки - бьём
            if (distance <= combat.minDistance)
            {
                if (!hitRequestPool.HasComponent(entity))
                {
                    hitRequestPool.SetComponent(entity, new HitRequest
                    {
                        targetId = targetId
                    });
                }

                if (moveToPositionPool.HasComponent(entity))
                {
                    moveToPositionPool.RemoveComponent(entity);
                }
            }
            else
            {
                if (!moveToPositionPool.HasComponent(entity))
                {
                    moveToPositionPool.SetComponent(entity, new MoveToPositionData
                    {
                        destination = targetTransform.value.position,
                        stoppingDistance = combat.minDistance,
                        isReached = false
                    });
                }

                if (hitRequestPool.HasComponent(entity))
                {
                    hitRequestPool.RemoveComponent(entity);
                }
            }
        }

        private bool IsTargetExists(int entity)
        {
            if (!attackPool.HasComponent(entity)) return false;

            ref var targetId = ref this.attackPool.GetComponent(entity).targetId;

            if (!this.world.IsEntityExists(targetId))
            {
                return false;
            }

            if (!this.hitPointsPool.HasComponent(targetId))
            {
                return false;
            }

            ref var targetHitPoints = ref this.hitPointsPool.GetComponent(targetId);
            return targetHitPoints.current > 0;
        }

        private int FindNearestEnemy(int entity)
        {
            if (!transformPool.HasComponent(entity)) return -1;

            ref var myTransform = ref transformPool.GetComponent(entity);
            Vector3 myPosition = myTransform.value.position;

            int myTeamId = -1;
            if (teamPool.HasComponent(entity))
            {
                ref var myTeam = ref teamPool.GetComponent(entity);
                myTeamId = myTeam.playerId;
            }

            int targetTeamId = (myTeamId == 1) ? 2 : 1;

            int nearestEnemyId = -1;
            float nearestDistance = 20f;

            var allEntities = GameObject.FindObjectsOfType<Entity>();

            foreach (var potentialTarget in allEntities)
            {
                if (potentialTarget == null) continue;

                int targetEntity = potentialTarget.Id;
                if (targetEntity == entity) continue;

                if (!teamPool.HasComponent(targetEntity)) continue;
                ref var team = ref teamPool.GetComponent(targetEntity);

                if (team.playerId != targetTeamId) continue;

                if (!hitPointsPool.HasComponent(targetEntity)) continue;
                ref var hp = ref hitPointsPool.GetComponent(targetEntity);
                if (hp.current <= 0) continue;

                if (transformPool.HasComponent(targetEntity))
                {
                    ref var targetTransform = ref transformPool.GetComponent(targetEntity);
                    float distance = Vector3.Distance(myPosition, targetTransform.value.position);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestEnemyId = targetEntity;
                    }
                }
            }

            return nearestEnemyId;
        }
    }
}