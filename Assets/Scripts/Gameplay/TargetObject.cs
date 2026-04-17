using UnityEngine;

namespace Gameplay
{
    public class TargetObject : SceneObjectBase 
    {
        private Quaternion _fixedRotation;

        private void Start()
        {
            _fixedRotation = Quaternion.identity;
        }

        private void LateUpdate()
        {
            // Компенсируем вращение родителя
            transform.rotation = _fixedRotation;
        }
    }
}
