using GameECS;
using UnityEngine;
using System.Collections.Generic;

namespace Game.GameEngine.Ecs
{
    public sealed class PlayerVisionSystem : IEcsFixedUpdate
    {
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<CommandRequest> commandPool;
        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<GroupMoveData> groupMovePool;
        private EcsPool<PatrolData> patrolPool;
        private EcsPool<HitPointsComponent> hpPool;
        private EcsPool<TeamComponent> teamPool;
        private EcsPool<AttackTarget> attackPool;


        private HashSet<int> attackingEntities = new HashSet<int>();

        private EcsWorld world;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            // Только для игроков
            if (!teamPool.HasComponent(entity)) return;
            ref var team = ref teamPool.GetComponent(entity);
            if (team.playerId != 1) return;

            if (!transformPool.HasComponent(entity)) return;

            if (HasActiveMoveCommand(entity))
            {
                attackingEntities.Remove(entity);
                return;
            }


            if (attackingEntities.Contains(entity)) return;

            int nearestEnemy = FindNearestEnemy(entity);

            if (nearestEnemy != -1)
            {
                attackingEntities.Add(entity);
                AttackEnemy(entity, nearestEnemy);
            }
            else
            {
                attackingEntities.Remove(entity);
            }
        }

        private bool HasActiveMoveCommand(int entity)
        {

            if (commandPool.HasComponent(entity))
            {
                ref var cmd = ref commandPool.GetComponent(entity);

                if (cmd.type == CommandType.MOVE_TO_POSITION)
                {
                    if (cmd.status == CommandStatus.COMPLETE || cmd.status == CommandStatus.FAIL)
                    {
                        return false;
                    }
                    return true;
                }

                if (cmd.type == CommandType.PATROL_BY_POINTS)
                {

                    if (IsEnemyNearby(entity))
                    {
                        return false;
                    }

                    if (cmd.status == CommandStatus.COMPLETE || cmd.status == CommandStatus.FAIL)
                    {
                        return false;
                    }
                    return true;
                }
            }


            if (moveToPositionPool.HasComponent(entity))
            {
                ref var moveData = ref moveToPositionPool.GetComponent(entity);
                if (moveData.isReached)
                {
                    return false;
                }
                return true;
            }


            if (groupMovePool.HasComponent(entity))
            {
                ref var groupData = ref groupMovePool.GetComponent(entity);
                if (groupData.hasStopped)
                {
                    return false;
                }
                return true;
            }

            return false;
        }


        private bool IsEnemyNearby(int entity)
        {
            if (!transformPool.HasComponent(entity)) return false;

            ref var myTransform = ref transformPool.GetComponent(entity);
            Vector3 myPos = myTransform.value.position;
            float radius = 15f; 

            var allEntities = GameObject.FindObjectsOfType<Entity>();

            foreach (var potentialEnemy in allEntities)
            {
                if (potentialEnemy == null) continue;

                int targetEntity = potentialEnemy.Id;
                if (targetEntity == entity) continue;


                if (!teamPool.HasComponent(targetEntity)) continue;
                ref var targetTeam = ref teamPool.GetComponent(targetEntity);
                if (targetTeam.playerId != 2) continue;


                if (!hpPool.HasComponent(targetEntity)) continue;
                ref var hp = ref hpPool.GetComponent(targetEntity);
                if (hp.current <= 0) continue;


                if (transformPool.HasComponent(targetEntity))
                {
                    ref var targetTransform = ref transformPool.GetComponent(targetEntity);
                    float dist = Vector3.Distance(myPos, targetTransform.value.position);
                    if (dist < radius)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private int FindNearestEnemy(int entity)
        {
            ref var myTransform = ref transformPool.GetComponent(entity);
            Vector3 myPos = myTransform.value.position;
            float radius = 15f;

            int nearestId = -1;
            float nearestDist = radius;

            var allEntities = GameObject.FindObjectsOfType<Entity>();

            foreach (var potentialEnemy in allEntities)
            {
                if (potentialEnemy == null) continue;

                int targetEntity = potentialEnemy.Id;
                if (targetEntity == entity) continue;


                if (!teamPool.HasComponent(targetEntity)) continue;
                ref var targetTeam = ref teamPool.GetComponent(targetEntity);
                if (targetTeam.playerId != 2) continue;

                if (!hpPool.HasComponent(targetEntity)) continue;
                ref var hp = ref hpPool.GetComponent(targetEntity);
                if (hp.current <= 0) continue;

                if (transformPool.HasComponent(targetEntity))
                {
                    ref var targetTransform = ref transformPool.GetComponent(targetEntity);
                    float dist = Vector3.Distance(myPos, targetTransform.value.position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestId = targetEntity;
                    }
                }
            }

            return nearestId;
        }

        private void AttackEnemy(int entity, int enemyId)
        {

            Entity enemyEntity = null;
            var allEntities = GameObject.FindObjectsOfType<Entity>();
            foreach (var e in allEntities)
            {
                if (e != null && e.IsExists() && e.Id == enemyId)
                {
                    enemyEntity = e;
                    break;
                }
            }

            if (enemyEntity == null) return;


            if (commandPool.HasComponent(entity))
            {
                commandPool.RemoveComponent(entity);
            }



            commandPool.SetComponent(entity, new CommandRequest
            {
                type = CommandType.ATTACK_TARGET,
                args = enemyEntity,
                status = CommandStatus.IDLE
            });

            Debug.Log($"Player {entity} auto-attacking enemy {enemyId} (no active move command)");
        }
    }
}