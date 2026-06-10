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

        // Кэш: кто уже атакует
        private HashSet<int> attackingEntities = new HashSet<int>();

        private EcsWorld world;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            // Только для игроков
            if (!teamPool.HasComponent(entity)) return;
            ref var team = ref teamPool.GetComponent(entity);
            if (team.playerId != 1) return;

            if (!transformPool.HasComponent(entity)) return;

            // ===== КЛЮЧЕВОЕ УСЛОВИЕ =====
            // Атакуем только если нет активной команды движения!
            if (HasActiveMoveCommand(entity))
            {
                // Есть команда движения - убираем из кэша атаки
                attackingEntities.Remove(entity);
                return;
            }
            // =============================

            // Пропускаем, если юнит уже атакует
            if (attackingEntities.Contains(entity)) return;

            // Проверяем, есть ли рядом враг
            int nearestEnemy = FindNearestEnemy(entity);

            if (nearestEnemy != -1)
            {
                // Нашли врага - атакуем!
                attackingEntities.Add(entity);
                AttackEnemy(entity, nearestEnemy);
            }
            else
            {
                // Нет врага - убираем из кэша
                attackingEntities.Remove(entity);
            }
        }

        // ===== ЭТОТ МЕТОД ДОБАВИТЬ СЮДА =====
        // Проверка, есть ли активная команда движения
        private bool HasActiveMoveCommand(int entity)
        {
            // Команда движения
            if (commandPool.HasComponent(entity))
            {
                ref var cmd = ref commandPool.GetComponent(entity);
                if (cmd.type == CommandType.MOVE_TO_POSITION ||
                    cmd.type == CommandType.PATROL_BY_POINTS)
                {
                    // Если команда завершена или провалена - не считается активной
                    if (cmd.status == CommandStatus.COMPLETE || cmd.status == CommandStatus.FAIL)
                    {
                        return false;
                    }
                    return true;
                }
            }

            // Движение к позиции
            if (moveToPositionPool.HasComponent(entity))
            {
                ref var moveData = ref moveToPositionPool.GetComponent(entity);
                // Если цель достигнута - не считается активным движением
                if (moveData.isReached)
                {
                    return false;
                }
                return true;
            }

            // Групповое движение
            if (groupMovePool.HasComponent(entity))
            {
                ref var groupData = ref groupMovePool.GetComponent(entity);
                // Если группа остановилась - не считается активным движением
                if (groupData.hasStopped)
                {
                    return false;
                }
                return true;
            }

            return false;
        }
        // ===== КОНЕЦ ДОБАВЛЕННОГО МЕТОДА =====

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

                // Проверяем команду - ищем врагов (Team=2)
                if (!teamPool.HasComponent(targetEntity)) continue;
                ref var targetTeam = ref teamPool.GetComponent(targetEntity);
                if (targetTeam.playerId != 2) continue;

                // Проверяем, жив ли враг
                if (!hpPool.HasComponent(targetEntity)) continue;
                ref var hp = ref hpPool.GetComponent(targetEntity);
                if (hp.current <= 0) continue;

                // Проверяем дистанцию
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
            // Находим Entity цели
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

            // Прерываем ТОЛЬКО команду, но НЕ движение?
            // В RTS: если юнит атакует, он останавливается
            if (commandPool.HasComponent(entity))
            {
                commandPool.RemoveComponent(entity);
            }

            // НЕ удаляем moveToPositionPool - это позволит юниту
            // после атаки запомнить, куда он шёл (опционально)

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