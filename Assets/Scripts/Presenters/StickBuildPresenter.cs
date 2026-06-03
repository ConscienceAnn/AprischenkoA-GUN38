using System;
using Messages.Input;
using Models.Interfaces;
using Presenters.Interfaces;
using UniRx;
using Zenject;

namespace Presenters
{
    [Serializable]
    public sealed class StickBuildPresenter : IStickBuildPresenter
    {
        [Inject] private IMessageBroker _messageBroker; 
        private readonly IStickModel _stickModel;
        private readonly CompositeDisposable _compositeDisposable = new();
                private bool _isBuildMode;

        public StickBuildPresenter(IStickModel stickModel) => _stickModel = stickModel;

        public void Initialize()
        {
            _messageBroker.Receive<InputStartedMessage>().Subscribe(OnInputStarted).AddTo(_compositeDisposable);
            _messageBroker.Receive<InputHoldMessage>().Subscribe(OnInputHold).AddTo(_compositeDisposable);
            _messageBroker.Receive<InputFinishedMessage>().Subscribe(OnInputFinished).AddTo(_compositeDisposable);
        }

        public void Dispose() => _compositeDisposable.Dispose();

        public void OnInputStarted(InputStartedMessage message)
        {
            _isBuildMode = true;
            _stickModel.EnableStick();
        }

        public void OnInputHold(InputHoldMessage message)
        {
            if (_isBuildMode)
            {
                _stickModel.RaiseStick();
            }
        }

        public void OnInputFinished(InputFinishedMessage message)
        {
            if (!_isBuildMode)
            {
                return;
            }
            _messageBroker.Publish(new SetInputActiveStateMessage {IsActive = false});
            _isBuildMode = false;
            _stickModel.FallStick();
        }
    }
}