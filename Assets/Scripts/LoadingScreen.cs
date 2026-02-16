using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    private static LoadingScreen instance;

    [Header("Loading Screen Settings")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color progressBarColor = Color.green;
    [SerializeField] private string loadingTextFormat = "Загрузка... {0}%";
    [SerializeField] private int fontSize = 36;
    [SerializeField] private float minimumLoadingTime = 1f;

    private GameObject canvasObject;
    private Slider progressBar;
    private TMP_Text loadingText;

    public static LoadingScreen Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("LoadingScreen");
                instance = go.AddComponent<LoadingScreen>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName, System.Action onComplete = null)
    {
        StartCoroutine(LoadSceneAsync(sceneName, onComplete));
    }

    private void CreateLoadingScreen()
    {
        // Создаем Canvas
        canvasObject = new GameObject("LoadingCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        // Фон
        GameObject panelObj = new GameObject("Background");
        panelObj.transform.SetParent(canvasObject.transform);
        Image panel = panelObj.AddComponent<Image>();
        panel.color = backgroundColor;

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Текст
        GameObject textObj = new GameObject("LoadingText");
        textObj.transform.SetParent(canvasObject.transform);

        loadingText = textObj.AddComponent<TextMeshProUGUI>();
        loadingText.text = string.Format(loadingTextFormat, 0);
        loadingText.fontSize = fontSize;
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.color = textColor;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.6f);
        textRect.anchorMax = new Vector2(0.5f, 0.6f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(400, 100);

        // Прогресс-бар
        GameObject sliderObj = new GameObject("ProgressBar");
        sliderObj.transform.SetParent(canvasObject.transform);

        progressBar = sliderObj.AddComponent<Slider>();

        // Фон слайдера
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.gray;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Заполнитель
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(sliderObj.transform);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = progressBarColor;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.sizeDelta = Vector2.zero;

        progressBar.fillRect = fillRect;
        progressBar.targetGraphic = fillImage;

        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.4f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.4f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = Vector2.zero;
        sliderRect.sizeDelta = new Vector2(400, 20);

        DontDestroyOnLoad(canvasObject);
    }

    private IEnumerator LoadSceneAsync(string sceneName, System.Action onComplete)
    {
        // Создаем экран загрузки
        CreateLoadingScreen();

        yield return new WaitForSeconds(0.1f);

        // Загружаем сцену
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (!asyncLoad.isDone)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Обновляем UI
            if (progressBar != null)
                progressBar.value = progress;

            if (loadingText != null)
                loadingText.text = string.Format(loadingTextFormat, Mathf.Round(progress * 100));

            if (progress >= 0.9f && elapsedTime >= minimumLoadingTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // Уничтожаем экран загрузки
        if (canvasObject != null)
            Destroy(canvasObject);

        onComplete?.Invoke();
    }
}