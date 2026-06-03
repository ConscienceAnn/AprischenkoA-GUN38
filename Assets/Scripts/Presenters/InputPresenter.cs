using System;
using Cysharp.Threading.Tasks.Linq;
using Messages.Input;
using Presenters.Interfaces;
using UniRx;
using UnityEngine.EventSystems;
using Input = UnityEngine.Input;

namespace Presenters
{
    [Serializable]
    public sealed class InputPresenter : IInputPresenter
    {
        private readonly InputStartedMessage _inputStartedMessage = new();
        private readonly InputHoldMessage _inputHoldMessage = new();
        private readonly InputFinishedMessage _inputFinishedMessage = new();
        private readonly CompositeDisposable _inputDisposable = new();
        private readonly IMessageBroker _messageBroker;
        private bool _enabled;
        private IDisposable _messageDispose;

        public InputPresenter(IMessageBroker messageBroker) => _messageBroker = messageBroker;

        public void Initialize()
        {
            Input.multiTouchEnabled = false;
            _enabled = true;
            _messageDispose = _messageBroker.Receive<SetInputActiveStateMessage>().Subscribe(OnSetInputActive);

            var inputStream = UniTaskAsyncEnumerable //TODO Аналогично Observable
                .EveryUpdate()
                .Where(_ => _enabled && !EventSystem.current.IsPointerOverGameObject());

            inputStream
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => _messageBroker.Publish(_inputStartedMessage))
                .AddTo(_inputDisposable);

            inputStream
                .Where(_ => Input.GetMouseButton(0))
                .Subscribe(_ => _messageBroker.Publish(_inputHoldMessage))
                .AddTo(_inputDisposable);

            inputStream
                .Where(_ => Input.GetMouseButtonUp(0))
                .Subscribe(_ => _messageBroker.Publish(_inputFinishedMessage))
                .AddTo(_inputDisposable);
        }

        public void Dispose()
        {
            _messageDispose.Dispose();
            _inputDisposable.Dispose();
        }

        private void OnSetInputActive(SetInputActiveStateMessage message) => _enabled = message.IsActive;
    }
}