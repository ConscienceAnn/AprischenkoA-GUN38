using UnityEngine;
using Core.MessageSystem;
using Messages;

namespace Gameplay
{
    public class InputManager : MonoBehaviour, IMessageListener<SetInputActiveState>
    {
        [SerializeField] private Camera _mainCamera;
        private Vector2 _startTouchPosition;
        private bool _isInputEnabled = true;

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            Messenger.Subscribe<SetInputActiveState>(this);
        }

        private void OnDestroy()
        {
            Messenger.Unsubscribe<SetInputActiveState>(this);
        }

        private void Update()
        {
            if (!_isInputEnabled) return;

            // Игнорируем ввод, если нажали на UI
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            ProcessInput();
        }

        private void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _startTouchPosition = Input.mousePosition;
                var worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Messenger.Send(new InputStarted(worldPos));
            }

            if (Input.GetMouseButtonUp(0))
            {
                var worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Messenger.Send(new InputFinished(worldPos));
            }
        }

        public void OnMessage(SetInputActiveState message)
        {
            _isInputEnabled = message.IsActive;
        }
    }
}