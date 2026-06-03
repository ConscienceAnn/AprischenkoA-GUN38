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

        [Inject]
        private void Inject(IBonusModel bonusModel)
        {
            _bonusModel = bonusModel;
        }

        private void Start()
        {
            // ÏÎÄÏÈÑÊÀ ÍÀ OnAdded È OnRemove Ó ĞÅÀÊÒÈÂÍÎÉ ÊÎËËÅÊÖÈÈ - İÒÎ ÒĞÅÁÎÂÀÍÈÅ ÇÀÄÀÍÈß!
            _bonusModel.CollectedBonuses
                .ObserveAdd()
                .Subscribe(_ => UpdateUI())
                .AddTo(this);

            _bonusModel.CollectedBonuses
                .ObserveRemove()
                .Subscribe(_ => UpdateUI())
                .AddTo(this);

            UpdateUI();

            Debug.Log("[BonusUIPresenter] Ïîäïèñàí íà èçìåíåíèÿ CollectedBonuses (OnAdded/OnRemoved)");
        }

        private void UpdateUI()
        {
            int starCount = 0;
            int heartCount = 0;

            foreach (var bonus in _bonusModel.CollectedBonuses)
            {
                if (bonus == BonusType.Star) starCount++;
                else if (bonus == BonusType.Heart) heartCount++;
            }

            if (_starCountText != null)
                _starCountText.text = starCount.ToString();

            if (_heartCountText != null)
                _heartCountText.text = heartCount.ToString();
        }
    }
}