using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class MoveStepSystem : IEcsFixedUpdate
    {
        private readonly EcsPool<MoveStepData> stepDataPool;
        private readonly EcsPool<MoveSpeedComponent> speedPool;
        private readonly EcsPool<RigidbodyComponent> rigidbodyPool;

        private readonly EcsEmitter<SmoothRotateEvent> rotateEmitter;

        private EcsPool<TransformComponent> transformPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.stepDataPool.HasComponent(entity))
            {
                return;
            }

            ref var stepData = ref this.stepDataPool.GetComponent(entity);
            if (stepData.completed)
            {
                this.stepDataPool.RemoveComponent(entity);
                return;
            }

            this.UpdatePosition(entity, stepData.direction);
            this.UpdateRotation(entity, stepData.direction);

            stepData.completed = true;
        }

        private void UpdatePosition(int entity, Vector3 direction)
        {
            ref var rigidbody = ref this.rigidbodyPool.GetComponent(entity).value;
            ref var moveSpeed = ref this.speedPool.GetComponent(entity).value;

            Vector3 finalDirection = AvoidObstacles(entity, direction);

            var moveStep = direction * moveSpeed * Time.fixedDeltaTime;
            var newPosition = rigidbody.position + moveStep;
            rigidbody.MovePosition(newPosition);
        }

        private Vector3 AvoidObstacles(int entity, Vector3 desiredDirection)
        {
            if (!this.transformPool.HasComponent(entity)) return desiredDirection;

            ref var transformComp = ref this.transformPool.GetComponent(entity);
            Vector3 currentPos = transformComp.value.position;

            if (Physics.Raycast(currentPos, desiredDirection, out RaycastHit hit, 1.2f))
            {
                if (hit.collider.CompareTag("Obstacle") || hit.collider.CompareTag("Unit"))
                {
                    Vector3 avoidDirection = Vector3.Cross(desiredDirection, Vector3.up);
                    return avoidDirection.normalized;
                }
            }
            return desiredDirection;
        }

        private void UpdateRotation(int entity, Vector3 direction)
        {
            this.rotateEmitter.SendEvent(entity, new SmoothRotateEvent
            {
                direction = direction
            });
        }
    }
}