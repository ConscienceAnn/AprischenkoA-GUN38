using Game.GameEngine.Ecs;
using UnityEngine;

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

            // Добавляем компонент зрения для автоматического обнаружения игрока
            this.SetData(new VisionComponent
            {
                radius = detectionRadius,
                detectedTargetId = -1,
                checkInterval = 0.5f
            });

            // Враг автоматически начинает патрулирование при спавне
            this.SetData(new CommandRequest
            {
                type = CommandType.PATROL_BY_POINTS,
                args = GetPatrolPoints(),
                status = CommandStatus.IDLE
            });
        }

        private object GetPatrolPoints()
        {
            // Можно получить из глобального менеджера или найти по тегу
            var patrolPoints = GameObject.FindGameObjectsWithTag("PatrolPoint");
            var points = new System.Collections.Generic.List<Vector3>();
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