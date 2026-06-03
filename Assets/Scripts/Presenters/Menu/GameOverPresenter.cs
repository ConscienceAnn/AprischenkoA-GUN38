using Messages;
using Models;
using Models.Interfaces;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace Presenters.Menu
{
    public sealed class GameOverPresenter : MonoBehaviour
    {
        private const string GameSceneName = "Game";
        private const string MainSceneName = "Main";
        [SerializeField] private TMP_Text _currentScore;
        [FormerlySerializedAs("_maxScore")] [SerializeField] private TMP_Text _bestScore;
        [Space(5)]
        [SerializeField] private Button _returnToMainButton;
        [SerializeField] private Button _restartButton;
        private IGameScoreModel _gameScoreModel;
        private IMessageBroker _messageBroker;
        private IBonusModel _bonusModel;

        [Header("Bonuses")]
        [SerializeField] private TMP_Text _starCountText;
        [SerializeField] private TMP_Text _heartCountText;


        [Inject]
        private void Inject(IGameScoreModel gameScoreModel, IMessageBroker broker, IBonusModel bonusModel)
        {
            _gameScoreModel = gameScoreModel;
            _messageBroker = broker;
            _bonusModel = bonusModel;
            _returnToMainButton.OnClickAsObservable().Subscribe(ReturnToMainButtonClicked).AddTo(this);
            _restartButton.OnClickAsObservable().Subscribe(RestartButtonClicked).AddTo(this);
        }

        private void ReturnToMainButtonClicked(Unit _) => SceneManager.LoadScene(MainSceneName);
        
        private void RestartButtonClicked(Unit _) => SceneManager.LoadScene(GameSceneName);

        private void Awake()
        {
            _messageBroker.Receive<MoveFailedMessage>().Subscribe(OnMoveFailed).AddTo(this);
            gameObject.SetActive(false);
        }

        private void OnMoveFailed(MoveFailedMessage message)
        {
            // Подсчет бонусов завершенной игры
            int stars = 0, hearts = 0;
            foreach (var bonus in _bonusModel.CollectedBonuses)
            {
                if (bonus == BonusType.Star) stars++;
                else if (bonus == BonusType.Heart) hearts++;
            }

            if (_starCountText != null) _starCountText.text = stars.ToString();
            if (_heartCountText != null) _heartCountText.text = hearts.ToString();

            gameObject.SetActive(true);
            _currentScore.text = _gameScoreModel.CurrentScore.ToString();
            _bestScore.text = _gameScoreModel.BestScore.ToString();
        }
    }
}