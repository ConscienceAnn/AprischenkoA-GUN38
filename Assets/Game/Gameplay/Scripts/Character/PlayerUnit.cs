using Game.GameEngine.Ecs;
using UnityEngine;

namespace Entities
{
    public sealed class PlayerUnit : CharacterEntity
    {
     //   [Header("Player Specific")]
      //  [SerializeField] private float gatherSpeed = 1f;
       // [SerializeField] private int carryCapacity = 10;

        protected override void InitCharacter()
        {
            // Добавляем компоненты, специфичные для юнита игрока
            this.SetData(new TeamComponent { playerId = 1 });

            // Можно добавить компонент сбора ресурсов
            // (если GatherTarget не добавлен в базовый класс)
        }

        // Переопределяем смерть для юнита
        protected override void Die()
        {
            Debug.Log($"Player unit {name} died!");
            base.Die();
        }
    }
}