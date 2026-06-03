using Core.Utils;
using DG.Tweening;
using Messages;
using Models.Interfaces;
using UniRx;
using UnityEngine;
using Zenject;

namespace Models
{
    public sealed class PlayerModel : IPlayerModel
    {
        private readonly MoveFailedMessage _moveFailedMessage = new();
        private readonly MoveSuccessfulMessage _moveSuccessfulMessage = new();
        private readonly GameObject _playerPrefab;
        private GameObject _playerObject;
        private IMessageBroker _messageBroker;

        public PlayerModel([Inject(Id = Constants.PlayerIdentifier)] GameObject playerObject, IMessageBroker broker)
        {
            _playerPrefab = playerObject;
            _messageBroker = broker;
        }

        public void Initialize()
        {
            _playerObject = Object.Instantiate(_playerPrefab);
            _playerObject.SetActive(false);
        }

        public void EnablePlayer() => _playerObject.SetActive(true);

        public void DisablePlayer() => _playerObject.SetActive(false);

        public void MovePlayer(Vector2 position, bool animate = false, bool fall = false)
        {
            if (!animate)
            {
                _playerObject.transform.position = position;
                return;
            }
            var sequence = DOTween.Sequence(this);
            sequence.Append(_playerObject.transform.DOMove(position, NumericConstants.Half));
            
            if (fall)
            {
                sequence.Append(_playerObject.transform.DOMove(position + Vector2.down * 5, NumericConstants.Half));
                sequence.AppendCallback((() => _messageBroker.Publish(_moveFailedMessage)));
                
            }
            else
            {
                sequence.AppendCallback((() => _messageBroker.Publish(_moveSuccessfulMessage)));
                
            }
            sequence.Play();
        }
    }
}