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
            Debug.Log($"Entity {entity} died!");

            if (animatorPool.HasComponent(entity))
            {
                ref var animator = ref animatorPool.GetComponent(entity);
                if (animator.value != null)
                {
                    animator.value.ChangeState(5);

                    if (gameObjectPool.HasComponent(entity))
                    {
                        ref var go = ref gameObjectPool.GetComponent(entity);
                        if (go.value != null)
                        {
                            go.value.GetComponent<Entity>().StartCoroutine(DestroyAfterAnimation(go.value, animator.value));
                        }
                    }
                }
            }
        }

        private System.Collections.IEnumerator DestroyAfterAnimation(GameObject obj, AnimatorMachine animator)
        {
            yield return new WaitForSeconds(2f);

            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
        }
    }
}