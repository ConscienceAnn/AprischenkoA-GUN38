using Models;
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

        [Header("Bonuses")]
        [SerializeField] private TMP_Text _starBonusText;
        [SerializeField] private TMP_Text _heartBonusText;

        private IGameScoreModel _gameScoreModel;

        private IBonusModel _bonusModel;

        [Inject]
        private void Inject(IGameScoreModel gameScoreModel, IBonusModel bonusModel) 
        {
            _gameScoreModel = gameScoreModel;
            _bonusModel = bonusModel;  
        }

        private void Start()
        {
            _scoreText.text = _gameScoreModel.BestScore.ToString();

            _starBonusText.text = _bonusModel.GetBonusCount(BonusType.Star).ToString();
            _heartBonusText.text = _bonusModel.GetBonusCount(BonusType.Heart).ToString();

            _startButton.OnClickAsObservable().Subscribe(OnStartButtonClicked).AddTo(this);
        }

        private void OnStartButtonClicked(Unit obj) => SceneManager.LoadScene(GameSceneName);
    }
}