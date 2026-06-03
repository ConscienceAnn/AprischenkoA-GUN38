using System;
using Messages;
using Models;
using Models.Interfaces;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;
using View;

namespace Presenters
{
    /// <summary>
    /// Отвечает за создание бонусов между платформами
    /// </summary>
    public sealed class BonusGeneratorPresenter : IInitializable, IDisposable
    {
        private readonly CompositeDisposable _disposable = new();
        private readonly IBuildingModel _buildingModel;
        private readonly IMessageBroker _messageBroker;

        // Префабы бонусов (назначить!)
        private readonly GameObject _starPrefab;
        private readonly GameObject _heartPrefab;

        // Родительский объект для всех бонусов (чтобы не засорять иерархию)
        private Transform _bonusesParent;

        // Список активных бонусов (чтобы удалить при перегенерации)
        private readonly System.Collections.Generic.List<GameObject> _activeBonuses = new();

        public BonusGeneratorPresenter(
            IBuildingModel buildingModel,
            IMessageBroker messageBroker,
            [Inject(Id = "StarBonus")] GameObject starPrefab,
            [Inject(Id = "HeartBonus")] GameObject heartPrefab)
        {
            _buildingModel = buildingModel;
            _messageBroker = messageBroker;
            _starPrefab = starPrefab;
            _heartPrefab = heartPrefab;
        }

        public void Initialize()
        {
            // Создаем родительский объект для бонусов
            _bonusesParent = new GameObject("[Bonuses]").transform;

            // ПОДПИСКА НА СООБЩЕНИЕ: бонусы появляются ПОСЛЕ того, как игрок успешно перешел
            // и камера переместилась (MoveSuccessfulMessage)
            _messageBroker.Receive<MoveSuccessfulMessage>()
                .Subscribe(_ => GenerateBonuses())
                .AddTo(_disposable);

            Debug.Log("[BonusGeneratorPresenter] Инициализирован");
        }

        /// <summary>
        /// Генерация бонусов между платформами
        /// Вызывается после каждого успешного перехода игрока
        /// </summary>
        private void GenerateBonuses()
        {
            // 1. Удаляем старые бонусы (если остались)
            ClearBonuses();

            // 2. Получаем границы следующей платформы (левая и правая границы)
            var (leftBorder, rightBorder) = _buildingModel.GetNextBuildingPositionRange();

            // 3. Рассчитываем, где находятся платформы:
            
          //  float platformSpacing = 2f; 

            // Бонусы размещаем на расстоянии 0.5, 1.0 и 1.5 от левой границы следующей платформы
            // (то есть перед платформой, но не на ней)

            float nextPlatformLeft = leftBorder;

            // Генерируем 2-3 бонуса в промежутке ПЕРЕД следующей платформой
            int bonusCount = Random.Range(2, 4);

            for (int i = 0; i < bonusCount; i++)
            {
                // Бонусы появляются на расстоянии 0.3 - 1.8 метра ПЕРЕД платформой
                // (не заходя на саму платформу)
                float offsetFromPlatform = Random.Range(0.3f, 1.8f);
                float xPosition = nextPlatformLeft - offsetFromPlatform;

                // Y-координата: на уровне земли + небольшой отступ
                float yPosition = 0.5f;

                // Выбираем случайный тип бонуса
                BonusType bonusType = Random.Range(0, 2) == 0 ? BonusType.Star : BonusType.Heart;
                GameObject prefab = bonusType == BonusType.Star ? _starPrefab : _heartPrefab;

                // Создаем бонус
                GameObject bonus = GameObject.Instantiate(
                    prefab,
                    new Vector3(xPosition, yPosition, 0),
                    Quaternion.identity,
                    _bonusesParent
                );

                _activeBonuses.Add(bonus);

                Debug.Log($"[BonusGenerator] Создан бонус {bonusType} на позиции X={xPosition}");
            }
        }

        private void ClearBonuses()
        {
            foreach (var bonus in _activeBonuses)
            {
                if (bonus != null)
                    GameObject.Destroy(bonus);
            }
            _activeBonuses.Clear();
        }

        public void Dispose()
        {
            ClearBonuses();
            _disposable.Dispose();
            if (_bonusesParent != null)
                GameObject.Destroy(_bonusesParent.gameObject);
        }
    }
}