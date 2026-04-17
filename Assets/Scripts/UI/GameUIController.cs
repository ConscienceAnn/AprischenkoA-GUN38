using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Core.MessageSystem;
using Messages;
using TMPro;

namespace UI
{
    public class GameUIController : MonoBehaviour,
        IMessageListener<LevelCompleted>,
        IMessageListener<StarCollected>,
        IMessageListener<LevelRestarted>
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private TMP_Text _starsText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _nextLevelButton;

        [Header("Victory Panel Buttons")]
        [Tooltip("Кнопка Main Menu на панели победы")]
        [SerializeField] private Button _victoryMenuButton;

        [Header("Victory Stars")]
        [Tooltip("Родительский объект-контейнер для звёзд на панели победы")]
        [SerializeField] private Transform _starsContainer;

        [Tooltip("Префаб UI-звезды (должен содержать компонент Image)")]
        [SerializeField] private GameObject _starPrefab;

        [Tooltip("Спрайт собранной (золотой) звезды")]
        [SerializeField] private Sprite _starCollectedSprite;

        [Tooltip("Спрайт пустой (серой) звезды")]
        [SerializeField] private Sprite _starEmptySprite;

        [Header("Level Settings")]
        [SerializeField] private string _nextLevelName = "Level2";

        private int _starsCollected = 0;
        private int _totalStars = 0;

        private Image[] _victoryStars;

        private void Awake()
        {
            // Подписываемся на сообщения
            Messenger.Subscribe<LevelCompleted>(this);
            Messenger.Subscribe<StarCollected>(this);
            Messenger.Subscribe<LevelRestarted>(this);

            // Подписываем кнопки на события
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);

            if (_menuButton != null)
                _menuButton.onClick.AddListener(OnMenuClicked);

            if (_nextLevelButton != null)
                _nextLevelButton.onClick.AddListener(OnNextLevelClicked);

            if (_victoryMenuButton != null)
                _victoryMenuButton.onClick.AddListener(OnMenuClicked);

            // Изначально панель победы скрыта
            if (_victoryPanel != null)
                _victoryPanel.SetActive(false);

            SetGameplayButtonsActive(true);

            // Считаем общее количество звезд на уровне
            _totalStars = FindObjectsOfType<Gameplay.StarObject>().Length;

            UpdateStarsUI();

            CreateVictoryStars();
        }

        private void OnDestroy()
        {
            Messenger.Unsubscribe<LevelCompleted>(this);
            Messenger.Unsubscribe<StarCollected>(this);
            Messenger.Unsubscribe<LevelRestarted>(this);

            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            if (_menuButton != null)
                _menuButton.onClick.RemoveListener(OnMenuClicked);
            if (_nextLevelButton != null)
                _nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
            if (_victoryMenuButton != null)
                _victoryMenuButton.onClick.RemoveListener(OnMenuClicked);
        }

        private void CreateVictoryStars()
        {
            if (_starsContainer == null || _starPrefab == null)
            {
                Debug.LogWarning("Stars container or prefab not assigned!");
                return;
            }

            // Удаляем старые звёзды (если есть)
            foreach (Transform child in _starsContainer)
            {
                Destroy(child.gameObject);
            }

            // Создаём новые звёзды по количеству totalStars
            _victoryStars = new Image[_totalStars];
            for (int i = 0; i < _totalStars; i++)
            {
                GameObject starGO = Instantiate(_starPrefab, _starsContainer);
                _victoryStars[i] = starGO.GetComponent<Image>();

                // По умолчанию — пустая звезда
                if (_starEmptySprite != null)
                    _victoryStars[i].sprite = _starEmptySprite;
            }
        }

        private void UpdateVictoryStars()
        {
            if (_victoryStars == null) return;

            for (int i = 0; i < _victoryStars.Length; i++)
            {
                if (i < _starsCollected)
                {
                    // Собранная звезда
                    if (_starCollectedSprite != null)
                        _victoryStars[i].sprite = _starCollectedSprite;
                }
                else
                {
                    // Пустая звезда
                    if (_starEmptySprite != null)
                        _victoryStars[i].sprite = _starEmptySprite;
                }
            }
        }

        public void OnMessage(LevelCompleted message)
        {
            Debug.Log("UI: Level Completed! Showing victory panel.");
            UpdateVictoryStars();

            if (_victoryPanel != null)
                _victoryPanel.SetActive(true);
            SetGameplayButtonsActive(false);

        }

        public void OnMessage(StarCollected message)
        {
            _starsCollected++;
            UpdateStarsUI();
            Debug.Log($"UI: Star collected! {_starsCollected}/{_totalStars}");
           
        }

        public void OnMessage(LevelRestarted message)
        {
            _starsCollected = 0;
            UpdateStarsUI();
            if (_victoryPanel != null)
                _victoryPanel.SetActive(false);

            SetGameplayButtonsActive(true);
        }

        private void UpdateStarsUI()
        {
            if (_starsText != null)
                _starsText.text = $"Stars: {_starsCollected}/{_totalStars}";
        }

        private void SetGameplayButtonsActive(bool active)
        {
            if (_restartButton != null)
                _restartButton.gameObject.SetActive(active);

            if (_menuButton != null)
                _menuButton.gameObject.SetActive(active);
        }

        private void OnRestartClicked()
        {
            Debug.Log("UI: Restart clicked");
            Messenger.Send(new LevelRestarted());
        }

        private void OnMenuClicked()
        {
            Debug.Log("UI: Menu clicked");
            SceneManager.LoadScene("MainMenu");
        }

        private void OnNextLevelClicked()
        {
            Debug.Log($"UI: Next level clicked -> {_nextLevelName}");
            SceneManager.LoadScene(_nextLevelName);
        }

    }
}