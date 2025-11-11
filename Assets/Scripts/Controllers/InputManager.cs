using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GameObject _restartPanel;
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _fillDuration = 2f;

    private GameInput.GameActions _gameActions;
    private bool _isHolding = false;
    private float _fillTimer = 0f;

    [Inject]
    public void Construct(GameInput.GameActions gameActions)
    {
        _gameActions = gameActions;
    }

    private void Start()
    {
        _restartPanel.SetActive(false);
        _fillImage.fillAmount = 0f;
    }

    private void OnEnable()
    {
        _gameActions.Restart.started += OnRestartStarted;
        _gameActions.Restart.canceled += OnRestartCanceled;
        _gameActions.Enable();
    }

    private void OnDisable()
    {
        _gameActions.Restart.started -= OnRestartStarted;
        _gameActions.Restart.canceled -= OnRestartCanceled;
        _gameActions.Disable();
    }

    private void OnRestartStarted(InputAction.CallbackContext context)
    {
        _isHolding = true;
        _fillTimer = 0f;
        _restartPanel.SetActive(true);
        Debug.Log("TAB pressed - starting restart sequence");
    }

    private void OnRestartCanceled(InputAction.CallbackContext context)
    {
        _isHolding = false;
        _restartPanel.SetActive(false);

        if (_fillTimer >= _fillDuration)
        {
            Debug.Log("Restart confirmed - reloading scene");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log("Restart cancelled");
            _fillImage.fillAmount = 0f;
        }
    }

    private void Update()
    {
        if (_isHolding)
        {
            _fillTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_fillTimer / _fillDuration);
            _fillImage.fillAmount = progress;

            if (_fillTimer >= _fillDuration)
            {
                // Автоматическая перезагрузка при полном заполнении
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}