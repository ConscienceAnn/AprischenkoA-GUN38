using System;
using System.Collections.Generic;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [DefaultExecutionOrder(-5000)]
    public class Entity : MonoBehaviour
    {
        private const int UNDEFINED = -1;

        public int Id
        {
            get { return this.id; }
        }

        private int id = UNDEFINED;

        private void OnEnable()
        {
            this.id = EcsModule.World.CreateEntity();
            this.Init();
        }

        private void OnDisable()
        {
            EcsModule.World.DestroyEntity(this.id);
            this.id = UNDEFINED;
        }

        protected virtual void Init()
        {
        }

        public bool IsExists()
        {
            return this.id >= 0;
        }

        public ref T GetData<T>() where T : struct
        {
            return ref EcsModule.World.GetComponent<T>(this.id);
        }

        public void SetData<T>(T component) where T : struct
        {
            EcsModule.World.SetComponent(this.id, ref component);
        }

        public void RemoveData<T>() where T : struct
        {
            EcsModule.World.RemoveComponent<T>(this.id);
        }

        public bool HasData<T>() where T : struct
        {
            return EcsModule.World.HasComponent<T>(this.id);
        }

        public void SendEvent<T>(T data) where T : struct
        {
            EcsModule.World.SendEvent<T>(this.id, data);
        }

        public void Subscribe<T>(Action<T> listener) where T : struct
        {
            EcsModule.World.Subscribe<T>(this.id, listener);
        }

        public void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            EcsModule.World.Unsubscribe<T>(this.id, listener);
        }

        public List<object> GetDataSet()
        {
            if (this.id != UNDEFINED)
            {
                return EcsModule.World.GetRawComponents(this.id);
            }

            return new List<object>();
        }
    }
}