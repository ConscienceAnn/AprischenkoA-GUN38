using System;
using Core.Utils;
using DG.Tweening;
using Messages;
using Messages.Input;
using Models.Interfaces;
using Presenters.Interfaces;
using UniRx;
using UnityEngine;
using Zenject;

namespace Presenters
{
    [Serializable]
    public sealed class GameLoopPresenter : IGameLoopPresenter
    {
        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly IBuildingModel _buildingModel;
        private readonly IPlayerModel _playerModel;
        private readonly IStickModel _stickModel;
        private readonly IGameScoreModel _gameScoreModel;
        private readonly IMessageBroker _messageBroker;
        private readonly Vector2 _startPosition;
        private Vector2 _currentStickPosition;
        private int _gameIterations;
        private Camera _camera;

        public GameLoopPresenter(
            IBuildingModel buildingModel, 
            IPlayerModel playerModel, 
            IStickModel stickModel,
            IGameScoreModel gameScoreModel,
            IMessageBroker broker,
            [Inject(Id = "Start")] Transform startPosition)
        {
            _buildingModel = buildingModel;
            _playerModel = playerModel;
            _stickModel = stickModel;
            _gameScoreModel = gameScoreModel;
            _messageBroker = broker;
            _startPosition = startPosition.position;
        }

        public void Initialize()
        {
            Subscribe();
            InitializeObjectsAtStart();
            _gameScoreModel.ResetScore();
        }

        public void Dispose() => Unsubscribe();

        private void InitializeObjectsAtStart()
        {
            _buildingModel.SetNextBuildingPosition(new Vector2(_startPosition.x - Constants.BuildingPositionDelta, _startPosition.y));

            _playerModel.MovePlayer(_buildingModel.GetPositionForPlayer(true));
            _playerModel.EnablePlayer();

            _buildingModel.SetNextBuildingPosition(new Vector2(_startPosition.x + Constants.BuildingPositionDelta, _startPosition.y));
            SetStickPosition();

            _gameIterations = NumericConstants.One;
            _camera = Camera.main;
        }
        
        private void SetStickPosition()
        {
            _currentStickPosition = _buildingModel.CalculateStartPositionForStick();
            _stickModel.ResetStick(_currentStickPosition);
        }

        private void Subscribe()
        {
            _messageBroker.Receive<StickFallCompletedMessage>().Subscribe(OnStickFallCompleted).AddTo(_compositeDisposable);
            _messageBroker.Receive<MoveFailedMessage>().Subscribe(OnMoveFailed).AddTo(_compositeDisposable);
            _messageBroker.Receive<MoveSuccessfulMessage>().Subscribe(OnMoveSuccessfull).AddTo(_compositeDisposable);
        }

        private void Unsubscribe() => _compositeDisposable.Dispose();

        private void OnStickFallCompleted(StickFallCompletedMessage message)
        {
            if (IsStickInBuildingPosition())
            {
                _playerModel.MovePlayer(_buildingModel.GetPositionForPlayer(false), true);
            }
            else
            {
                var playerPosition = _buildingModel.GetPositionForPlayer(true);
                _playerModel.MovePlayer( playerPosition + Vector2.right * (_stickModel.GetStickLength() + (_currentStickPosition.x - playerPosition.x)), true, true);
            }
        }

        private bool IsStickInBuildingPosition()
        {
            var stickLength = _currentStickPosition.x + _stickModel.GetStickLength();
            var nextBuildingPositionRange = _buildingModel.GetNextBuildingPositionRange();
            return stickLength >= nextBuildingPositionRange.min && stickLength <= nextBuildingPositionRange.max;
        }

        private void OnMoveFailed(MoveFailedMessage message)
        {
            _playerModel.DisablePlayer();
            _stickModel.DisableStick();
        }

        private void OnMoveSuccessfull(MoveSuccessfulMessage message) => SetNextGameIteration();

        private void SetNextGameIteration()
        {
            _gameIterations+= Constants.BuildingPositionDelta;
            _gameScoreModel.IncreaseScore();
            _buildingModel.SetNextBuildingPosition(
                new Vector2(_startPosition.x + Constants.BuildingPositionDelta * _gameIterations, _startPosition.y));
            _stickModel.DisableStick();
            SetStickPosition();

            _camera.transform
                .DOMove(CalculateNewCameraPosition(), NumericConstants.Half)
                .OnComplete(() => _messageBroker.Publish(new SetInputActiveStateMessage { IsActive = true }))
                .Play();
            
            Vector3 CalculateNewCameraPosition() =>
                new(_startPosition.x + Constants.BuildingPositionDelta * _gameIterations - Constants.BuildingPositionDelta,
                    _camera.transform.position.y,
                    _camera.transform.position.z);
        }
    }
}