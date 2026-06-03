using Core.SaveLoad;
using Core.Utils;
using Models;
using UniRx;
using UnityEngine;
using Zenject;

namespace Installers
{
    public sealed class ProjectInstaller : MonoInstaller
    {
        public override void Start() => Application.targetFrameRate = NumericConstants.FPS;

        public override void InstallBindings()
        {
            Container.Bind<IMessageBroker>().FromInstance(MessageBroker.Default);
            Container
                .Bind<ISaveLoadDataHandler>()
                .To<PlayerPrefsSaveLoadDataHandler>()
                .AsSingle();
            Container
                .BindInterfacesAndSelfTo<GameScoreModel>()
                .AsSingle();
        }
    }
}