using GameECS;
using UnityEngine;
using System.Collections.Generic;

namespace Game.GameEngine.Ecs
{
    public sealed class EnemyVisionSystem : IEcsFixedUpdate
    {
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<VisionComponent> visionPool;
        private EcsPool<TeamComponent> teamPool;
        private EcsPool<CommandRequest> commandPool;
        private EcsPool<HitPointsComponent> hpPool;
        private EcsPool<AttackTarget> attackPool;
        private EcsPool<GameObjectComponent> gameObjectPool;

        private bool enableDebug = true;

        private EcsWorld world;

        private Dictionary<int, int> currentTargets = new Dictionary<int, int>();

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            // только для врагов (Team = 2)
            if (teamPool.HasComponent(entity))
            {
                ref var team = ref teamPool.GetComponent(entity);
                if (team.playerId != 2)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            if (!visionPool.HasComponent(entity)) return;

            ref var vision = ref visionPool.GetComponent(entity);

            if (vision.checkInterval <= 0)
            {
                vision.checkInterval = 0.5f;
            }

            vision.lastCheckTime += Time.fixedDeltaTime;

            if (vision.lastCheckTime >= vision.checkInterval)
            {
                vision.lastCheckTime = 0;
                DetectNearestPlayer(entity, ref vision);
            }

            CheckCurrentTarget(entity);
        }

        private void DetectNearestPlayer(int entity, ref VisionComponent vision)
        {
            if (!transformPool.HasComponent(entity)) return;

            ref var transform = ref transformPool.GetComponent(entity);
            Vector3 position = transform.value.position;

            if (enableDebug && Time.frameCount % 60 == 0) 
            {
                if (teamPool.HasComponent(entity))
                {
                    ref var myTeam = ref teamPool.GetComponent(entity);
                    Debug.Log($"[EnemyVisionSystem] Enemy {entity} checking for players. My Team: {myTeam.playerId}");
                }
                else
                {
                    Debug.LogWarning($"[EnemyVisionSystem] Enemy {entity} has NO TeamComponent!");
                }
            }

            int nearestPlayerId = -1;
            float nearestDistance = vision.radius;

            var allEntities = GameObject.FindObjectsOfType<Entity>();

            foreach (var potentialTarget in allEntities)
            {
                if (potentialTarget == null) continue;

                int targetEntity = potentialTarget.Id;
                if (targetEntity == entity) continue;

                if (!teamPool.HasComponent(targetEntity)) continue;
                ref var team = ref teamPool.GetComponent(targetEntity);

                if (team.playerId != 1) continue;

                if (gameObjectPool.HasComponent(targetEntity))
                {
                    ref var go = ref gameObjectPool.GetComponent(targetEntity);
                    if (go.value == null || !go.value.activeInHierarchy)
                    {
                        continue;
                    }
                }

                if (!hpPool.HasComponent(targetEntity)) continue;
                ref var hp = ref hpPool.GetComponent(targetEntity);
                if (hp.current <= 0) continue;

                if (transformPool.HasComponent(targetEntity))
                {
                    ref var targetTransform = ref transformPool.GetComponent(targetEntity);
                    float distance = Vector3.Distance(position, targetTransform.value.position);

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPlayerId = targetEntity;
                    }
                }
            }

            bool hasTarget = (nearestPlayerId != -1);
            int previousTarget = currentTargets.ContainsKey(entity) ? currentTargets[entity] : -1;

            if (hasTarget)
            {
                if (previousTarget != nearestPlayerId)
                {
                    Debug.Log($"[EnemyVisionSystem] Enemy {entity} found player {nearestPlayerId} at distance {nearestDistance:F2}");

                    if (teamPool.HasComponent(entity))
                    {
                        ref var myTeam = ref teamPool.GetComponent(entity);
                        Debug.Log($"[EnemyVisionSystem] Enemy {entity} Team={myTeam.playerId} ? Attacking Player {nearestPlayerId} Team=1");

                        if (myTeam.playerId == 1)
                        {
                            Debug.LogError($"[EnemyVisionSystem] CRITICAL: Enemy {entity} has Team=1 (PLAYER)! This is wrong!");
                        }
                    }

                    currentTargets[entity] = nearestPlayerId;
                    StartAttackingPlayer(entity, nearestPlayerId, ref vision);
                }
            }
            else
            {
                if (previousTarget != -1)
                {
                    Debug.Log($"[EnemyVisionSystem] Enemy {entity} lost target");
                    currentTargets[entity] = -1;
                    StopAttacking(entity);
                }
            }
        }

