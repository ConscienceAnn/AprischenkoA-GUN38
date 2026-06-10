using GameECS;
using UnityEngine;
using System.Collections.Generic;

namespace Game.GameEngine.Ecs
{
    public sealed class GroupMoveSystem : IEcsFixedUpdate
    {
        private EcsPool<GroupMoveData> groupMovePool;
        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<MoveStepData> moveStepPool;
        private EcsPool<CommandRequest> commandPool;

        private readonly Dictionary<int, GroupInfo> groups = new();

        private class GroupInfo
        {
            public Vector3 destination;
            public int leaderId = -1;
            public bool leaderReached;
            public float leaderDistanceToTarget;
        }

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!groupMovePool.HasComponent(entity)) return;

            ref var groupData = ref groupMovePool.GetComponent(entity);

            // Получаем или создаём информацию о группе
            if (!groups.TryGetValue(groupData.groupId, out var groupInfo))
            {
                groupInfo = new GroupInfo();
                groups[groupData.groupId] = groupInfo;
                groupInfo.destination = groupData.destination;
            }

            // Обновляем информацию о лидере
            if (groupData.isGroupLeader)
            {
                UpdateLeaderInfo(entity, ref groupData, groupInfo);
            }

            // Управляем движением в зависимости от статуса лидера
            if (groupInfo.leaderReached)
            {
                // Лидер достиг цели - все должны остановиться
                HandleFollowerStop(entity, ref groupData, groupInfo);
            }
            else
            {
                // Лидер ещё идёт - двигаемся
                HandleFollowerMove(entity, ref groupData, groupInfo);
            }
        }

        private void UpdateLeaderInfo(int entity, ref GroupMoveData groupData, GroupInfo groupInfo)
        {
            groupInfo.leaderId = entity;
            groupInfo.destination = groupData.destination;

            // Проверяем, достиг ли лидер цели
            if (moveToPositionPool.HasComponent(entity))
            {
                ref var moveData = ref moveToPositionPool.GetComponent(entity);
                groupInfo.leaderReached = moveData.isReached;

                ref var transform = ref transformPool.GetComponent(entity);
                groupInfo.leaderDistanceToTarget = Vector3.Distance(transform.value.position, groupData.destination);
            }
        }

        private void HandleFollowerMove(int entity, ref GroupMoveData groupData, GroupInfo groupInfo)
        {
            // Если уже остановлен - пропускаем
            if (groupData.hasStopped) return;

            // Если нет компонента движения - добавляем
            if (!moveToPositionPool.HasComponent(entity))
            {
                moveToPositionPool.SetComponent(entity, new MoveToPositionData
                {
                    destination = groupData.destination,
                    stoppingDistance = groupData.waitDistance,
                    isReached = false
                });
            }
        }

        private void HandleFollowerStop(int entity, ref GroupMoveData groupData, GroupInfo groupInfo)
        {
            // Если уже остановлен - пропускаем
            if (groupData.hasStopped) return;

            ref var transform = ref transformPool.GetComponent(entity);
            float distanceToTarget = Vector3.Distance(transform.value.position, groupData.destination);

            // Останавливаемся если:
            // 1. Мы уже на дистанции ожидания
            // 2. ИЛИ мы очень близко к лидеру (если лидер уже у цели)
            bool shouldStop = distanceToTarget <= groupData.waitDistance;

            if (!shouldStop && groupInfo.leaderId != -1 && groupMovePool.HasComponent(groupInfo.leaderId))
            {
                ref var leaderGroupData = ref groupMovePool.GetComponent(groupInfo.leaderId);
                if (leaderGroupData.hasStopped)
                {
                    // Если лидер остановлен - останавливаемся на своей дистанции
                    shouldStop = true;
                }
            }

            if (shouldStop)
            {
                // Останавливаем движение
                moveToPositionPool.RemoveComponent(entity);
                moveStepPool.RemoveComponent(entity);

                groupData.hasStopped = true;
                groupMovePool.SetComponent(entity, groupData);

                Debug.Log($"Unit {entity} stopped at distance {distanceToTarget:F2}");
            }
            else if (!moveToPositionPool.HasComponent(entity))
            {
                // Продолжаем движение к цели
                moveToPositionPool.SetComponent(entity, new MoveToPositionData
                {
                    destination = groupData.destination,
                    stoppingDistance = groupData.waitDistance,
                    isReached = false
                });
            }
        }
    }
}