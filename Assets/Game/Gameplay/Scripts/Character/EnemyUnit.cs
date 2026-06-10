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
            // Добавляем компоненты, специфичные для врага
            this.SetData(new TeamComponent { playerId = 2 }); // Вражеская команда

            // ПРОВЕРКА: убедимся, что TeamComponent установлен
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

            // Добавляем компонент зрения для автоматического обнаружения игрока
            this.SetData(new VisionComponent
            {
                radius = detectionRadius,
                detectedTargetId = -1,
                checkInterval = 0.5f,
                lastCheckTime = 0f
            });

            // Враг автоматически начинает патрулирование при спавне
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
            // Можно получить из глобального менеджера или найти по тегу
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
            // Враг даёт опыт при смерти
            Debug.Log($"Enemy {name} died! Reward: {experienceReward} XP");

            // Проигрываем анимацию смерти
            if (this.HasData<AnimatorComponent>())
            {
                ref var animator = ref this.GetData<AnimatorComponent>();
                animator.value.ChangeState(5); // DEATH state
            }

            // Уничтожаем объект через 2 секунды (чтобы анимация успела проиграться)
            Destroy(this.gameObject, 2f);

            base.Die();
        }
    }
}