using System;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    [Serializable]
    public struct GroupMoveData
    {
        public Vector3 destination;
        public bool isGroupLeader;
        public int groupId;
        public float waitDistance;
        public bool hasStopped; 
    }
}