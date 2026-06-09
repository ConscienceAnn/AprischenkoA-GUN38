using Game.GameEngine.Ecs;
using UnityEngine;

namespace SampleProject.Base
{
    public sealed class CommandCenterEntity : Entity
    {
        [SerializeField] private float collectionRadius = 3f;

        protected override void Init()
        {
            this.SetData(new TransformComponent
            {
                value = this.transform,
                radius = this.collectionRadius
            });

            // Хранилище ресурсов
            this.SetData(new ResourceStorageComponent
            {
                gold = 0,
                wood = 0,
                minerals = 0
            });
        }
    }

    public struct ResourceStorageComponent
    {
        public int gold;
        public int wood;
        public int minerals;
    }

}