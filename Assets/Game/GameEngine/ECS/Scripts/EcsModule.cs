using System;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [DefaultExecutionOrder(-10000)]
    public sealed class EcsModule : MonoBehaviour
    {
        public static event Action OnUpdate;
        public static event Action OnFixedUpdate;
        public static event Action OnLateUpdate;
        
        private static EcsWorld world;

        [SerializeField]
        private EcsInstaller[] installers;

        private bool isInstalled;
        
        public static EcsWorld World
        {
            get { return world; }
        }

        private void Awake()
        {
            world = new EcsWorld();
            
            foreach (var installer in this.installers)
            {
                installer.Install(world);
            }

            world.ResolveDependencies();
            this.isInstalled = true;
        }

        private void Update()
        {
            world.Update();
            OnUpdate?.Invoke();
        }

        private void FixedUpdate()
        {
            world.FixedUpdate();
            OnFixedUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            world.LateUpdate();
            OnLateUpdate?.Invoke();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (this.isInstalled)
            {
                world.OnDrawGizmos();
            }
        }
#endif
    }
}