using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class PlayerHitObserver : IEcsObserver<HitEvent>
    {
        private EcsPool<HitPointsComponent> hpPool;
        private EcsPool<AnimatorComponent> animatorPool;
        private EcsPool<GameObjectComponent> gameObjectPool;

        void IEcsObserver<HitEvent>.Handle(int entity, HitEvent hitEvent)
        {
            if (!hpPool.HasComponent(entity))
            {
                Debug.LogWarning($"Player {entity} has no HP component!");
                return;
            }

            ref var hp = ref hpPool.GetComponent(entity);
            hp.current -= hitEvent.damage;

            Debug.Log($"Player took {hitEvent.damage} damage! HP: {hp.current}/{hp.max}");

            if (hp.current <= 0)
            {
                Die(entity);
            }
        }

        private void Die(int entity)
        {
            Debug.Log($"Player {entity} died!");

            // Отключаем GameObject
            if (gameObjectPool.HasComponent(entity))
            {
                ref var go = ref gameObjectPool.GetComponent(entity);
                if (go.value != null)
                {
                    go.value.SetActive(false);
                }
            }

            // Удаляем компоненты
            var world = EcsModule.World;
            if (world.HasComponent<CommandRequest>(entity))
            {
                world.RemoveComponent<CommandRequest>(entity);
            }

            // Отправляем событие уничтожения
            world.SendEvent(entity, new DestroyEvent());
        }
    }
}