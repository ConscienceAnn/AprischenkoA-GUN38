using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class PlayerHitObserver : IEcsObserver<HitEvent>
    {
        private EcsPool<HitPointsComponent> hpPool;
        private EcsPool<AnimatorComponent> animatorPool;

        void IEcsObserver<HitEvent>.Handle(int entity, HitEvent hitEvent)
        {
            if (!hpPool.HasComponent(entity)) return;

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

            // Проигрываем анимацию смерти
            if (animatorPool.HasComponent(entity))
            {
                ref var animator = ref animatorPool.GetComponent(entity);
                animator.value.ChangeState(5); // DEATH state
            }

            // Отключаем управление игроком
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