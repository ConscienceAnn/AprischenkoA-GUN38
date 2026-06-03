using System;
using UniRx;
using Zenject; 

namespace Models.Interfaces
{
    /// <summary>
    /// Интерфейс модели бонусов
    /// </summary>
    public interface IBonusModel : IInitializable, IDisposable
    {
        /// <summary>
        /// Реактивная коллекция собранных бонусов (только для чтения)
        /// Позволяет UI подписаться на изменения
        /// </summary>
        IReadOnlyReactiveCollection<BonusType> CollectedBonuses { get; }

        /// <summary>
        /// Добавить бонус (вызывается при сборе)
        /// </summary>
        void AddBonus(BonusType bonus);

        /// <summary>
        /// Очистить бонусы текущей сессии (при падении игрока)
        /// </summary>
        void ClearBonuses();

        /// <summary>
        /// Получить общее количество бонусов определенного типа (для сохранения)
        /// </summary>
        int GetBonusCount(BonusType bonus);
    }
}