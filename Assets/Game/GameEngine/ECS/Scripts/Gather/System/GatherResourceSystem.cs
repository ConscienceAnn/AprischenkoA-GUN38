using GameECS;
using SampleProject.Base;
using UnityEngine;
using SampleProject.ResourceObject;

namespace Game.GameEngine.Ecs
{
    public sealed class GatherResourceSystem : IEcsFixedUpdate
    {
        private const float GATHERING_DURATION = 5.0f;
        private const string RESOURCE_TYPE = "Minerals";
        private const int RESOURCE_AMOUNT = 5;

        private EcsPool<GatherTarget> targetResourcePool;
        private EcsPool<GatherState> gatherStatePool;
        private EcsPool<GatherDuration> gatherDurationPool;
        private EcsPool<ResourceBag> resourceBagPool;

        private EcsPool<MoveToPositionData> moveToPositionPool;
        private EcsPool<TransformComponent> transformPool;

        private EcsWorld world;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!this.targetResourcePool.HasComponent(entity))
            {
                return;
            }

            ref var state = ref this.gatherStatePool.GetComponent(entity);
            if (state == GatherState.MOVE_TO_RESOURCE)
            {
                this.UpdateMoveToResourceState(entity);
            }
            else if (state == GatherState.GATHERING)
            {
                this.UpdateGatheringState(entity);
            }
            else if (state == GatherState.MOVE_TO_HOME)
            {
                this.UpdateMoveToBaseState(entity);
            }
        }

        private void UpdateMoveToResourceState(int entity)
        {
            ref var resourceId = ref this.targetResourcePool.GetComponent(entity).targetId;
            if (!this.world.IsEntityExists(resourceId))
            {
                this.StopGathering(entity);
                return;
            }

            if (!this.moveToPositionPool.HasComponent(entity))
            {
                this.AddMoveToResourceData(entity);
            }

            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);
            if (!moveData.isReached)
            {
                return;
            }

            //Transitions:
            if (this.resourceBagPool.HasComponent(entity))
            {
                //Transit to MOVE_TO_BASE:
                this.SetMoveToHomeState(entity);
            }
            else
            {
                //Transit to GATHERING:
                this.SetGatheringState(entity);
            }
        }

        private void UpdateGatheringState(int entity)
        {
            ref var targetData = ref this.targetResourcePool.GetComponent(entity);
            int resourceId = targetData.targetId;

            if (!this.world.IsEntityExists(resourceId))
            {
                this.StopGathering(entity);
                return;
            }


            if (this.gatherDurationPool.HasComponent(entity))
            {
                return;
            }

    
            // ПЫТАЕМСЯ ПОЛУЧИТЬ ResourceComponent напрямую
            string resourceType = "Unknown";
            int resourceAmount = 5;

            // Проверяем, есть ли у ресурса компонент ResourceComponent
            if (this.world.HasComponent<ResourceComponent>(resourceId))
            {
                ref var resource = ref this.world.GetComponent<ResourceComponent>(resourceId);
                resourceType = resource.resourceType;
                resourceAmount = resource.resourceAmount;

                // УМЕНЬШАЕМ ЗАПАС РЕСУРСА
                resource.currentAmount -= resourceAmount;

                // ЕСЛИ РЕСУРС ИСЧЕРПАН - УНИЧТОЖАЕМ ЕГО
                if (resource.currentAmount <= 0)
                {
                    // Отправляем событие уничтожения ресурса
                    this.world.SendEvent<DestroyEvent>(resourceId, new DestroyEvent());
                    Debug.Log($"Resource {resourceType} depleted!");
                }


            }
            else
            {
                Debug.LogWarning($"Resource {resourceId} has no ResourceComponent, using defaults");
            }

            this.resourceBagPool.SetComponent(entity, new ResourceBag
            {
                resourceType = resourceType,
                resourceAmount = resourceAmount
            });

            //Transit to move base:
            this.SetMoveToHomeState(entity);
        }

        private void UpdateMoveToBaseState(int entity)
        {
            ref var moveData = ref this.moveToPositionPool.GetComponent(entity);
            if (!moveData.isReached)
            {
                return;
            }

            //Put resources to base...
            if (this.resourceBagPool.HasComponent(entity))
            {
                var gatherData = this.resourceBagPool.GetComponent(entity);

                // Найти базу
                var commandCenter = GameObject.FindObjectOfType<CommandCenterEntity>();
                if (commandCenter != null && commandCenter.HasData<BaseStorageComponent>())
                {
                    ref var storage = ref commandCenter.GetData<BaseStorageComponent>();

                    // Добавить ресурс в зависимости от типа
                    if (gatherData.resourceType == "Wood")
                        storage.wood += gatherData.resourceAmount;
                    else if (gatherData.resourceType == "Minerals")
                        storage.minerals += gatherData.resourceAmount;
                    else if (gatherData.resourceType == "Gold")
                        storage.gold += gatherData.resourceAmount;

                    Debug.Log($"Base: Wood={storage.wood}, Minerals={storage.minerals}, Gold={storage.gold}");
                }

                this.resourceBagPool.RemoveComponent(entity);
            }

            ref var resourceId = ref this.targetResourcePool.GetComponent(entity).targetId;

            if (!this.world.IsEntityExists(resourceId)) //TODO: Find other resource...
            {
                //COMPLETE GATHERING IF RESOURCE NOT FOUND!!!
                this.StopGathering(entity);
                return;
            }

            this.SetMoveToResourceState(entity);
        }

        private void SetMoveToResourceState(int entity)
        {
            this.gatherStatePool.SetComponent(entity, GatherState.MOVE_TO_RESOURCE);
            this.AddMoveToResourceData(entity);
        }

        private void AddMoveToResourceData(int entity)
        {
            ref var resourceId = ref this.targetResourcePool.GetComponent(entity).targetId;

            if (!this.world.IsEntityExists(resourceId))
            {
                this.StopGathering(entity);
                return;
            }

            if (!this.transformPool.HasComponent(resourceId))
            {
                this.StopGathering(entity);
                return;
            }

            ref var resourceTransform = ref this.transformPool.GetComponent(resourceId);

            float stopDistance = resourceTransform.radius + 0.5f;

            this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                destination = resourceTransform.value.position,
                stoppingDistance = resourceTransform.radius
            });
        }

        private void SetGatheringState(int entity)
        {
            ref var resourceId = ref this.targetResourcePool.GetComponent(entity).targetId;
            if (!this.world.IsEntityExists(resourceId))
            {
                this.StopGathering(entity);
                return;
            }

            this.gatherStatePool.SetComponent(entity, GatherState.GATHERING);

            this.gatherDurationPool.SetComponent(entity, new GatherDuration
            {
                remainingTime = GATHERING_DURATION
            });
        }

        private void SetMoveToHomeState(int entity)
        {
            this.gatherStatePool.SetComponent(entity, GatherState.MOVE_TO_HOME);

            //TODO: FIND COMMAND CENTER
            var commandCenter = GameObject.FindObjectOfType<CommandCenterEntity>();
            if (commandCenter == null)
            {
                //Command center is not found!
                this.StopGathering(entity);
                return;
            }

            ref var homeTransform = ref this.transformPool.GetComponent(commandCenter.Id);

            this.moveToPositionPool.SetComponent(entity, new MoveToPositionData
            {
                destination = homeTransform.value.position,
                stoppingDistance = homeTransform.radius
            });
        }

        private void StopGathering(int entity)
        {
            this.moveToPositionPool.RemoveComponent(entity);
            
            this.gatherStatePool.RemoveComponent(entity);
            this.targetResourcePool.RemoveComponent(entity);
            this.gatherDurationPool.RemoveComponent(entity);
        }
    }
}