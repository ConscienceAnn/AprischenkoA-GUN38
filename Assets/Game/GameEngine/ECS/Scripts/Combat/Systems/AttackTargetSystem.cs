using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class AttackTargetSystem : IEcsFixedUpdate
    {
        private EcsPool<AttackTarget> targetPool;
        private EcsPool<HitRequest> hitRequestPool;
        private EcsPool<MoveToPositionData> moveToPositionPool;

        private EcsPool<GameObjectComponent> gameObjectPool;

        private EcsPool<CombatComponent> combatPool;
        private EcsPool<TransformComponent> transformPool;
        
        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.targetPool.HasComponent(entity))
            {
                return;
            }

            ref var targetId = ref this.targetPool.GetComponent(entity).targetId;

          
            // Проверяем существование цели
            if (!EcsModule.World.IsEntityExists(targetId))
            {
                this.targetPool.RemoveComponent(entity);
                this.hitRequestPool.RemoveComponent(entity);
                this.moveToPositionPool.RemoveComponent(entity);
                return;
            }

            // Проверяем наличие Transform у цели
            if (!this.transformPool.HasComponent(targetId))
            {
                this.targetPool.RemoveComponent(entity);
                return;
            }

            // Проверяем, жив ли GameObject цели
            if (gameObjectPool.HasComponent(targetId))
            {
                ref var go = ref gameObjectPool.GetComponent(targetId);
                if (go.value == null || !go.value.activeInHierarchy)
                {
                    this.targetPool.RemoveComponent(entity);
                    return;
                }
            }

            var myPosition = this.transformPool.GetComponent(entity).value.position;
            var targetPosition = this.transformPool.GetComponent(targetId).value.position;
            ref var minDistance = ref this.combatPool.GetComponent(entity).minDistance;

            if (Vector3.Distance(myPosition, targetPosition) <= minDistance)
            {
                this.moveToPositionPool.RemoveComponent(entity);
                this.hitRequestPool.SetComponent(entity, new HitRequest
                {
                    targetId = targetId
                });
            }
            else
            {
                this.hitRequestPool.RemoveComponent(entity);
                this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
                {
                    destination = targetPosition,
                    stoppingDistance = minDistance
                });
            }
        }
    }
}