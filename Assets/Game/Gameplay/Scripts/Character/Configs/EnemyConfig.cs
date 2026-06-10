using UnityEngine;

namespace SampleProject
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Gameplay/New EnemyConfig")]
    public sealed class EnemyConfig : CharacterConfig
    {
        [Header("Enemy Specific")]
        public float detectionRadius = 10f;
        public int experienceReward = 50;
    }
}