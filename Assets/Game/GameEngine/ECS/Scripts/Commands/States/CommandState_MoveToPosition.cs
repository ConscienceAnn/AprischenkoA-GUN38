using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_MoveToPosition : CommandState
    {
        private const float DEFAULT_STOPPING_DISTANCE = 0.2f;

        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<TransformComponent> transformPool;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.MOVE_TO_POSITION;
        }

        public override void Enter(int entity, object args)
        {
            Vector3 destination = (Vector3)args;

            // Если у юнита есть групповые данные - используем их дистанцию
            float stoppingDistance = DEFAULT_STOPPING_DISTANCE;

            // Проверяем, есть ли компонент группы
            var world = EcsModule.World;
            if (world.HasComponent<GroupMoveData>(entity))
            {
                ref var groupData = ref world.GetComponent<GroupMoveData>(entity);
                stoppingDistance = groupData.waitDistance;
            }

            this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                destination = destination,
                stoppingDistance = stoppingDistance,
                isReached = false
            });
        }

        public override void Update(int entity)
        {
            if (!this.moveToPositionPool.HasComponent(entity))
            {
                this.Complete(entity);
                return;
            }

            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);
            if (moveData.isReached)
            {
                this.Complete(entity);
            }
        }

        public override void Exit(int entity)
        {
            this.moveToPositionPool.RemoveComponent(entity);
        }
    }
}