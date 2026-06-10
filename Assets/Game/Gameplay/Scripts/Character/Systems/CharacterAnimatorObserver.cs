using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class CharacterAnimatorObserver : IEcsObserver<AnimatorEvent>
    {
        private const string ATTACK_MESSAGE = "attack";

        private EcsPool<HitRequest> requestPool;
        private EcsPool<CombatComponent> attackComponentPool;
        private EcsPool<HitPointsComponent> hitPointsPool; 
        private EcsEmitter<HitEvent> hitEmitter;

        void IEcsObserver<AnimatorEvent>.Handle(int entity, AnimatorEvent @event)
        {
            if (@event.message == ATTACK_MESSAGE)
            {
                Debug.Log("ATTACK!");
                this.Attack(entity);
            }
        }

        private void Attack(int entity)
        {
            if (!requestPool.HasComponent(entity))
            {
                return;
            }

            ref var request = ref requestPool.GetComponent(entity);
            int targetId = request.targetId;

            if (!EcsModule.World.IsEntityExists(targetId))
            {
                requestPool.RemoveComponent(entity);
                return;
            }

            if (!hitPointsPool.HasComponent(targetId))
            {
                requestPool.RemoveComponent(entity);
                return;
            }


            ref var hp = ref hitPointsPool.GetComponent(targetId);
            if (hp.current <= 0)
            {
                requestPool.RemoveComponent(entity);
                return;
            }

            ref var component = ref attackComponentPool.GetComponent(entity);

            Debug.Log($"Entity {entity} deals {component.damage} damage to target {targetId}");

            hitEmitter.SendEvent(targetId, new HitEvent
            {
                targetId = targetId,
                damage = component.damage,
                damageType = component.damageType
            });


            requestPool.RemoveComponent(entity);
        }
    }
}