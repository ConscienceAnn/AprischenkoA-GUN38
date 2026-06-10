using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct VisionComponent
    {
        public float radius;
        public int detectedTargetId;
        public float checkInterval;
        public float lastCheckTime;
    }
}