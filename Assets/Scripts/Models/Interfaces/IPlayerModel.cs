using UnityEngine;
using Zenject;

namespace Models.Interfaces
{
    /// <summary>
    /// Interface for model that manipulates player object 
    /// </summary>
    public interface IPlayerModel : IInitializable
    {
        /// <summary>
        /// Activate player
        /// </summary>
        void EnablePlayer();

        /// <summary>
        /// De-activate player
        /// </summary>
        void DisablePlayer();
        
        /// <summary>
        /// Move player to new position
        /// </summary>
        /// <param name="position">New position</param>
        /// <param name="animate">Should we animate player movement</param>
        /// <param name="fall">Should player fall in the end</param>
        void MovePlayer(Vector2 position, bool animate = false, bool fall = false);
    }
}