using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _pinsLeftText;
    [SerializeField] private TMP_Text _throwsText;
    [SerializeField] private TMP_Text _frameText;
    [SerializeField] private TMP_Text _gameOverText;


    private void Start()
    {
        // Проверяем что все ссылки установлены
        if (_scoreText == null) Debug.LogError("Score Text не установлен!");
        if (_pinsLeftText == null) Debug.LogError("Pins Left Text не установлен!");
        if (_throwsText == null) Debug.LogError("Throws Text не установлен!");
        if (_frameText == null) Debug.LogError("Frame Text не установлен!");
        if (_gameOverText == null) Debug.LogError("Game Over Text не установлен!");

        // Скрываем текст окончания игры
        HideGameOver();
    }

    public void UpdateScore(int score)
    {
        Debug.Log($"UIManager.UpdateScore() вызван с score={score}");
        if (_scoreText != null)
        {
            _scoreText.text = $"Очки: {score}";
            Debug.Log($"Текст установлен: 'Очки: {score}'");
        }
        else
        {
            Debug.LogError("_scoreText не подключен в Inspector!");
        }
    }

    public void UpdatePinsLeft(int pinsStanding)
    {
        if (_pinsLeftText != null)
        {
            _pinsLeftText.text = $"Кеглей осталось: {pinsStanding}/10";
        }
    }

    public void UpdateThrowInfo(int currentThrow, int maxThrows = 2)
    {
        if (_throwsText != null)
        {
            _throwsText.text = $"Бросок: {currentThrow}/{maxThrows}";
        }
    }

    public void UpdateFrameInfo(int currentFrame, int maxFrames = 10)
    {
        if (_frameText != null)
        {
            _frameText.text = $"Фрейм: {currentFrame}/{maxFrames}";
        }
    }

    public void ShowGameOver(int finalScore)
    {
        if (_gameOverText != null)
        {
            _gameOverText.text = $"Конец!\nСчет: {finalScore}";
            _gameOverText.gameObject.SetActive(true);
        }
    }

    public void HideGameOver()
    {
        if (_gameOverText != null)
        {
            _gameOverText.gameObject.SetActive(false);
        }
    }
}