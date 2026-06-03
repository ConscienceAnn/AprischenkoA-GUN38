using Models.Interfaces;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Presenters.Menu
{
    public sealed class MainMenuPresenter : MonoBehaviour
    {
        private const string GameSceneName = "Game";
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private Button _startButton;
        private IGameScoreModel _gameScoreModel;

        [Inject]
        private void Inject(IGameScoreModel gameScoreModel) => _gameScoreModel = gameScoreModel;

        private void Start()
        {
            _scoreText.text = _gameScoreModel.BestScore.ToString();
            _startButton.OnClickAsObservable().Subscribe(OnStartButtonClicked).AddTo(this);
        }

        private void OnStartButtonClicked(Unit obj) => SceneManager.LoadScene(GameSceneName);
    }
}