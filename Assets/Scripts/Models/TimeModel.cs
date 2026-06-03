using System;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Models.Interfaces;
using UniRx;

namespace Models
{
    public sealed class TimeModel:  ITimeModel
    {
        private readonly ReactiveProperty<int> _gameTime = new();
        public IObservable<int> GameTime =>  _gameTime;

        public void Initialize() => CountTime().Forget();

        private async UniTask CountTime()
        {
            while (true)
            {
                await UniTask.Delay(NumericConstants.One * 1000);
                _gameTime.Value++;
            }
        }
    }
}