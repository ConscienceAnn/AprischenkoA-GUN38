using UnityEngine;

namespace Gameplay
{
    public class MainObject : SceneObjectBase
    {
        [SerializeField] private TrailRenderer _trail;

        private void Awake()
        {
            if (_trail == null)
                _trail = GetComponent<TrailRenderer>();

            // Очищаем трейл при старте
            if (_trail != null)
            {
                _trail.Clear();
                _trail.emitting = true;
            }
        }

        public void EnableTrail()
        {
            if (_trail)
            {
                _trail.emitting = true;   
                _trail.enabled = true;
                _trail.Clear();
            }
        }

        public void DisableTrail()
        {

            if (_trail != null)
            {
                _trail.emitting = false;   
                _trail.enabled = false;
            }
        }

        public void ClearTrail()
        {
            if (_trail != null)
            {
                _trail.Clear();
            }
        }
    }
}