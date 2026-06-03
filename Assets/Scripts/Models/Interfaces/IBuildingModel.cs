using UnityEngine;
using Zenject;

namespace Models.Interfaces
{
    /// <summary>
    /// Interface for models that keeps building objects and position
    /// </summary>
    public interface IBuildingModel : IInitializable
    {
        /// <summary>
        /// Set position for the next building
        /// Implementation should set position to the building, that is next to player
        /// </summary>
        /// <param name="position"></param>
        void SetNextBuildingPosition(Vector2 position);

        /// <summary>
        /// Get position on building where player stands
        /// </summary>
        /// <param name="isDefault">True - we return current building position, False - next</param>
        /// <returns>Player Position</returns>
        Vector2 GetPositionForPlayer(bool isDefault);

        /// <summary>
        /// Get the left and right borders of building
        /// </summary>
        /// <returns>Left and right border</returns>
        (float min, float max) GetNextBuildingPositionRange();

        /// <summary>
        /// Calculate position from which stick should be built
        /// </summary>
        /// <returns>Stick position on current building</returns>
        Vector2 CalculateStartPositionForStick();
    }
}