using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class GatherDurationSystem : IEcsFixedUpdate
    {
        private EcsPool<GatherDuration> durationPool;
        private EcsPool<GatherTarget> targetResourcePool;
        private EcsPool<TransformComponent> transformPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.durationPool.HasComponent(entity))
                return;

            ref var duration = ref this.durationPool.GetComponent(entity);
            duration.remainingTime -= Time.fixedDeltaTime;

           
            if (this.targetResourcePool.HasComponent(entity))
            {
                ref var target = ref this.targetResourcePool.GetComponent(entity);
                this.RotateToResource(entity, target.targetId);
            }

            if (duration.remainingTime <= 0)
            {
                this.durationPool.RemoveComponent(entity);
            }
        }

        private void RotateToResource(int entity, int resourceId)
        {
            if (!this.transformPool.HasComponent(entity) || !this.transformPool.HasComponent(resourceId))
                return;

            ref var myTransform = ref this.transformPool.GetComponent(entity);
            ref var resourceTransform = ref this.transformPool.GetComponent(resourceId);

            Vector3 direction = (resourceTransform.value.position - myTransform.value.position).normalized;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.01f)
            {
                myTransform.value.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}