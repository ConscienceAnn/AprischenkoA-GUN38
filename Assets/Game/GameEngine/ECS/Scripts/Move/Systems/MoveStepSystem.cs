using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class MoveStepSystem : IEcsFixedUpdate
    {
        private EcsPool<MoveStepData> stepDataPool;
        private EcsPool<MoveSpeedComponent> speedPool;
        private EcsPool<NavMeshAgentComponent> agentPool;
        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<RigidbodyComponent> rigidbodyPool;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.stepDataPool.HasComponent(entity))
                return;

            ref var stepData = ref this.stepDataPool.GetComponent(entity);
            if (stepData.completed)
            {
                this.stepDataPool.RemoveComponent(entity);
                return;
            }

            this.UpdatePosition(entity);
            stepData.completed = true;
        }

        private void UpdatePosition(int entity)
        {
            if (!this.agentPool.HasComponent(entity))
                return;

            ref var agent = ref this.agentPool.GetComponent(entity).value;
            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);
            ref var speed = ref this.speedPool.GetComponent(entity);

            if (moveData.isReached)
            {
                if (agent.isActiveAndEnabled)
                {
                    agent.isStopped = true;        // Полная остановка
                    agent.ResetPath();              // Сброс пути
                    agent.velocity = Vector3.zero;  // Обнуление скорости
                }
                return;
            }

            // Если агент остановлен - возобновляем
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }

            // Настраиваем и двигаем
            agent.speed = speed.value;
            agent.SetDestination(moveData.destination);
        }
    }
}