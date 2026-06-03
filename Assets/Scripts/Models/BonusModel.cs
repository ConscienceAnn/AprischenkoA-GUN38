using System;
using Core.SaveLoad;
using Models.Interfaces;
using UniRx;
using UnityEngine;

namespace Models
{
    public sealed class BonusModel : IBonusModel
    {
        // Ключи для сохранения в PlayerPrefs
        private const string SAVE_KEY_STAR = "Bonus_Star";
        private const string SAVE_KEY_HEART = "Bonus_Heart";

        private readonly ISaveLoadDataHandler _saveLoad;

        // Коллекция бонусов текущей сессии (то, что собрали за одну игру)
        private readonly ReactiveCollection<BonusType> _collectedBonuses = new();

        // Общие счетчики за всё время (сохраняются между сессиями)
        private int _totalStars;
        private int _totalHearts;

        public IReadOnlyReactiveCollection<BonusType> CollectedBonuses => _collectedBonuses;

        public BonusModel(ISaveLoadDataHandler saveLoad)
        {
            _saveLoad = saveLoad;
        }

        public void Initialize()
        {
            // Загружаем сохраненные значения
            _totalStars = LoadBonusCount(SAVE_KEY_STAR);
            _totalHearts = LoadBonusCount(SAVE_KEY_HEART);

            Debug.Log($"[BonusModel] Загружено: Stars={_totalStars}, Hearts={_totalHearts}");
        }

        public void AddBonus(BonusType bonus)
        {
            // Добавляем в коллекцию текущей сессии
            _collectedBonuses.Add(bonus);

            // Увеличиваем общий счетчик и сохраняем
            switch (bonus)
            {
                case BonusType.Star:
                    _totalStars++;
                    SaveBonusCount(SAVE_KEY_STAR, _totalStars);
                    break;
                case BonusType.Heart:
                    _totalHearts++;
                    SaveBonusCount(SAVE_KEY_HEART, _totalHearts);
                    break;
            }

            Debug.Log($"[BonusModel] Собран бонус: {bonus}. Всего: Stars={_totalStars}, Hearts={_totalHearts}");
        }

        public void ClearBonuses()
        {
            // Очищаем ТОЛЬКО бонусы текущей сессии
            // Общие счетчики (_totalStars, _totalHearts) НЕ трогаем!
            _collectedBonuses.Clear();
            Debug.Log("[BonusModel] Бонусы текущей сессии очищены (при падении)");
        }

        public int GetBonusCount(BonusType bonus)
        {
            return bonus switch
            {
                BonusType.Star => _totalStars,
                BonusType.Heart => _totalHearts,
                _ => 0
            };
        }

        public void Dispose()
        {
            _collectedBonuses?.Dispose();
        }

        // Вспомогательные методы для работы с сохранением
        private int LoadBonusCount(string key)
        {
            return _saveLoad.TryLoadInt(key, out int value) ? value : 0;
        }

        private void SaveBonusCount(string key, int value)
        {
            _saveLoad.SaveInt(key, value);
        }
    }
}