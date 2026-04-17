using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class MovingZone : MonoBehaviour
    {
        [SerializeField] private List<MovePoint> _coveredPoints = new();
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private bool _autoDetectPoints = true;

        // Публичное свойство для доступа к точкам
        public IReadOnlyList<MovePoint> CoveredPoints => _coveredPoints;

        private void OnValidate()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_autoDetectPoints)
            {
                // Не обновляем автоматически в рантайме
#if UNITY_EDITOR
                if (!Application.isPlaying && this != null && gameObject != null)
                {
                    // Используем задержку, чтобы избежать ошибок при уничтожении
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        if (this != null && gameObject != null)
                        {
                            AutoDetectPoints();
                        }
                    };
                }
#endif
            }
        }

        /// <summary>
        /// Автоматически находит все точки, которые находятся внутри зоны
        /// </summary>
        public void AutoDetectPoints()
        {
#if UNITY_EDITOR
            // Проверяем, что объект не уничтожен
            if (this == null || gameObject == null)
                return;

            _coveredPoints.Clear();

            // Получаем SpriteRenderer, если он потерялся
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer == null)
            {
                Debug.LogWarning("MovingZone: SpriteRenderer not found!");
                return;
            }

            // Ищем все MovePoint в сцене
            MovePoint[] allPoints = FindObjectsOfType<MovePoint>();

            foreach (var point in allPoints)
            {
                if (point != null && IsPointInsideZone(point.transform.position))
                {
                    _coveredPoints.Add(point);
                }
            }

            // Сортируем для красоты (опционально)
            _coveredPoints.Sort((a, b) =>
            {
                if (a == null || b == null) return 0;
                int yCompare = a.transform.position.y.CompareTo(b.transform.position.y);
                if (yCompare != 0) return yCompare;
                return a.transform.position.x.CompareTo(b.transform.position.x);
            });

            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Проверяет, находится ли точка внутри зоны
        /// </summary>
        private bool IsPointInsideZone(Vector3 pointPosition)
        {
            // Проверяем, что объект существует
            if (this == null || gameObject == null)
                return false;

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer == null)
                return false;

            // Получаем границы спрайта
            Bounds bounds = _spriteRenderer.bounds;

            // Немного расширяем границы для надежности (погрешность)
            bounds.Expand(0.2f);

            return bounds.Contains(pointPosition);
        }

        /// <summary>
        /// Проверяет, содержит ли зона указанную точку
        /// </summary>
        public bool ContainsPoint(MovePoint point)
        {
            if (point == null) return false;
            return _coveredPoints.Contains(point);
        }

        /// <summary>
        /// Проверяет, есть ли пересечение с другой зоной (общие точки)
        /// </summary>
        public bool HasIntersection(MovingZone other)
        {
            if (other == null) return false;

            foreach (var point in _coveredPoints)
            {
                if (point != null && other._coveredPoints.Contains(point))
                    return true;
            }
            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Отрисовка зоны в редакторе (для наглядности)
        /// </summary>
        private void OnDrawGizmos()
        {
            if (this == null) return;

            // Рисуем связи с точками
            if (_coveredPoints != null && _coveredPoints.Count > 0)
            {
                Gizmos.color = Color.yellow;
                foreach (var point in _coveredPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawLine(transform.position, point.transform.position);
                        Gizmos.DrawWireSphere(point.transform.position, 0.15f);
                    }
                }
            }

            // Рисуем границы зоны
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                Gizmos.DrawWireCube(_spriteRenderer.bounds.center, _spriteRenderer.bounds.size);
            }
        }
#endif

        /// <summary>
        /// Контекстное меню для ручного обновления точек
        /// </summary>
        [ContextMenu("Refresh Covered Points")]
        public void RefreshPoints()
        {
            AutoDetectPoints();
        }
    }
}