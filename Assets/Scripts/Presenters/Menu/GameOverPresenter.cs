using Messages;
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

        [Inject]
        private void Inject(IGameScoreModel gameScoreModel, IMessageBroker broker)
        {
            _gameScoreModel = gameScoreModel;
            _messageBroker = broker;
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
            gameObject.SetActive(true);
            _currentScore.text = _gameScoreModel.CurrentScore.ToString();
            _bestScore.text = _gameScoreModel.BestScore.ToString();
        }
    }
}