using Models.Interfaces;
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

        [Inject]
        private void Inject(IGameScoreModel model)
        {
            model.CurrentScore.Subscribe(CurrentScoreUpdated).AddTo(this);
            model.BestScore.Subscribe(BestScoreUpdated).AddTo(this);
            _bestScore.text = model.BestScore.Value.ToString();
        }

        private void CurrentScoreUpdated(int score) => _currentScore.text = score.ToString();
        
        private void BestScoreUpdated(int score) => _bestScore.text = score.ToString();
    }
}