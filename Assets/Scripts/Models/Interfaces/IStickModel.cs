using UnityEngine;
using Zenject;

namespace Models.Interfaces
{
    /// <summary>
    /// Interface for model that is used to manipulate with stick
    /// </summary>
    public interface IStickModel : IInitializable
    {
        /// <summary>
        /// Activate stick
        /// </summary>
        void EnableStick();

        /// <summary>
        /// De-activate stick
        /// </summary>
        void DisableStick();

        /// <summary>
        /// Increase stick's length
        /// </summary>
        void RaiseStick();

        /// <summary>
        /// Reset stick's data to start new raising routine
        /// </summary>
        /// <param name="newPosition">Position on a new building</param>
        void ResetStick(Vector2 newPosition);

        /// <summary>
        /// Get current length of stick
        /// </summary>
        /// <returns>Length as float value</returns>
        float GetStickLength();

        /// <summary>
        /// Make stick fall
        /// </summary>
        void FallStick();
    }
}