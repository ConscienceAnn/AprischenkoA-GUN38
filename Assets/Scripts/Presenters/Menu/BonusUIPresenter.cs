using Messages;
using Models;
using Models.Interfaces;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Presenters.Menu
{
    public sealed class BonusUIPresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _starCountText;
        [SerializeField] private TMP_Text _heartCountText;

        private IBonusModel _bonusModel;
        private IMessageBroker _messageBroker;
        private int _savedStars;
        private int _savedHearts;

        private readonly CompositeDisposable _disposable = new();

        // Внедрение через Zenject (вызывается даже на неактивных объектах!)
        [Inject]
        private void Construct(IBonusModel bonusModel, IMessageBroker messageBroker)
        {
            _bonusModel = bonusModel;
            _messageBroker = messageBroker;

            // Подписываемся на падение СРАЗУ
            _messageBroker.Receive<MoveFailedMessage>()
                .Subscribe(_ => OnGameOver())
                .AddTo(_disposable);

            Debug.Log("[BonusUIPresenter] Подписался на MoveFailedMessage в Construct");
        }

        private void OnGameOver()
        {
            Debug.Log("[BonusUIPresenter] OnGameOver ВЫЗВАН!");

            _savedStars = 0;
            _savedHearts = 0;

            foreach (var bonus in _bonusModel.CollectedBonuses)
            {
                if (bonus == BonusType.Star)
                    _savedStars++;
                else if (bonus == BonusType.Heart)
                    _savedHearts++;
            }

            Debug.Log($"[BonusUIPresenter] Подсчитано: Stars={_savedStars}, Hearts={_savedHearts}");

            if (_starCountText != null)
                _starCountText.text = _savedStars.ToString();

            if (_heartCountText != null)
                _heartCountText.text = _savedHearts.ToString();

            Debug.Log($"[BonusUIPresenter] GameOver. Бонусы за игру: Звезды={_savedStars}, Сердца={_savedHearts}");
        }

        private void OnDestroy()
        {
            _disposable.Dispose();
        }
    }
}