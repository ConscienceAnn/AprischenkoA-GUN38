using UnityEngine;

namespace Netologia.Quest.Characters.Player
{
	public class CameraController : MonoBehaviour
	{
		private Vector3 _offset;
		private Transform _target;

		[SerializeField, Range(0.1f, 20f)]
		private float _speed = 4f;

		private void Awake()
		{
			var transform = this.transform;
			_target = transform.parent;
			transform.parent = null;

			_offset = _target.position - transform.position;
		}

		private void LateUpdate()
		{
			var transform = this.transform;
			var position = transform.position;
			var target = _target.position - _offset;

			transform.position = Vector3.Slerp(position, target, _speed * TimeManager.DeltaTime);
		}
	}
}