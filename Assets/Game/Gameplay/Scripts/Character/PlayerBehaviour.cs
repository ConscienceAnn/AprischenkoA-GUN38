using System;
using System.Collections.Generic;
using Game.GameEngine.Ecs;
using GameECS;
using UnityEngine;

namespace SampleProject
{
    [RequireComponent(typeof(Entity))]
    public sealed class PlayerBehaviour : EntityBehaviour
    {
        protected override IEnumerable<IEcsSystem> ProvideSystems()
        {
            yield return new IdleStateMachine();

            yield return new CommandStateMachine(
                new CommandState_MoveToPosition(),
                new CommandState_AttackTarget(),
                new CommandState_PatrolByPoints(),
                new CommandState_GatherResource()
            );

            yield return new CharacterAnimatorSystem();
            yield return new CharacterRigidbodySystem();

            // —истема автоматического обнаружени€ врагов
            yield return new PlayerVisionSystem();
        }

        protected override IEnumerable<(Type, IEcsObserver)> ProvideObservers()
        {
            yield return (typeof(AnimatorEvent), new CharacterAnimatorObserver());
            yield return (typeof(HitEvent), new PlayerHitObserver());
        }
    }
}