using DG.Tweening;
using Models;
using UnityEngine;

namespace View
{
    /// <summary>
    /// Компонент, который вешается на префаб бонуса
    /// </summary>
    public class BonusView : MonoBehaviour
    {
        [SerializeField] private BonusType _bonusType;

        public BonusType BonusType => _bonusType;

        // to do добавить визуальные эффекты
        private void Start()
        {
            // Небольшая анимация появления
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // Игрок коснулся бонуса
                // Сам сбор обрабатывается в BonusCollectorPresenter
                // Здесь можно добавить эффект сбора
                Destroy(gameObject, 0.1f);
            }
        }
    }
}