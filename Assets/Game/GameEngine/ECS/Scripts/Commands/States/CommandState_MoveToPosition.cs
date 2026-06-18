using GameECS;
using UnityEngine;
using UnityEngine.AI;

namespace Game.GameEngine.Ecs
{
    public sealed class CommandState_MoveToPosition : CommandState
    {
        private const float DEFAULT_STOPPING_DISTANCE = 0.5f;

        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<TransformComponent> transformPool;

        public override bool MatchesType(CommandType type)
        {
            return type is CommandType.MOVE_TO_POSITION;
        }

        public override void Enter(int entity, object args)
        {
            Vector3 destination = (Vector3)args;

            // ===== —Ã≈Ÿ≈Õ»≈ “Œ◊ » =====
            float angle = (entity * 137) % 360;
            float radius = 0.5f + (entity % 3) * 0.3f;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * radius;

            Vector3 finalDestination = destination + offset;

            if (NavMesh.SamplePosition(finalDestination, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                finalDestination = hit.position;
            }

           
            float stoppingDistance = DEFAULT_STOPPING_DISTANCE;

            var world = EcsModule.World;
            if (world.HasComponent<GroupMoveData>(entity))
            {
                ref var groupData = ref world.GetComponent<GroupMoveData>(entity);
                stoppingDistance = Mathf.Max(DEFAULT_STOPPING_DISTANCE, groupData.waitDistance);
            }

            this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                destination = finalDestination,
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