using UnityEngine;

namespace SampleProject
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Gameplay/New PlayerConfig")]
    public sealed class PlayerConfig : CharacterConfig
    {
        [Header("Player Specific")]
        public float gatherSpeed = 1f;
        public int carryCapacity = 10;
    }
}