using UnityEngine;

namespace Messages
{
    public struct InputStarted
    {
        public readonly Vector2 WorldPosition;
        public InputStarted(Vector2 worldPosition) => WorldPosition = worldPosition;
    }

    public struct InputFinished
    {
        public readonly Vector2 WorldPosition;
        public InputFinished(Vector2 worldPosition) => WorldPosition = worldPosition;
    }
}
