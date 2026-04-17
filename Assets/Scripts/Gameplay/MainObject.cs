using UnityEngine;

namespace Gameplay
{
    public class MainObject : SceneObjectBase
    {
        [SerializeField] private TrailRenderer _trail;

        public void EnableTrail() { if (_trail) _trail.enabled = true; }
        public void DisableTrail() { if (_trail) _trail.enabled = false; }
    }
}