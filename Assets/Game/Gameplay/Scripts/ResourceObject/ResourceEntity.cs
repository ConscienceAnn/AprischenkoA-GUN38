using Game.GameEngine.Ecs;
using UnityEngine;

namespace SampleProject.ResourceObject
{
    public sealed class ResourceEntity : Entity
    {
        [Header("Resource Settings")]
        [SerializeField] private string resourceType = "Wood"; // Wood, Minerals, Gold
        [SerializeField] private int resourceAmount = 10;
        [SerializeField] private int maxHitPoints = 50;

        [Header("Collection Settings")]
        [SerializeField] private Transform collectionPoint;
        [SerializeField] private float collectionRadius = 1.5f; // Радиус взаимодействия

        protected override void Init()
        {

            Transform targetTransform = this.collectionPoint != null
         ? this.collectionPoint
         : this.transform;

            this.SetData(new TransformComponent
            {
                value = targetTransform, 
                radius = this.collectionRadius
            });


            // Компонент ресурса (для Gather системы)
            this.SetData(new ResourceComponent
                {
                    resourceType = this.resourceType,
                    resourceAmount = this.resourceAmount,
                    currentAmount = this.maxHitPoints
                });

                // HitPoints (чтобы ресурс можно было "уничтожить" или он не бесконечный)
                this.SetData(new HitPointsComponent
                {
                    max = this.maxHitPoints,
                    current = this.maxHitPoints
                });

                // GameObject компонент
                this.SetData(new GameObjectComponent
                {
                    value = this.gameObject
                });
            
        }

        // Метод для уменьшения ресурса при сборе
        public bool GatherResource(int amount)
        {
            if (this.HasData<ResourceComponent>())
            {
                ref var resource = ref this.GetData<ResourceComponent>();
                if (resource.currentAmount >= amount)
                {
                    resource.currentAmount -= amount;
                    return true;
                }
            }
            return false;
        }
    }


    // Компонент для хранения информации о ресурсе
    public struct ResourceComponent
    {
        public string resourceType;
        public int resourceAmount;    // Сколько даёт за один сбор
        public int currentAmount;     // Осталось ресурса
    }

   
}