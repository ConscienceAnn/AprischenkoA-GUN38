using UnityEngine;

namespace Gameplay
{
    public class MovingZone : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Проверяет, находится ли точка внутри зоны (в реальном времени)
        /// </summary>
        public bool ContainsPoint(MovePoint point)
        {
            if (point == null || _spriteRenderer == null)
                return false;

            return ContainsPosition(point.transform.position);
        }

        /// <summary>
        /// Проверяет, находится ли позиция внутри зоны
        /// </summary>
        public bool ContainsPosition(Vector3 worldPosition)
        {
            if (_spriteRenderer == null)
                return false;

            Bounds bounds = _spriteRenderer.bounds;
            bounds.Expand(0.15f); // Небольшое расширение для надежности

            return bounds.Contains(worldPosition);
        }

        /// <summary>
        /// Проверяет, есть ли пересечение с другой зоной
        /// </summary>
        public bool HasIntersection(MovingZone other)
        {
            if (other == null) return false;

            // Находим все точки в сцене
            MovePoint[] allPoints = FindObjectsOfType<MovePoint>();

            foreach (var point in allPoints)
            {
                if (point != null)
                {
                    // Если точка находится в обеих зонах одновременно — есть пересечение
                    if (ContainsPoint(point) && other.ContainsPoint(point))
                        return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer != null)
            {
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                Gizmos.DrawWireCube(_spriteRenderer.bounds.center, _spriteRenderer.bounds.size);
            }
        }
#endif
    }
}