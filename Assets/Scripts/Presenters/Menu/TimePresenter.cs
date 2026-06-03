using Models.Interfaces;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Presenters.Menu
{
    public sealed class TimePresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _currentScore;

        [Inject]
        private void Inject(ITimeModel timeModel) 
            => timeModel.GameTime.Subscribe((time) => _currentScore.text = time.ToString()).AddTo(this);
    }
}