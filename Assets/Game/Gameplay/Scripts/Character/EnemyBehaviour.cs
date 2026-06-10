using System;
using System.Collections.Generic;
using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;

namespace SampleProject
{
    [RequireComponent(typeof(Entity))]
    public sealed class EnemyBehaviour : EntityBehaviour
    {
        protected override IEnumerable<IEcsSystem> ProvideSystems()
        {
            yield return new IdleStateMachine();

            yield return new CommandStateMachine(
                new CommandState_MoveToPosition(),
                new CommandState_AttackTarget(),
                new CommandState_PatrolByPoints()
            // НЕТ GatherResource - враги не собирают
            );

            yield return new CharacterAnimatorSystem();
            yield return new CharacterRigidbodySystem();

            // Система зрения для врага
            yield return new EnemyVisionSystem();
        }

        protected override IEnumerable<(Type, IEcsObserver)> ProvideObservers()
        {
            yield return (typeof(AnimatorEvent), new CharacterAnimatorObserver());
            yield return (typeof(HitEvent), new EnemyHitObserver()); // Враг получает урон
        }
    }
}