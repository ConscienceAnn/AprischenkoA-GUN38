using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private PinManager _pinManager;
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private Transform _ballSpawnPoint;
    [SerializeField] private Button newGameButton;

    [Header("Settings")]
    [SerializeField] private float _resetDelay = 2f;

    private ScoreCalculator _scoreCalculator;
    private BallController _currentBall;
    private bool _isGameActive = true;
    private bool _isThrowInProgress = false;

    // Кешированные YieldInstructions
    private WaitForSeconds _waitHalfSecond;
    private WaitForSeconds _waitOneSecond;
    private WaitForSeconds _waitResetDelay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("GameManager: Awake - Instance создан");
        }
        else
        {
            Debug.LogWarning("GameManager: Awake - Уничтожаем дубликат");
            Destroy(gameObject);
            
        }

        InitializeWaitInstructions();
    }

    private void InitializeWaitInstructions()
    {
        _waitHalfSecond = new WaitForSeconds(0.5f);
        _waitOneSecond = new WaitForSeconds(1f);
        _waitResetDelay = new WaitForSeconds(_resetDelay);
        Debug.Log("GameManager: WaitInstructions инициализированы");
    }

    private void Start()
    {
        Debug.Log("GameManager: Start - начало игры");
        StartNewGame();

    }

    public void StartNewGame()
    {
        Debug.Log("StartNewGame вызван");

        if (newGameButton != null)
        {
            Debug.Log($"Кнопка найдена. Устанавливаем active = false");
            newGameButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Кнопка не найдена!");
        }

        Debug.Log("=== НАЧАЛО НОВОЙ ИГРЫ ===");
        _scoreCalculator = new ScoreCalculator();
        _scoreCalculator.StartNewGame();

        _isGameActive = true;
        _isThrowInProgress = false;

        _uiManager.HideGameOver();
        _pinManager.SpawnPins();
        SpawnNewBall();
        UpdateUI();
    }

    private void SpawnNewBall()
    {
        if (!_isGameActive || _scoreCalculator.GetCurrentFrameIndex() >= 10)
            
            return;

        if (_currentBall != null)
        {
            Destroy(_currentBall.gameObject);
        }

        GameObject ballObj = Instantiate(_ballPrefab, _ballSpawnPoint.position, Quaternion.identity);
        _currentBall = ballObj.GetComponent<BallController>();
        _currentBall.Initialize(this);

        _isThrowInProgress = false;

        Debug.Log(_scoreCalculator.GetDebugInfo());
        UpdateUI();
    }

    public void OnBallThrown()
    {
        if (!_isGameActive || _isThrowInProgress)
            return;

        _isThrowInProgress = true;
        StartCoroutine(ProcessThrow());
    }

   
    private IEnumerator ProcessThrow()
    {
        // 1. Ждем пока мяч либо достигнет кеглей, либо улетит
        yield return new WaitForSeconds(2f);

        // Ждем пока кегли успокоятся
        yield return StartCoroutine(WaitForPinsToSettle());

        // Уничтожаем мяч
        if (_currentBall != null)
        {
            Destroy(_currentBall.gameObject);
            _currentBall = null;
        }

        int pinsStandingAfterThrow = _pinManager.CountStandingPins();
        int currentThrow = _scoreCalculator.GetCurrentThrowInFrame();
        int pinsKnockedDownThisThrow = 0;


        if (currentThrow == 1)
        {
            pinsKnockedDownThisThrow = 10 - pinsStandingAfterThrow;
            Debug.Log($"Фрейм {_scoreCalculator.GetCurrentFrameIndex() + 1}, бросок 1: сбито {pinsKnockedDownThisThrow} кеглей");
        }
        else // currentThrow == 2
        { 
            int pinsLeftAfterFirstThrow = _scoreCalculator.GetPinsLeftAfterFirstThrow();
            pinsKnockedDownThisThrow = pinsLeftAfterFirstThrow - pinsStandingAfterThrow;

            Debug.Log($"Фрейм {_scoreCalculator.GetCurrentFrameIndex() + 1}, бросок 2: " +
                     $"сбито {pinsKnockedDownThisThrow} кеглей (оставалось {pinsLeftAfterFirstThrow})");
        }


        _scoreCalculator.RecordThrow(pinsKnockedDownThisThrow, currentThrow);
        int totalScore = _scoreCalculator.CalculateTotalScore();

        UpdateUI();

        if (_scoreCalculator.GetCurrentFrameIndex() >= 10)
        {
            EndGame(totalScore);
            yield break;
        }


        yield return _waitOneSecond;

        if (currentThrow == 1 && pinsKnockedDownThisThrow < 10) // Не страйк
        {

            Debug.Log("Не страйк, создаем мяч для второго броска");
            SpawnNewBall();
        }
        else 
        {
            Debug.Log("Страйк или фрейм завершен, создаем новые кегли");
            yield return _waitResetDelay;
            _pinManager.SpawnPins(); // Создаем новые кегли
            yield return _waitHalfSecond;
            SpawnNewBall();
        }
    }


    private IEnumerator WaitForPinsToSettle()
    {
        int checksWithoutMovement = 0;
        const int maxChecksWithoutMovement = 8;

        while (checksWithoutMovement < maxChecksWithoutMovement)
        {
            if (_pinManager.CountStandingPins() == 0)
            {
                yield break;
            }

            if (_pinManager.IsAnyStandingPinMoving())
            {
                checksWithoutMovement = 0;
            }
            else
            {
                checksWithoutMovement++;
            }

            yield return _waitHalfSecond;
        }
    }

    private void UpdateUI()
    {
        int totalScore = _scoreCalculator.CalculateTotalScore();
        int pinsStanding = _pinManager.CountStandingPins();
        int currentThrow = _scoreCalculator.GetCurrentThrowInFrame();
        int currentFrame = _scoreCalculator.GetCurrentFrameIndex() + 1;

        Debug.Log($"=== UpdateUI ===");
        Debug.Log($"Счет: {totalScore}");
        Debug.Log($"Кеглей стоит: {pinsStanding}");
        Debug.Log($"Бросок: {currentThrow}");
        Debug.Log($"Фрейм: {currentFrame}");
        Debug.Log($"=== Конец UpdateUI ===");

        _uiManager.UpdateScore(totalScore);
        _uiManager.UpdatePinsLeft(pinsStanding);
        _uiManager.UpdateThrowInfo(currentThrow);
        _uiManager.UpdateFrameInfo(currentFrame);
    }

    private void EndGame(int finalScore)
    {
        _isGameActive = false;
        _uiManager.ShowGameOver(finalScore);

        ShowNewGameButton();
    }

    public void NewGameButton()
    {
        StartNewGame();
    }

    public void ShowNewGameButton()
    {
        if (newGameButton != null)
        {
            newGameButton.gameObject.SetActive(true);
        }
    }



}