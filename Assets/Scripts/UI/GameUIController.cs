using UnityEngine;
using UnityEngine.SceneManagement;
using Core.MessageSystem;
using Messages;

namespace UI
{
    public class GameUIController : MonoBehaviour, IMessageListener<LevelCompleted>, IMessageListener<StarCollected>
    {
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private UnityEngine.UI.Text _starsText;

        private int _starsCollected = 0;

        private void Awake()
        {
            Messenger.Subscribe<LevelCompleted>(this);
            Messenger.Subscribe<StarCollected>(this);

            if (_victoryPanel != null)
                _victoryPanel.SetActive(false);

            UpdateStarsUI();
        }

        private void OnDestroy()
        {
            Messenger.Unsubscribe<LevelCompleted>(this);
            Messenger.Unsubscribe<StarCollected>(this);
        }

        public void OnMessage(LevelCompleted message)
        {
            if (_victoryPanel != null)
                _victoryPanel.SetActive(true);
        }

        public void OnMessage(StarCollected message)
        {
            _starsCollected++;
            PlayerPrefs.SetInt("StarsCollected", _starsCollected);
            UpdateStarsUI();
        }

        private void UpdateStarsUI()
        {
            if (_starsText != null)
                _starsText.text = $"Stars: {_starsCollected}";
        }

        // Вызывается по кнопке
        public void OnRestartClicked()
        {
            Messenger.Send(new LevelRestarted());
            _starsCollected = 0;
            UpdateStarsUI();
            if (_victoryPanel != null)
                _victoryPanel.SetActive(false);
        }

        // Вызывается по кнопке
        public void OnMenuClicked()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}