using GameECS;
using SampleProject.ResourceObject;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [CreateAssetMenu(
        fileName = "New Installer «Gather»",
        menuName = "Game/GameEngine/Ecs/New Installer «Gather»"
    )]
    public sealed class GatherInstaller : EcsInstaller
    {
        public override void Install(EcsWorld world)
        {
            world.DeclareComponent<GatherDuration>();
            world.DeclareComponent<ResourceBag>();
            world.DeclareComponent<GatherTarget>();
            world.DeclareComponent<GatherState>();
            world.DeclareComponent<BaseStorageComponent>();
            world.DeclareComponent<ResourceComponent>();

            world.DeclareSystem<GatherDurationSystem>();
            world.DeclareSystem<GatherResourceSystem>();
        }
    }
}