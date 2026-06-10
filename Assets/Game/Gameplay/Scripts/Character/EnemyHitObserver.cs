using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class EnemyHitObserver : IEcsObserver<HitEvent>
    {
        private EcsPool<HitPointsComponent> hpPool;
        private EcsPool<AnimatorComponent> animatorPool;
        private EcsPool<GameObjectComponent> gameObjectPool;

        void IEcsObserver<HitEvent>.Handle(int entity, HitEvent hitEvent)
        {
            if (!hpPool.HasComponent(entity))
            {
                Debug.LogWarning($"Enemy {entity} has no HP component!");
                return;
            }

            ref var hp = ref hpPool.GetComponent(entity);
            hp.current -= hitEvent.damage;

            Debug.Log($"Enemy took {hitEvent.damage} damage! HP: {hp.current}/{hp.max}");

            if (hp.current <= 0)
            {
                Die(entity);
            }
        }

        private void Die(int entity)
        {
            Debug.Log($"Enemy {entity} died!");

            if (animatorPool.HasComponent(entity))
            {
                ref var animator = ref animatorPool.GetComponent(entity);
                animator.value.ChangeState(5); // DEATH state
            }

            var world = EcsModule.World;
            if (world.HasComponent<CommandRequest>(entity))
            {
                world.RemoveComponent<CommandRequest>(entity);
            }

            // ”ничтожаем через 2 секунды (дл€ анимации смерти)
            if (gameObjectPool.HasComponent(entity))
            {
                ref var go = ref gameObjectPool.GetComponent(entity);
                GameObject.Destroy(go.value, 2f);
            }

            world.SendEvent(entity, new DestroyEvent());
        }
    }
}