using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct BaseStorageComponent
    {
        public int wood;
        public int minerals;
        public int gold;
    }
}
