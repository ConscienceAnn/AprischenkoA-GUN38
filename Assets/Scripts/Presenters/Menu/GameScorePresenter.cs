using Models.Interfaces;
using Models;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Presenters.Menu
{
    public sealed class GameScorePresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _currentScore;
        [FormerlySerializedAs("_maxScore")] [SerializeField] private TMP_Text _bestScore;

        [Header("Bonuses")]
        [SerializeField] private TMP_Text _starCountText;   // Звезды в текущей сессии
        [SerializeField] private TMP_Text _heartCountText;  // Сердца в текущей сессии

        private IGameScoreModel _gameScoreModel;
        private IBonusModel _bonusModel;


        [Inject]
        private void Inject(IGameScoreModel gameScoreModel, IBonusModel bonusModel)
        {
            _gameScoreModel = gameScoreModel;
            _bonusModel = bonusModel;

            // Подписка на очки
            _gameScoreModel.CurrentScore.Subscribe(CurrentScoreUpdated).AddTo(this);
            _gameScoreModel.BestScore.Subscribe(BestScoreUpdated).AddTo(this);
            _bestScore.text = _gameScoreModel.BestScore.Value.ToString();

            // Подписка на изменения коллекции бонусов текущей сессии
            _bonusModel.CollectedBonuses
                .ObserveAdd()
                .Subscribe(_ => UpdateBonusUI())
                .AddTo(this);

            _bonusModel.CollectedBonuses
                .ObserveRemove()
                .Subscribe(_ => UpdateBonusUI())
                .AddTo(this);

            // Первоначальное обновление UI бонусов
            UpdateBonusUI();
        }

        private void CurrentScoreUpdated(int score) => _currentScore.text = score.ToString();
        
        private void BestScoreUpdated(int score) => _bestScore.text = score.ToString();

        /// <summary>
        /// Обновляет отображение бонусов на UI
        /// Показывает бонусы, собранные в ТЕКУЩЕЙ сессии
        /// </summary>
        private void UpdateBonusUI()
        {
            // Считаем количество звезд и сердец в CollectedBonuses
            int starCount = 0;
            int heartCount = 0;

            foreach (var bonus in _bonusModel.CollectedBonuses)
            {
                if (bonus == BonusType.Star)
                    starCount++;
                else if (bonus == BonusType.Heart)
                    heartCount++;
            }

            // Обновляем UI
            if (_starCountText != null)
                _starCountText.text = starCount.ToString();

            if (_heartCountText != null)
                _heartCountText.text = heartCount.ToString();
        }
    }
}