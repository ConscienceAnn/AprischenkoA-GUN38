using Core.Utils;
using DG.Tweening;
using Messages;
using Models.Interfaces;
using UniRx;
using UnityEngine;
using Zenject;

namespace Models
{
    public sealed class StickModel : IStickModel
    {
        [Inject] private IMessageBroker _messageBroker;
        private const float StickIncreaseDelta = 0.1f;
        private const float StickMaxLength = 5;
        private readonly StickFallCompletedMessage _stickFallCompleted = new();
        private readonly GameObject _stickPrefab;
        private readonly Vector3 _defaultStickScale = new(1, 0.5f, 1);
        private GameObject _stickGameObject;
        private Vector3 _defaultScale;

        public StickModel([Inject(Id = "Stick")] GameObject stickPrefab) => _stickPrefab = stickPrefab;

        public void Initialize()
        {
            _stickGameObject = Object.Instantiate(_stickPrefab);
            _stickGameObject.SetActive(false);
        }

        public void EnableStick() => _stickGameObject.SetActive(true);

        public void DisableStick() => _stickGameObject.SetActive(false);

        public void RaiseStick()
        {
            _defaultScale += Vector3.up * StickIncreaseDelta;
            if (_defaultScale.y >= StickMaxLength)
            {
                _defaultScale.y = StickMaxLength;
            }
            _stickGameObject.transform.localScale = _defaultScale;
        }

        public void ResetStick(Vector2 newPosition)
        {
            _stickGameObject.transform.position = newPosition;
            _stickGameObject.transform.rotation = Quaternion.identity;
            _defaultScale = _defaultStickScale;
        }

        public float GetStickLength() => _stickGameObject.transform.localScale.y;
        
        public void FallStick()
        {

            _stickGameObject.transform
                .DORotate(new Vector3(0, 0, -NumericConstants.NinetyDegrees), NumericConstants.Half)
                .OnComplete((() => _messageBroker.Publish(_stickFallCompleted)))
                .Play(); 
        }
    }
}