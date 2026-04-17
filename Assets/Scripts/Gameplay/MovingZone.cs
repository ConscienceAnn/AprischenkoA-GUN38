using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class MovingZone : MonoBehaviour
    {
        [SerializeField] private List<MovePoint> _coveredPoints = new();

        public bool ContainsPoint(MovePoint point)
        {
            return _coveredPoints.Contains(point);
        }

        public bool HasIntersection(MovingZone other)
        {
            foreach (var point in _coveredPoints)
            {
                if (other._coveredPoints.Contains(point))
                    return true;
            }
            return false;
        }

        // Для редактора
        public void AddPoint(MovePoint point)
        {
            if (!_coveredPoints.Contains(point))
                _coveredPoints.Add(point);
        }
    }
}