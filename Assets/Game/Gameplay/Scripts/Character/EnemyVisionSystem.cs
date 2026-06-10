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

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!visionPool.HasComponent(entity)) return;

            ref var vision = ref visionPool.GetComponent(entity);

            // Инициализация интервала, если не задан
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
        }

        private void DetectNearestPlayer(int entity, ref VisionComponent vision)
        {
            if (!transformPool.HasComponent(entity)) return;

            ref var transform = ref transformPool.GetComponent(entity);
            Vector3 position = transform.value.position;

            int nearestPlayerId = -1;
            float nearestDistance = vision.radius;

            // Получаем всех Entity на сцене
            var allEntities = GameObject.FindObjectsOfType<Entity>();

            foreach (var potentialTarget in allEntities)
            {
                if (potentialTarget == null) continue;

                int targetEntity = potentialTarget.Id;

                // Пропускаем себя
                if (targetEntity == entity) continue;

                // Проверяем наличие TeamComponent
                if (!teamPool.HasComponent(targetEntity)) continue;

                ref var team = ref teamPool.GetComponent(targetEntity);

                // Ищем игрока (команда 1) и проверяем что он не враг
                if (team.playerId != 1) continue; // teamId 1 = игрок

                // Проверяем жив ли
                if (!hpPool.HasComponent(targetEntity)) continue;

                ref var hp = ref hpPool.GetComponent(targetEntity);
                if (hp.current <= 0) continue;

                // Проверяем расстояние
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

            if (nearestPlayerId != -1)
            {
                AttackPlayer(entity, nearestPlayerId, ref vision);
            }
            else
            {
                ReturnToPatrol(entity, ref vision);
            }
        }

        private void AttackPlayer(int entity, int playerId, ref VisionComponent vision)
        {
            // Проверяем, не атакуем ли уже
            if (commandPool.HasComponent(entity))
            {
                ref var currentCommand = ref commandPool.GetComponent(entity);
                if (currentCommand.type == CommandType.ATTACK_TARGET)
                {
                    vision.detectedTargetId = playerId;
                    return;
                }
            }

            // Находим Entity объект игрока
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
                // Убираем текущую команду
                if (commandPool.HasComponent(entity))
                {
                    commandPool.RemoveComponent(entity);
                }

                // Устанавливаем команду атаки
                commandPool.SetComponent(entity, new CommandRequest
                {
                    type = CommandType.ATTACK_TARGET,
                    args = targetEntityObj,
                    status = CommandStatus.IDLE
                });

                vision.detectedTargetId = playerId;
                float distance = Vector3.Distance(transformPool.GetComponent(entity).value.position, targetEntityObj.transform.position);
                Debug.Log($"Enemy {entity} detected player at distance {distance}! Attacking!");
            }
        }

        private void ReturnToPatrol(int entity, ref VisionComponent vision)
        {
            // Если враг атаковал и игрок пропал - возвращаемся к патрулированию
            if (commandPool.HasComponent(entity))
            {
                ref var currentCommand = ref commandPool.GetComponent(entity);
                if (currentCommand.type == CommandType.ATTACK_TARGET)
                {
                    commandPool.RemoveComponent(entity);
                    Debug.Log($"Enemy {entity} lost player. Returning to patrol.");

                    // Заново запускаем патрулирование
                    StartPatrol(entity);
                }
            }

            vision.detectedTargetId = -1;
        }

        private void StartPatrol(int entity)
        {
            // Получаем точки патрулирования
            var patrolPoints = GetPatrolPoints();

            if (patrolPoints.Count > 0)
            {
                commandPool.SetComponent(entity, new CommandRequest
                {
                    type = CommandType.PATROL_BY_POINTS,
                    args = patrolPoints,
                    status = CommandStatus.IDLE
                });
                Debug.Log($"Enemy {entity} started patrol on {patrolPoints.Count} points");
            }
            else
            {
                // Создаём временные точки вокруг врага
                var fallbackPoints = GetFallbackPatrolPoints(entity);
                if (fallbackPoints.Count > 0)
                {
                    commandPool.SetComponent(entity, new CommandRequest
                    {
                        type = CommandType.PATROL_BY_POINTS,
                        args = fallbackPoints,
                        status = CommandStatus.IDLE
                    });
                    Debug.Log($"Enemy {entity} started fallback patrol");
                }
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

        private List<Vector3> GetFallbackPatrolPoints(int entity)
        {
            var points = new List<Vector3>();

            if (transformPool.HasComponent(entity))
            {
                ref var transform = ref transformPool.GetComponent(entity);
                Vector3 pos = transform.value.position;

                // Квадратный маршрут вокруг текущей позиции
                points.Add(pos + new Vector3(5, 0, 0));
                points.Add(pos + new Vector3(0, 0, 5));
                points.Add(pos + new Vector3(-5, 0, 0));
                points.Add(pos + new Vector3(0, 0, -5));
                points.Add(pos);
            }

            return points;
        }
    }
}