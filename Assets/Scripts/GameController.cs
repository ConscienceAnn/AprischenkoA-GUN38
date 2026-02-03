using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("Основные ссылки")]
    public PathChoiceManager choiceManager;

    [Header("UI Элементы")]
    public GameObject choicePanel;        // Панель с кнопками выбора
    public Button leftButton;             // Левая дорожка
    public Button rightButton;            // Правая дорожка
    public GameObject resultsPanel;       // Панель результатов
    public TMP_Text resultsText;              // Текст результатов
    public Button restartButton;          // Кнопка "Начать заново"

    void Start()
    {
        SetupUI();
        StartNewRound();
    }

    void SetupUI()
    {
        // Назначаем обработчики кнопок выбора
        if (leftButton != null)
            leftButton.onClick.AddListener(() => MakeChoice(true));

        if (rightButton != null)
            rightButton.onClick.AddListener(() => MakeChoice(false));

        // Назначаем обработчик кнопки "Начать заново"
        if (restartButton != null)
            restartButton.onClick.AddListener(StartNewRound);

        // Скрываем все UI панели
        HideAllUI();
    }

    /// <summary>
    /// Начинает новый раунд (выбор дорожки)
    /// </summary>
    public void StartNewRound()
    {
        Debug.Log("=== НАЧИНАЕМ НОВЫЙ РАУНД ===");

        DOTween.KillAll();
        // Скрываем панель результатов
        HideResults();

        // Очищаем старые дорожки (если есть)
        ClearOldPaths();

        // Через небольшую задержку показываем выбор
        Invoke("ShowChoice", 0.5f);
    }

    void ShowChoice()
    {
        // Показываем ворота выбора
        if (choiceManager != null)
            choiceManager.ShowChoice();

        // Показываем UI выбора
        ShowChoiceUI();
    }

    void MakeChoice(bool isLeft)
    {
        Debug.Log($"Выбрана {(isLeft ? "левая" : "правая")} дорожка");

        // Анимация нажатия кнопки
        Button chosenButton = isLeft ? leftButton : rightButton;
        if (chosenButton != null)
        {
            chosenButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
        }

        // Передаём выбор в менеджер
        if (choiceManager != null)
            choiceManager.OnPathChosen(isLeft);

        // Скрываем UI выбора
        HideChoiceUI();
    }

    /// <summary>
    /// Вызывается когда персонаж дошёл до конца дорожки
    /// </summary>
    public void OnPathCompleted()
    {
        Debug.Log("Путь завершён!");

        // Через задержку показываем результаты
        Invoke("ShowResults", 1f);
    }

    void ShowResults()
    {
        if (resultsPanel != null && resultsText != null)
        {
            resultsPanel.SetActive(true);

            // Простой текст результатов
            resultsText.text = "ДОРОЖКА ПРОЙДЕНА!\n\n" +
                              "Хочешь попробовать ещё раз?";

            // Анимация появления
            CanvasGroup cg = GetCanvasGroup(resultsPanel);
            cg.alpha = 0f;
            cg.DOFade(1f, 0.5f);

            // Анимация кнопки "Начать заново"
            if (restartButton != null)
            {
                restartButton.transform.localScale = Vector3.zero;
                restartButton.transform.DOScale(Vector3.one, 0.4f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.3f);
            }
        }
    }

    void HideResults()
    {
        if (resultsPanel != null)
        {
            CanvasGroup cg = GetCanvasGroup(resultsPanel);
            cg.DOFade(0f, 0.3f)
                .OnComplete(() => resultsPanel.SetActive(false));
        }
    }

    void ShowChoiceUI()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(true);

            // Анимация появления панели
            CanvasGroup cg = GetCanvasGroup(choicePanel);
            cg.alpha = 0f;
            cg.DOFade(1f, 0.3f);

            // Анимация кнопок (по очереди)
            if (leftButton != null)
            {
                leftButton.transform.localScale = Vector3.zero;
                leftButton.transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.1f);
            }

            if (rightButton != null)
            {
                rightButton.transform.localScale = Vector3.zero;
                rightButton.transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.2f);
            }
        }
    }

    void HideChoiceUI()
    {
        if (choicePanel != null)
        {
            CanvasGroup cg = GetCanvasGroup(choicePanel);
            cg.DOFade(0f, 0.3f)
                .OnComplete(() => choicePanel.SetActive(false));
        }
    }

    void HideAllUI()
    {
        if (choicePanel != null) choicePanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
    }

    void ClearOldPaths()
    {
     
        if (choiceManager != null)
        {
            
            choiceManager.ClearPaths();
        }
    }

    CanvasGroup GetCanvasGroup(GameObject obj)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }

}