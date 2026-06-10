using System.Collections.Generic;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_PatrolByPoints : CommandState
    {
        private const float STOPPING_DISTANCE = 1.2f;

        private EcsPool<TransformComponent> transformPool;
        private EcsPool<PatrolData> patrolPointsPool;
        private EcsPool<MoveToPositionData> moveToPositionPool;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.PATROL_BY_POINTS;
        }

        public override void Enter(int entity, object args)
        {
            var points = (List<Vector3>)args;

            this.patrolPointsPool.SetComponent(entity, new PatrolData
            {
                points = points,
                pointer = 0,
                stoppingDistance = STOPPING_DISTANCE
            });

            // СРАЗУ УСТАНАВЛИВАЕМ ПЕРВУЮ ТОЧКУ ДЛЯ ДВИЖЕНИЯ
            if (points != null && points.Count > 0)
            {
                this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
                {
                    destination = points[0],
                    stoppingDistance = STOPPING_DISTANCE,
                    isReached = false
                });
            }
        }

        // ДОБАВИТЬ ЭТОТ МЕТОД
        public override void Update(int entity)
        {
            // Проверяем, есть ли у юнита данные о патрулировании
            if (!this.patrolPointsPool.HasComponent(entity)) return;

            // Проверяем, есть ли данные о движении
            if (!this.moveToPositionPool.HasComponent(entity)) return;

            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);

            // Если цель ещё не достигнута - ничего не делаем
            if (!moveData.isReached) return;

            // Цель достигнута - переключаемся на следующую точку
            ref var patrolData = ref this.patrolPointsPool.GetComponent(entity);

            // Переходим к следующей точке
            patrolData.MoveNext();

            // Получаем новую цель
            var nextPoint = patrolData.GetCurrentPoint();

            // Устанавливаем новую цель для движения
            this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                destination = nextPoint,
                stoppingDistance = STOPPING_DISTANCE,
                isReached = false
            });

            Debug.Log($"Patrol: moving to point {patrolData.pointer}, position: {nextPoint}");
        }

        public override void Exit(int entity)
        {
            this.patrolPointsPool.RemoveComponent(entity);
            this.moveToPositionPool.RemoveComponent(entity);
        }
    }
}