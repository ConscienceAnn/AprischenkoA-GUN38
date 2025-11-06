using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GameObject _restartPanel;
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _fillDuration = 2f; // за сколько секунд заполнится

    private GameControls.GameActions _gameActions; // будет инжектиться
    private bool _isHolding = false;
    private float _fillTimer = 0f;

    [Inject]
    public void Construct(GameControls.GameActions gameActions)
    {
        _gameActions = gameActions;
    }

    private void OnEnable()
    {
        _gameActions.Restart.started += OnRestartStarted;
        _gameActions.Restart.canceled += OnRestartCanceled;
        _gameActions.Enable();
    }

    private void OnDisable()
    {
        _gameActions.Disable();
        _gameActions.Restart.started -= OnRestartStarted;
        _gameActions.Restart.canceled -= OnRestartCanceled;
    }

    private void OnRestartStarted(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        _isHolding = true;
        _fillTimer = 0f;
        _restartPanel.SetActive(true);
    }

    private void OnRestartCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        _isHolding = false;
        _restartPanel.SetActive(false);

        // Если шкала была заполнена — перезагружаем
        if (_fillTimer >= _fillDuration)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void Update()
    {
        if (_isHolding)
        {
            _fillTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_fillTimer / _fillDuration);
            _fillImage.fillAmount = progress;
        }
    }
}