        private void CheckCurrentTarget(int entity)
        {
            if (!commandPool.HasComponent(entity)) return;

            ref var currentCommand = ref commandPool.GetComponent(entity);
            if (currentCommand.type != CommandType.ATTACK_TARGET) return;

            if (currentCommand.args == null)
            {
                commandPool.RemoveComponent(entity);
                return;
            }

            if (currentCommand.args is not Entity targetEntity)
            {
                commandPool.RemoveComponent(entity);
                return;
            }

            if (targetEntity == null)
            {
                commandPool.RemoveComponent(entity);
                currentTargets[entity] = -1;
                StartPatrol(entity);
                return;
            }

            int targetId = targetEntity.Id;

            if (!world.IsEntityExists(targetId))
            {
                commandPool.RemoveComponent(entity);
                currentTargets[entity] = -1;
                StartPatrol(entity);
                return;
            }

            bool targetIsAlive = false;

            if (gameObjectPool.HasComponent(targetId))
            {
                ref var go = ref gameObjectPool.GetComponent(targetId);
                if (go.value != null && go.value.activeInHierarchy)
                {
                    if (hpPool.HasComponent(targetId))
                    {
                        ref var hp = ref hpPool.GetComponent(targetId);
                        targetIsAlive = (hp.current > 0);
                    }
                }
            }

            if (!targetIsAlive)
            {
                Debug.Log($"Enemy {entity}: Target {targetId} is dead or inactive, stopping attack");
                commandPool.RemoveComponent(entity);
                currentTargets[entity] = -1;
                StartPatrol(entity);
            }
        }

        private void StartAttackingPlayer(int entity, int playerId, ref VisionComponent vision)
        {
            if (teamPool.HasComponent(entity))
            {
                ref var myTeam = ref teamPool.GetComponent(entity);
                if (myTeam.playerId != 2)
                {
                    Debug.LogError($"[EnemyVisionSystem] BLOCKING ATTACK: Enemy {entity} has Team={myTeam.playerId}, not 2! Cannot attack player.");
                    return;
                }
            }

            var allEntities = GameObject.FindObjectsOfType<Entity>();
            Entity targetEntityObj = null;

            foreach (var potentialTarget in allEntities)
            {
                if (potentialTarget != null && potentialTarget.IsExists() && potentialTarget.Id == playerId)
                {
                    targetEntityObj = potentialTarget;
                    break;
                }
            }

            if (targetEntityObj != null)
            {
                if (commandPool.HasComponent(entity))
                {
                    commandPool.RemoveComponent(entity);
                }

                Debug.Log($"[EnemyVisionSystem] Enemy {entity} issuing ATTACK command on player {playerId}");

                commandPool.SetComponent(entity, new CommandRequest
                {
                    type = CommandType.ATTACK_TARGET,
                    args = targetEntityObj,
                    status = CommandStatus.IDLE
                });

                vision.detectedTargetId = playerId;
            }
        }

        private void StopAttacking(int entity)
        {
            if (commandPool.HasComponent(entity))
            {
                commandPool.RemoveComponent(entity);
                Debug.Log($"Enemy {entity}: No valid players, stopping attack");
            }

            if (visionPool.HasComponent(entity))
            {
                ref var vision = ref visionPool.GetComponent(entity);
                vision.detectedTargetId = -1;
            }

            StartPatrol(entity);
        }

        private void StartPatrol(int entity)
        {
            var patrolPoints = GetPatrolPoints();

            if (patrolPoints.Count > 0)
            {
                commandPool.SetComponent(entity, new CommandRequest
                {
                    type = CommandType.PATROL_BY_POINTS,
                    args = patrolPoints,
                    status = CommandStatus.IDLE
                });
                Debug.Log($"Enemy {entity} returned to patrol");
            }
        }

        private List<Vector3> GetPatrolPoints()
        {
            var points = new List<Vector3>();
            var patrolPointObjects = GameObject.FindGameObjectsWithTag("PatrolPoint");

            foreach (var point in patrolPointObjects)
            {
                if (point != null)
                    points.Add(point.transform.position);
            }

            return points;
        }
    }
}