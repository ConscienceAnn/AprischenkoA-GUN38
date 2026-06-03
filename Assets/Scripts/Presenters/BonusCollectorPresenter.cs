using System;
using System.Collections.Generic;
using Messages;
using Models;
using Models.Interfaces;
using UniRx;
using UnityEngine;
using View;
using Zenject;

namespace Presenters
{
    /// <summary>
    /// Отвечает за сбор бонусов игроком
    /// </summary>
    public sealed class BonusCollectorPresenter : IInitializable, IDisposable
    {
        private readonly CompositeDisposable _disposable = new();
        private readonly IBonusModel _bonusModel;
        private readonly IPlayerModel _playerModel;
        private readonly IMessageBroker _messageBroker;

        private GameObject _playerObject;
        private bool _isMoving = false;

        // ВРЕМЕННОЕ ХРАНИЛИЩЕ: бонусы, собранные во время движения
        private readonly List<BonusType> _pendingBonuses = new List<BonusType>();

        public BonusCollectorPresenter(IBonusModel bonusModel, IPlayerModel playerModel, IMessageBroker messageBroker)  
        {
            _bonusModel = bonusModel;
            _playerModel = playerModel;
            _messageBroker = messageBroker; 
        }

        public void Initialize()
        {
            // Ищем игрока на сцене
            _playerObject = GameObject.FindGameObjectWithTag("Player");

            // Каждый кадр проверяем бонусы ТОЛЬКО во время движения
            Observable.EveryUpdate()
                .Where(_ => _isMoving && _playerObject != null)
                .Subscribe(_ => CheckForBonuses())
                .AddTo(_disposable);

            // КОГДА ИГРОК НАЧИНАЕТ ДВИЖЕНИЕ (после падения стика)
            _messageBroker.Receive<StickFallCompletedMessage>()
                .Subscribe(_ =>
                {
                    _isMoving = true;
                    _pendingBonuses.Clear();  // очищаем временное хранилище перед новым движением
                    Debug.Log("[BonusCollector] Игрок начал движение");
                })
                .AddTo(_disposable);

            // КОГДА ИГРОК УСПЕШНО ПРИЗЕМЛИЛСЯ - засчитываем бонусы
            _messageBroker.Receive<MoveSuccessfulMessage>()
                .Subscribe(_ =>
                {
                    _isMoving = false;
                    // Засчитываем все собранные бонусы за это движение
                    foreach (var bonus in _pendingBonuses)
                    {
                        _bonusModel.AddBonus(bonus);
                        Debug.Log($"[BonusCollector] Засчитан бонус: {bonus}");
                    }
                    _pendingBonuses.Clear();
                    Debug.Log($"[BonusCollector] Игрок приземлился, бонусы засчитаны");
                })
                .AddTo(_disposable);

            // КОГДА ИГРОК УПАЛ - НЕ засчитываем бонусы!
            _messageBroker.Receive<MoveFailedMessage>()
                .Subscribe(_ =>
                {
                    _isMoving = false;
                    // Просто очищаем временное хранилище, НЕ добавляя бонусы!
                    _pendingBonuses.Clear();
                   _bonusModel.ClearBonuses();  // очищаем бонусы текущей сессии
                    Debug.Log("[BonusCollector] Игрок упал, бонусы НЕ засчитаны и очищены");
                })
                .AddTo(_disposable);
        }

        private void CheckForBonuses()
        {
            if (_playerObject == null) return;

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(_playerObject.transform.position, 0.5f);

            foreach (var collider in hitColliders)
            {
                var bonusView = collider.GetComponent<BonusView>();
                if (bonusView != null)
                {
                    // ВРЕМЕННО сохраняем бонус, НЕ засчитываем окончательно
                    _pendingBonuses.Add(bonusView.BonusType);
                    Debug.Log($"[BonusCollector] Бонус {bonusView.BonusType} собран, ожидает подтверждения");

                    // Удаляем бонус из мира
                    GameObject.Destroy(collider.gameObject);
                }
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}