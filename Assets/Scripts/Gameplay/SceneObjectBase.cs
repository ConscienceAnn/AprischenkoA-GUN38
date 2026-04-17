using UnityEngine;

namespace Gameplay
{
    public abstract class SceneObjectBase : MonoBehaviour
    {
        private const float POSITION_DELTA = 0.15f;

        public Vector2 Position => transform.position;

        public bool IsOnSamePosition(Vector2 otherPosition)
        {
            return Mathf.Abs(Position.x - otherPosition.x) <= POSITION_DELTA &&
                   Mathf.Abs(Position.y - otherPosition.y) <= POSITION_DELTA;
        }

        public bool IsOnSamePosition(SceneObjectBase other)
        {
            return IsOnSamePosition(other.Position);
        }
    }
}