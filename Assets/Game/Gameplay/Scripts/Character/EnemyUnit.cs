using Game.GameEngine.Ecs;
using UnityEngine;
using System.Collections.Generic;

namespace Entities
{
    public sealed class EnemyUnit : CharacterEntity
    {
        [Header("Enemy Specific")]
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private int experienceReward = 50;

        private CommandRequest currentCommand;

        protected override void InitCharacter()
        {
            this.SetData(new TeamComponent { playerId = 2 }); 

            if (this.HasData<TeamComponent>())
            {
                ref var team = ref this.GetData<TeamComponent>();
                Debug.Log($"[EnemyUnit] {name} (ID:{this.Id}) initialized with Team={team.playerId} (ENEMY)");
                if (team.playerId != 2)
                {
                    Debug.LogError($"[EnemyUnit] ERROR: {name} has Team={team.playerId} but should be 2!");
                }
            }
            else
            {
                Debug.LogError($"[EnemyUnit] ERROR: {name} TeamComponent was NOT set!");
            }

            this.SetData(new VisionComponent
            {
                radius = detectionRadius,
                detectedTargetId = -1,
                checkInterval = 0.5f,
                lastCheckTime = 0f
            });

            List<Vector3> patrolPoints = GetPatrolPoints();
            Debug.Log($"[EnemyUnit] {name} starting patrol with {patrolPoints.Count} points");

            this.SetData(new CommandRequest
            {
                type = CommandType.PATROL_BY_POINTS,
                args = patrolPoints,
                status = CommandStatus.IDLE
            });
        }

        private List<Vector3> GetPatrolPoints()
        {
            var patrolPoints = GameObject.FindGameObjectsWithTag("PatrolPoint");
            var points = new List<Vector3>();
            foreach (var point in patrolPoints)
            {
                points.Add(point.transform.position);
            }
            return points;
        }

        protected override void Die()
        {
            Debug.Log($"Enemy {name} died! Reward: {experienceReward} XP");


            if (this.HasData<AnimatorComponent>())
            {
                ref var animator = ref this.GetData<AnimatorComponent>();
                animator.value.ChangeState(5); 
            }

            Destroy(this.gameObject, 2f);

            base.Die();
        }
    }
}