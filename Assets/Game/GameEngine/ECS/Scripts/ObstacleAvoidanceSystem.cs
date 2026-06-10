using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class ObstacleAvoidanceSystem : IEcsFixedUpdate
    {
        private EcsPool<MoveStepData> stepPool;
        private EcsPool<TransformComponent> transformPool;
        private EcsPool<MoveSpeedComponent> speedPool;
        private EcsPool<RigidbodyComponent> rigidbodyPool;

        private const float RAY_DISTANCE = 1.5f;
        private const float AVOID_FORCE = 0.7f;
        private const int RAY_COUNT = 5;
        private const float RAY_ANGLE = 45f;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!stepPool.HasComponent(entity)) return;

            ref var step = ref stepPool.GetComponent(entity);
            if (step.completed) return;

            ref var transform = ref transformPool.GetComponent(entity);
            Vector3 currentPos = transform.value.position;
            Vector3 originalDirection = step.direction;

            // Мульти-луч для плавного обхода
            Vector3 avoidDirection = CalculateAvoidDirection(currentPos, originalDirection);

            if (avoidDirection != originalDirection)
            {
                step.direction = Vector3.Lerp(originalDirection, avoidDirection, AVOID_FORCE).normalized;
                stepPool.SetComponent(entity, step);
            }
        }

        private Vector3 CalculateAvoidDirection(Vector3 position, Vector3 desiredDirection)
        {
            Vector3 right = Vector3.Cross(desiredDirection, Vector3.up).normalized;

            // Проверяем несколько направлений
            float[] angles = { 0f, RAY_ANGLE, -RAY_ANGLE, RAY_ANGLE * 1.5f, -RAY_ANGLE * 1.5f };
            float[] weights = { 1f, 0.7f, 0.7f, 0.3f, 0.3f };

            Vector3 bestDirection = desiredDirection;
            float bestWeight = 0f;

            for (int i = 0; i < RAY_COUNT; i++)
            {
                Quaternion rotation = Quaternion.Euler(0, angles[i], 0);
                Vector3 rayDirection = rotation * desiredDirection;

                if (!Physics.Raycast(position, rayDirection, RAY_DISTANCE,
                    LayerMask.GetMask("Obstacle", "Unit")))
                {
                    float weight = weights[i];
                    if (weight > bestWeight)
                    {
                        bestWeight = weight;
                        bestDirection = rayDirection;
                    }
                }
            }

            // Отладочная визуализация
            Debug.DrawRay(position, desiredDirection * RAY_DISTANCE, Color.green);
            Debug.DrawRay(position, bestDirection * RAY_DISTANCE, Color.red);

            return bestDirection;
        }
    }
}