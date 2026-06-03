using System.Threading;
using Core.SaveLoad;
using Models.Interfaces;
using UniRx;

namespace Models
{
    public sealed class GameScoreModel : IGameScoreModel
    {
        private const string BestScoreKey = "BestScore";
        private readonly ISaveLoadDataHandler _saveLoadDataHandler;
        private ReactiveProperty<int> _currentScore;
        private ReactiveProperty<int> _bestScore;

        public ReactiveProperty<int> CurrentScore => _currentScore;
        public ReactiveProperty<int> BestScore => _bestScore;

        public GameScoreModel(ISaveLoadDataHandler saveLoadDataHandler) => _saveLoadDataHandler = saveLoadDataHandler;

        public void Initialize()
        {
            _bestScore = new ReactiveProperty<int>(_saveLoadDataHandler.TryLoadInt(BestScoreKey, out var bestScore) ? bestScore : 0);
            _currentScore = new ReactiveProperty<int>(0);
        }

        public void IncreaseScore()
        {
            _currentScore.Value++;
            if (_currentScore.Value > _bestScore.Value)
            {
                _bestScore.Value = _currentScore.Value;
                _saveLoadDataHandler.SaveInt(BestScoreKey, _bestScore.Value);
            }
        }

        public void ResetScore() => _currentScore.Value = 0;
    }
}