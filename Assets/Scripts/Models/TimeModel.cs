using System;
using System.Threading;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Models.Interfaces;
using UniRx;

namespace Models
{
    public sealed class TimeModel : ITimeModel
    {
        private readonly ReactiveProperty<int> _gameTime = new();
        private CancellationTokenSource _cts;
        public IObservable<int> GameTime => _gameTime;

        public void Initialize()
        {
            _cts = new CancellationTokenSource();  // создаем "пульт управления"
            CountTime(_cts.Token).Forget();        // запускаем таймер с возможностью отмены
        }

        private async UniTask CountTime(CancellationToken token)
        {
            while (true)
            {
                //если просят остановиться - выходим
                if (token.IsCancellationRequested)
                {
                    break;
                }
                await UniTask.Delay(NumericConstants.One * 1000, cancellationToken: token);

                // во время ожидания могли попросить остановиться
                if (token.IsCancellationRequested)
                {
                    break;
                }

                // Если дошли сюда - все ок, увеличиваем время
                _gameTime.Value++;
            }
        }

        public void Dispose()
        {
            // Проверяем, существует ли "пульт управления"
            if (_cts != null)
            {
                _cts.Cancel();   // Нажимаем кнопку "СТОП"
                _cts.Dispose();  // Убираем батарейки (освобождаем ресурсы)
                _cts = null;     // Забываем про пульт
            }
        }
    }
}