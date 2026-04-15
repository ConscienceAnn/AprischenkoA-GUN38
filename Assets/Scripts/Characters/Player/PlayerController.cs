using Netologia.Quest.Talks;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Netologia.Quest.Characters.Player
{
    public class PlayerController : BaseMovement
    {
        private LineRenderer _renderer;
        private Controls _controls;
        private Camera _camera;
        private Transform _mouseTrans;      //курсор мышки
        private Transform _movePointTrans;  //точка текущего следования 
        
        private readonly Vector3[] _path = new Vector3[8];
        private float _maxCameraDistance;
        private Transform _currentTarget;
        private Character _currentCharacter;
        private bool _tryInteract;
        private bool _moveLock;

        [SerializeField]
        private LayerMask _hitcastMask;//"Floor" | "Interactables"
        
        [SerializeField]
        private float _offsetMarkY = .15f;
        [SerializeField, Range(0f, 10f)]
        private float _stoppingDistanceForCharacter = 4f;

        [SerializeField]
        private MeshRenderer _mouseFocus;
        [SerializeField]
        private MeshRenderer _movePoint;

        public List<QuestInfo> ActiveQuests { get; } = new(8);
        
        protected override void Awake()
        {
            base.Awake();
            _renderer = GetComponent<LineRenderer>();
            _renderer.positionCount = _path.Length;
            //todo disable if pause
            _controls = new Controls();
            _controls.Enable();
            _controls.Mouse.Click.performed += OnMoveClick;

            _camera = Camera.main;
            _maxCameraDistance = Vector3.SqrMagnitude(_camera.transform.position - transform.position);

            _mouseTrans = _mouseFocus.transform;
            _movePointTrans = _movePoint.transform;

            _mouseTrans.parent = null;
            _movePointTrans.parent = null;
            
            InformationBureau.OnDialogClose += OnCancelDialog;
        }
        
        private void Update()
        {
            var position = (Vector3)_controls.Mouse.Position.ReadValue<Vector2>();
            var ray = _camera.ScreenPointToRay(position);
            if (Physics.Raycast(ray, out var hit, _maxCameraDistance, _hitcastMask, QueryTriggerInteraction.Ignore))
            {
                _currentTarget = hit.transform;
                Debug.Log($"[Update] Попадание в: {_currentTarget.name}");
                position = _currentTarget.TryGetComponent<Character>(out var character)
                    ? character.CirclePosition
                    : hit.point;
                position.y += _offsetMarkY;
                _mouseTrans.position = position;
                _mouseFocus.enabled = true;
            }
            else
            {
                Debug.Log("[Update] Луч НИ ВО ЧТО НЕ ПОПАЛ");
                _mouseFocus.enabled = false;

            }
            if (IsMove)
            {
                var count = GetPath(_path);
                Array.Fill(_path, _path[count - 1], count, _path.Length - count);
            }
            else
            {
                Array.Fill(_path, default);
                //Disable move mark and talk
                if (_tryInteract)
                {
                    if(_currentCharacter != null && _currentCharacter.OnInteract(this))
						(_movePoint.enabled, _moveLock) = (false, true);
					_tryInteract = false;
				}
            }
            
            _renderer.SetPositions(_path);
        }
        
        private void OnMoveClick(InputAction.CallbackContext obj)
        {
            Debug.Log($"[OnMoveClick] _currentTarget = {(_currentTarget != null ? _currentTarget.name : "NULL")}");
            if (_moveLock) return;

            if (_currentTarget == null)
            {
                Debug.LogError("КЛИК ПРИ NULL!");  
                return;
            }

            var point = default(Vector3);
            _movePoint.enabled = true;
            //Floor click
            if (!_currentTarget.TryGetComponent(out _currentCharacter))
            {
                _tryInteract = false;
                point = _mouseTrans.position;
                _movePointTrans.position = point;
                SetPosition(point);
                _agent.stoppingDistance = 0f;
                return;
            }
            //Character click
            _tryInteract = true;
            point = _currentTarget.position;
            _movePointTrans.position = _currentCharacter.CirclePosition;
            SetPosition(point);
            _agent.stoppingDistance = _stoppingDistanceForCharacter;
        }
        
        private void OnCancelDialog() => _moveLock = false;
        
        private void OnDestroy()
        {
            _controls.Mouse.Click.performed -= OnMoveClick;
            InformationBureau.OnDialogClose += OnCancelDialog;
        }
    }
}
