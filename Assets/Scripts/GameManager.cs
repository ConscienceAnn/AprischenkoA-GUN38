using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Паттерн Singleton для удобного доступа из других скриптов

    public GameObject ballPrefab; // Префаб снежка (будет меняться)
    public Transform ballSpawnPoint; // Точка спауна мяча
    public TMP_Text scoreText; // Ссылка на UI Text для отображения счета
    public TMP_Text pinsLeftText; // Текст "Кеглей осталось"

    private int currentFrame = 1;
    private int[] frameScores = new int[10];
   // private int currentThrow = 0;
    private int totalScore = 0;
    private int pinsStandingCount = 10;
    private BallController currentBall;
  //  private bool isWaitingForThrow = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SpawnNewBall();
        UpdateUI();
    }

    void SpawnNewBall()
    {
        if (ballPrefab && ballSpawnPoint)
        {
            GameObject ballObj = Instantiate(ballPrefab, ballSpawnPoint.position, Quaternion.identity);
            currentBall = ballObj.GetComponent<BallController>();
          //  isWaitingForThrow = true;
        }
    }

    // Этот метод будут вызывать кегли, когда их собьют
    public void PinKnockedDown()
    {
        // Пересчитываем устоявшие кегли (будет в следующем шаге)
        CountStandingPins();
    }

    void CountStandingPins()
    {
        pinsStandingCount = 0;
        // Найдем все объекты с тегом "Pin" (его нужно присвоить префабу снеговика!)
        GameObject[] pins = GameObject.FindGameObjectsWithTag("Pin");
        foreach (GameObject pin in pins)
        {
            Rigidbody pinRb = pin.GetComponent<Rigidbody>();
            // Проверяем, упала ли кегля (например, наклонилась более чем на 45 градусов)
            if (pinRb && pin.transform.up.y > 0.5f) // Если вектор "вверх" кегли еще смотрит вверх
            {
                pinsStandingCount++;
            }
        }
        UpdateUI();

        // Проверяем, все ли кегли упали или мяч ушел в аут
        StartCoroutine(CheckThrowEnd());
    }

    // Корутина (небольшая задержка перед проверкой)
    private System.Collections.IEnumerator CheckThrowEnd()
    {
        yield return new WaitForSeconds(3f); // Ждем 3 секунды, пока все устаканится

        if (pinsStandingCount == 0 || currentBall.transform.position.y < -5) // Мяч упал с дорожки
        {
            EndThrow();
        }
    }

    void EndThrow()
    {
        int pinsKnockedDown = 10 - pinsStandingCount;
        // Здесь должна быть сложная логика подсчета очков в боулинге (Strike, Spare).
        // Для САМОГО ПЕРВОГО ПРИМЕРА упростим:
        totalScore += pinsKnockedDown;
        UpdateUI();

        // Готовимся к следующему броску
        Destroy(currentBall.gameObject);
        if (currentFrame <= 10 && pinsStandingCount > 0) // Если еще не все кегли сбиты и фреймы не кончились
        {
            SpawnNewBall();
        }
        else
        {
            // Конец игры или фрейма
            Debug.Log("Frame/Game Over! Final Score: " + totalScore);
            // Здесь можно сбросить кегли
        }
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = "Score: " + totalScore.ToString();
        if (pinsLeftText) pinsLeftText.text = "Pins Left: " + pinsStandingCount.ToString();
    }
}