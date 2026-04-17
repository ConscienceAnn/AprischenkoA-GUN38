using System.Collections.Generic;
using UnityEngine;
using Core.MessageSystem;
using Messages;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay
{
    public class GameFieldManager : MonoBehaviour,
        IMessageListener<InputStarted>,
        IMessageListener<InputFinished>,
        IMessageListener<LevelRestarted>
    {
        [Header("Objects")]
        [SerializeField] private MainObject _mainObject;
        [SerializeField] private TargetObject _targetObject;
        [SerializeField] private Transform _fieldRoot; // Родитель всех MovePoint

        [Header("Grid Arrays")]
        [SerializeField] private MovePoint[] _movePoints;
        [SerializeField] private MovingZone[] _movingZones;
        [SerializeField] private List<StarObject> _stars = new();

        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 3f;

        [Header("Grid Settings")]
        [SerializeField] private int _totalColumns = 3;
        [SerializeField] private int _totalRows = 3;
        [SerializeField] private float _gridSize = 1f;
        [SerializeField] private GameObject _movePointPrefab;

        // Состояние для определения: движение или поворот
        private Vector2 _touchStartWorldPos;
        private bool _isRotateMode = false;

        // Для сброса уровня
        private Vector3 _mainObjectStartPos;
        private Quaternion _fieldStartRotation;
        private Dictionary<StarObject, bool> _starsInitialState = new();

        private void Awake()
        {
            Messenger.Subscribe<InputStarted>(this);
            Messenger.Subscribe<InputFinished>(this);
            Messenger.Subscribe<LevelRestarted>(this);

            SaveInitialState();
        }

        private void OnDestroy()
        {
            Messenger.Unsubscribe<InputStarted>(this);
            Messenger.Unsubscribe<InputFinished>(this);
            Messenger.Unsubscribe<LevelRestarted>(this);
        }

        private void SaveInitialState()
        {
            _mainObjectStartPos = _mainObject.transform.position;
            _fieldStartRotation = _fieldRoot.rotation;

            foreach (var star in _stars)
            {
                _starsInitialState[star] = star.gameObject.activeSelf;
            }
        }

        public void OnMessage(InputStarted message)
        {
            _touchStartWorldPos = message.WorldPosition;

            // Проверяем: нажали на шарик или мимо?
            RaycastHit2D hit = Physics2D.Raycast(message.WorldPosition, Vector2.zero);
            _isRotateMode = (hit.collider == null || hit.collider.gameObject != _mainObject.gameObject);
        }

        public void OnMessage(InputFinished message)
        {
            if (_isRotateMode)
            {
                RotateField(message.WorldPosition);
            }
            else
            {
                MoveBall(message.WorldPosition);
            }
        }

        private void RotateField(Vector2 endWorldPos)
        {
            Vector2 direction = (endWorldPos - _touchStartWorldPos).normalized;
            float angle = 0;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                angle = direction.x > 0 ? -90 : 90;
            }
            else
            {
                angle = direction.y > 0 ? 90 : -90;
            }

            StartCoroutine(RotateCoroutine(angle));
        }

        private System.Collections.IEnumerator RotateCoroutine(float targetAngle)
        {
            Messenger.Send(new SetInputActiveState { IsActive = false });

            Quaternion startRot = _fieldRoot.rotation;
            Quaternion endRot = startRot * Quaternion.Euler(0, 0, targetAngle);

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * _rotationSpeed;
                _fieldRoot.rotation = Quaternion.Lerp(startRot, endRot, t);
                yield return null;
            }

            _fieldRoot.rotation = endRot;
            Messenger.Send(new SetInputActiveState { IsActive = true });
        }

        private void MoveBall(Vector2 endWorldPos)
        {
            Vector2 direction = (endWorldPos - _touchStartWorldPos).normalized;
            Vector2Int gridDir = Vector2Int.zero;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                gridDir = new Vector2Int(direction.x > 0 ? 1 : -1, 0);
            else
                gridDir = new Vector2Int(0, direction.y > 0 ? 1 : -1);

            Vector2 targetPos = _mainObject.Position + gridDir;

            // Ищем точку и проверяем зону
            MovePoint targetPoint = FindMovePointAt(targetPos);
            if (targetPoint != null && IsPointInMovingZone(targetPoint))
            {
                StartCoroutine(MoveCoroutine(targetPoint));
            }
        }

        private System.Collections.IEnumerator MoveCoroutine(MovePoint targetPoint)
        {
            Messenger.Send(new SetInputActiveState { IsActive = false });

            Vector3 startPos = _mainObject.transform.position;
            Vector3 endPos = targetPoint.transform.position;

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * _moveSpeed;
                _mainObject.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            _mainObject.transform.position = endPos;

            Messenger.Send(new ObjectMoved());
            Messenger.Send(new SetInputActiveState { IsActive = true });

            CheckWinCondition();
        }

        private MovePoint FindMovePointAt(Vector2 position)
        {
            foreach (var point in _movePoints)
            {
                if (point.IsOnSamePosition(position))
                    return point;
            }
            return null;
        }

        private bool IsPointInMovingZone(MovePoint point)
        {
            var currentZone = GetZoneAt(_mainObject.Position);
            var targetZone = GetZoneAt(point.Position);

            if (currentZone == null || targetZone == null)
                return false;

            return currentZone == targetZone || targetZone.HasIntersection(currentZone);
        }

        private MovingZone GetZoneAt(Vector2 position)
        {
            foreach (var zone in _movingZones)
            {
                var point = FindMovePointAt(position);
                if (point != null && zone.ContainsPoint(point))
                    return zone;
            }
            return null;
        }

        private void CheckWinCondition()
        {
            if (_mainObject.IsOnSamePosition(_targetObject))
            {
                Debug.Log("LEVEL COMPLETED!");
                Messenger.Send(new LevelCompleted());
                Messenger.Send(new SetInputActiveState { IsActive = false });
            }
        }

        public void OnMessage(LevelRestarted message)
        {
            StopAllCoroutines();

            _mainObject.transform.position = _mainObjectStartPos;
            _fieldRoot.rotation = _fieldStartRotation;

            foreach (var star in _stars)
            {
                star.gameObject.SetActive(_starsInitialState[star]);
            }

            Messenger.Send(new SetInputActiveState { IsActive = true });
        }


        private void OnValidate()
        {
            if (_movePointPrefab != null)
                Debug.Log($"MovePoint prefab assigned: {_movePointPrefab.name}");
            else
                Debug.Log("MovePoint prefab is NULL");
        }

        #region Grid Generation

        /// <summary>
        /// Рассчитывает X-координату границы сетки
        /// </summary>
        public float GetXGridValue()
        {
            return _gridSize * (_totalColumns / 2) + _gridSize / 2 * (_totalColumns % 2 - 1);
        }

        /// <summary>
        /// Рассчитывает Y-координату границы сетки
        /// </summary>
        public float GetYGridValue()
        {
            return _gridSize * (_totalRows / 2) + _gridSize / 2 * (_totalRows % 2 - 1);
        }

        /// <summary>
        /// Создает сетку точек
        /// </summary>
        public void GenerateGrid()
        {
#if UNITY_EDITOR
            // 1. Очищаем старые точки
            ClearMovePoints();

            // 2. Создаем контейнер для точек, если его нет
            if (_fieldRoot == null)
            {
                var fieldRootGO = transform.Find("FieldRoot");
                if (fieldRootGO != null)
                    _fieldRoot = fieldRootGO;
                else
                {
                    _fieldRoot = new GameObject("FieldRoot").transform;
                    _fieldRoot.SetParent(transform);
                    _fieldRoot.localPosition = Vector3.zero;
                    _fieldRoot.localRotation = Quaternion.identity;
                }
            }

            // 3. Рассчитываем количество точек
            int totalPoints = _totalColumns * _totalRows;
            _movePoints = new MovePoint[totalPoints];

            // 4. Создаем точки
            float startX = -GetXGridValue();
            float startY = -GetYGridValue();
            int index = 0;

            for (int row = 0; row < _totalRows; row++)
            {
                for (int col = 0; col < _totalColumns; col++)
                {
                    Vector3 position = new Vector3(
                        startX + col * _gridSize,
                        startY + row * _gridSize,
                        0
                    );

                    MovePoint point = CreateMovePoint($"Point_{col}_{row}", position);
                    _movePoints[index] = point;
                    index++;
                }
            }

            // 5. Сохраняем изменения
            EditorUtility.SetDirty(this);
#endif
        }

        private MovePoint CreateMovePoint(string name, Vector3 position)
        {
#if UNITY_EDITOR
            GameObject pointGO;

            if (_movePointPrefab != null)
            {
                pointGO = (GameObject)PrefabUtility.InstantiatePrefab(_movePointPrefab, _fieldRoot);
            }
            else
            {
                pointGO = new GameObject(name);
                pointGO.transform.SetParent(_fieldRoot);
                pointGO.AddComponent<MovePoint>();
            }

            pointGO.name = name;
            pointGO.transform.position = position;

            // Если нет SpriteRenderer, добавляем для видимости
            if (pointGO.GetComponent<SpriteRenderer>() == null)
            {
                var sr = pointGO.AddComponent<SpriteRenderer>();
                sr.sprite = CreateDefaultSprite();
                sr.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            }

            return pointGO.GetComponent<MovePoint>();
#else
            return null;
#endif
        }

        private void ClearMovePoints()
        {
#if UNITY_EDITOR
            if (_movePoints != null)
            {
                foreach (var point in _movePoints)
                {
                    if (point != null)
                        DestroyImmediate(point.gameObject);
                }
            }

            // Также удаляем все дочерние объекты из FieldRoot
            if (_fieldRoot != null)
            {
                for (int i = _fieldRoot.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(_fieldRoot.GetChild(i).gameObject);
                }
            }
#endif
        }

        private Sprite CreateDefaultSprite()
        {
#if UNITY_EDITOR
            // Создаем простой квадрат 1x1 для спрайта
            var texture = new Texture2D(64, 64);
            var colors = new Color[64 * 64];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.white;
            texture.SetPixels(colors);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
#else
            return null;
#endif
        }

        #endregion
    }
}