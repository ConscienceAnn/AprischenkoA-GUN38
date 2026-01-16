using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Objects")]
    public GameObject ballPrefab;
    public Transform ballSpawnPoint;
    public GameObject allPinsPrefab;
    public Transform pinsSpawnPoint;

    [Header("UI Elements")]
    public TMP_Text scoreText;
    public TMP_Text pinsLeftText;
    public TMP_Text throwsText;
    public TMP_Text frameText;

    [Header("Game Settings")]
    public float resetDelay = 2f;
    public float pinStandingThreshold = 0.7f;
    public float pinSettleTime = 4f;

    // Система фреймов
    private class Frame
    {
        public int firstThrow = 0;
        public int secondThrow = 0;
        public int frameScore = 0;
        public bool isStrike = false;
        public bool isSpare = false;
        public bool isComplete = false;
    }

    private List<Frame> frames = new List<Frame>();
    private int currentFrame = 0;
    private int currentThrowInFrame = 1;
    private int totalScore = 0;
    private int pinsStanding = 10;
    private bool isGameActive = true;
    private BallController currentBall;
    private GameObject currentPins;
    private bool isThrowInProgress = false;

    void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }
    void Start() { StartNewGame(); }

    void StartNewGame()
    {
        frames.Clear();
        for (int i = 0; i < 10; i++) frames.Add(new Frame());

        currentFrame = 0;
        currentThrowInFrame = 1;
        totalScore = 0;
        pinsStanding = 10;
        isGameActive = true;
        isThrowInProgress = false;

        SpawnPins();
        SpawnNewBall();
        UpdateUI();
    }

    void SpawnPins()
    {
        if (currentPins != null) Destroy(currentPins);
        if (allPinsPrefab && pinsSpawnPoint)
            currentPins = Instantiate(allPinsPrefab, pinsSpawnPoint.position, pinsSpawnPoint.rotation);
    }

    void SpawnNewBall()
    {
        if (!isGameActive || currentFrame >= 10) return;

        if (currentBall != null) Destroy(currentBall.gameObject);

        GameObject ballObj = Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity);
        currentBall = ballObj.GetComponent<BallController>();

        isThrowInProgress = false; // Разрешаем новый бросок

        Debug.Log($"Фрейм {currentFrame + 1}, бросок {currentThrowInFrame}");

        UpdateUI();
    }

    public void OnBallThrown()
    {
        if (!isGameActive || isThrowInProgress) return;
        isThrowInProgress = true;
        StartCoroutine(ProcessThrow());
    }

    IEnumerator ProcessThrow()
    {
        // Ждем успокоения СТОЯЩИХ кеглей
        yield return StartCoroutine(WaitForStandingPinsToSettle());

        // Уничтожаем старый мяч
        if (currentBall != null)
        {
            Destroy(currentBall.gameObject);
            currentBall = null;
        }

        Frame frame = frames[currentFrame];
        int pinsKnockedDownThisThrow;

        if (currentThrowInFrame == 1)
        {
            // Первый бросок: изначально было 10 кеглей
            pinsKnockedDownThisThrow = 10 - pinsStanding;
            frame.firstThrow = pinsKnockedDownThisThrow;

            Debug.Log($"Фрейм {currentFrame + 1}, бросок 1: сбито {pinsKnockedDownThisThrow} кеглей");

            if (pinsKnockedDownThisThrow == 10) // STRIKE
            {
                frame.isStrike = true;
                frame.isComplete = true;
                Debug.Log($"STRIKE в фрейме {currentFrame + 1}!");

                CalculateTotalScore();
                UpdateUI();

                // Переходим к следующему фрейму
                currentFrame++;
                currentThrowInFrame = 1;
                UpdateUI();

                // Создаем новые кегли и мяч
                if (currentFrame < 10)
                {
                    ResetPinsForNextFrame();
                    yield return new WaitForSeconds(0.5f);
                    SpawnNewBall();
                }
            }
            else
            {
                // Переходим ко второму броску
                currentThrowInFrame = 2;

                CalculateTotalScore();
                UpdateUI();

                // Создаем мяч для второго броска (кегли НЕ сбрасываем!)
                yield return new WaitForSeconds(1f);
                SpawnNewBall();
            }
        }
        else if (currentThrowInFrame == 2)
        {
            // Второй бросок: считаем сколько оставалось после первого броска
            int pinsStandingAfterFirstThrow = 10 - frame.firstThrow;
            pinsKnockedDownThisThrow = pinsStandingAfterFirstThrow - pinsStanding;
            frame.secondThrow = pinsKnockedDownThisThrow;

            Debug.Log($"Фрейм {currentFrame + 1}, бросок 2: сбито {pinsKnockedDownThisThrow} кеглей (оставалось {pinsStandingAfterFirstThrow})");

            // Проверяем SPARE
            if (frame.firstThrow + pinsKnockedDownThisThrow == 10)
            {
                frame.isSpare = true;
                Debug.Log($"SPARE в фрейме {currentFrame + 1}!");
            }

            frame.isComplete = true;

            CalculateTotalScore();
            UpdateUI();

            // Переходим к следующему фрейму
            currentFrame++;
            currentThrowInFrame = 1;
            UpdateUI();

            // Создаем новые кегли и мяч для следующего фрейма
            if (currentFrame < 10)
            {
                yield return new WaitForSeconds(resetDelay);
                ResetPinsForNextFrame();
                yield return new WaitForSeconds(0.5f);
                SpawnNewBall();
            }
        }

        // Проверяем окончание игры
        if (currentFrame >= 10)
        {
            EndGame();
        }
    }

    IEnumerator WaitForStandingPinsToSettle()
    {
        int checksWithoutMovement = 0;
        const int maxChecksWithoutMovement = 8; // 4 секунды (0.5 сек * 8)

        while (checksWithoutMovement < maxChecksWithoutMovement)
        {
            pinsStanding = 0;
            bool anyStandingPinMoving = false;

            if (currentPins != null)
            {
                foreach (Transform pin in currentPins.transform)
                {
                    if (!pin) continue;

                    float upDot = Vector3.Dot(pin.up, Vector3.up);
                    bool isStanding = upDot > pinStandingThreshold;

                    if (isStanding)
                    {
                        pinsStanding++;

                        Rigidbody rb = pin.GetComponent<Rigidbody>();
                        if (rb && rb.velocity.magnitude > 0.05f)
                        {
                            anyStandingPinMoving = true;
                        }
                    }
                }
            }

            // Если не осталось стоящих кеглей - выходим СРАЗУ
            if (pinsStanding == 0)
            {
                Debug.Log("Все кегли упали!");
                yield break;
            }

            if (anyStandingPinMoving)
            {
                checksWithoutMovement = 0; // Сбрасываем счетчик при движении
                Debug.Log("Стоящая кегля движется...");
            }
            else
            {
                checksWithoutMovement++;
                Debug.Log($"Стоящие кегли не двигаются уже {checksWithoutMovement * 0.5f} сек");
            }

            yield return new WaitForSeconds(0.5f);
        }

        // Если дошли сюда, значит стоящие кегли не двигались 4 секунды
        Debug.Log($"Стоящие кегли не двигались {pinSettleTime} секунд");
    }

    void ResetPinsForNextFrame()
    {
        if (currentFrame < 10)
        {
            SpawnPins();
            pinsStanding = 10;
        }
    }

    void CalculateTotalScore()
    {
        totalScore = 0;

        for (int i = 0; i < frames.Count; i++)
        {
            Frame frame = frames[i];
            if (!frame.isComplete) break;

            int frameScore = frame.firstThrow + frame.secondThrow;

            if (frame.isStrike && i + 1 < frames.Count)
            {
                frameScore += frames[i + 1].firstThrow;
                if (frames[i + 1].isStrike && i + 2 < frames.Count)
                    frameScore += frames[i + 2].firstThrow;
                else if (i + 1 < frames.Count)
                    frameScore += frames[i + 1].secondThrow;
            }
            else if (frame.isSpare && i + 1 < frames.Count)
            {
                frameScore += frames[i + 1].firstThrow;
            }

            frame.frameScore = frameScore;
            totalScore += frameScore;
        }
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = $"Очки: {totalScore}";
        if (pinsLeftText) pinsLeftText.text = $"Кеглей осталось: {pinsStanding}/10";
        if (throwsText) throwsText.text = $"Бросок: {currentThrowInFrame}/2";
        if (frameText) frameText.text = $"Фрейм: {currentFrame + 1}/10";
    }

    void EndGame()
    {
        isGameActive = false;
        Debug.Log($"Игра окончена! Счет: {totalScore}");
        if (throwsText) throwsText.text = $"Конец!\nСчет: {totalScore}";
    }

    public void NewGameButton() { StartNewGame(); }
}