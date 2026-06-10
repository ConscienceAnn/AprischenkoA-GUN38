using Game.GameEngine.Ecs;
using UnityEngine;

namespace Entities
{
    public sealed class PlayerUnit : CharacterEntity
    {
        [Header("Player Specific")]
        [SerializeField] private float detectionRadius = 15f;

        protected override void InitCharacter()
        {

            this.SetData(new TeamComponent { playerId = 1 });


            if (this.HasData<TeamComponent>())
            {
                ref var team = ref this.GetData<TeamComponent>();
                Debug.Log($"[PlayerUnit] {name} (ID:{this.Id}) initialized with Team={team.playerId} (PLAYER)");
                if (team.playerId != 1)
                {
                    Debug.LogError($"[PlayerUnit] ERROR: {name} has Team={team.playerId} but should be 1!");
                }
            }
            else
            {
                Debug.LogError($"[PlayerUnit] ERROR: {name} TeamComponent was NOT set!");
            }

            this.SetData(new VisionComponent
            {
                radius = detectionRadius,
                detectedTargetId = -1,
                checkInterval = 0.5f,
                lastCheckTime = 0f
            });
        }

        protected override void Die()
        {
            Debug.Log($"Player unit {name} died!");
            base.Die();
        }
    }
}