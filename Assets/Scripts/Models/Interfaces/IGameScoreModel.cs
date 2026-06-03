using UniRx;
using Zenject;

namespace Models.Interfaces
{
    /// <summary>
    /// Interface for Model that keeps player's score (best and current)
    /// Current can be reseted
    /// Best should be saved and loaded, so SaveLoad should be injected
    /// </summary>
    public interface IGameScoreModel : IInitializable
    {
        /// <summary>
        /// Subscribe for updated score value
        /// </summary>
        ReactiveProperty<int> CurrentScore { get; }
        
        /// <summary>
        /// Subscribe for updated Best score value
        /// </summary>
        ReactiveProperty<int> BestScore { get; }
        
        /// <summary>
        /// Increase score
        /// </summary>
        void IncreaseScore();

        /// <summary>
        /// Set score value to default
        /// </summary>
        void ResetScore();
    }
}