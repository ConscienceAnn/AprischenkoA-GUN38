using System;
using System.Collections.Generic;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [DefaultExecutionOrder(-2000)]
    [RequireComponent(typeof(Entity))]
    public abstract class EntityBehaviour : MonoBehaviour
    {
        private Entity entity;

        private readonly List<IEcsUpdate> updateSystems = new();
        private readonly List<IEcsFixedUpdate> fixedUpdateSystems = new();
        private readonly List<IEcsLateUpdate> lateUpdateSystems = new();
        
        private readonly List<(Type, IEcsObserver)> observers = new();

        protected abstract IEnumerable<IEcsSystem> ProvideSystems();
        protected abstract IEnumerable<(Type, IEcsObserver)> ProvideObservers();

        private void Awake()
        {
            this.entity = this.GetComponent<Entity>();
            var systems = this.ProvideSystems();
            this.RegisterSystems(systems);

            var observers = this.ProvideObservers();
            this.observers.AddRange(observers);
        }

        private void OnEnable()
        {
            EcsModule.OnUpdate += this.OnUpdate;
            EcsModule.OnFixedUpdate += OnFixedUpdate;
            EcsModule.OnLateUpdate += OnLateUpdate;
            this.SubscribeObservers();
        }

        private void OnDisable()
        {
            EcsModule.OnUpdate -= this.OnUpdate;
            EcsModule.OnFixedUpdate -= OnFixedUpdate;
            EcsModule.OnLateUpdate -= OnLateUpdate;
            this.UnsubscribeObservers();
        }

        private void OnUpdate()
        {
            if (this.entity.IsExists())
            {
                foreach (var state in this.updateSystems)
                {
                    state.Update(this.entity.Id);
                }
            }
        }

        private void OnFixedUpdate()
        {
            if (this.entity.IsExists())
            {
                foreach (var state in this.fixedUpdateSystems)
                {
                    state.FixedUpdate(this.entity.Id);
                }
            }
        }

        private void OnLateUpdate()
        {
            if (this.entity.IsExists())
            {
                foreach (var state in this.lateUpdateSystems)
                {
                    state.LateUpdate(this.entity.Id);
                }
            }
        }

        private void RegisterSystems(IEnumerable<object> systems)
        {
            var world = EcsModule.World;
            foreach (var system in systems)
            {
                world.Inject(system);

                if (system is IEcsUpdate update)
                {
                    this.updateSystems.Add(update);
                }

                if (system is IEcsFixedUpdate fixedUpdate)
                {
                    this.fixedUpdateSystems.Add(fixedUpdate);
                }

                if (system is IEcsLateUpdate lateUpdate)
                {
                    this.lateUpdateSystems.Add(lateUpdate);
                }
            }
        }
        
        private void SubscribeObservers()
        {
            var world = EcsModule.World;
            foreach (var (eventType, observer) in this.observers)
            {
                world.Subscribe(this.entity.Id, eventType, observer);
            }
        }
        private void UnsubscribeObservers()
        {
            var world = EcsModule.World;
            foreach (var (eventType, observer) in this.observers)
            {
                world.Unsubscribe(this.entity.Id, eventType, observer);
            }
        }
    }
}