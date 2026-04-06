using UnityEngine;

namespace Netologia.Quest
{
	/// <summary>
	/// Анимирование курсора и метки фокуса
	/// </summary>
	public class TransformMode : MonoBehaviour
	{
		private Transform _transform;
		private Vector3? _rotate;
		private bool _scaler;
		private float _time;
		
		[SerializeField]
		private float _rotationSpeed;
		[SerializeField]
		private AnimationCurve _scales;
		[SerializeField]
		private float _scaleSpeed;

		private void Awake()
		{
			_rotate = _rotationSpeed == 0f 
				? null : new Vector3(0f, _rotationSpeed, 0f);
			_scaler = _scaleSpeed != 0f;
			_transform = transform;
		}

		private void Update()
		{
			var delta = Time.deltaTime;
			if (_rotate.HasValue)
				_transform.Rotate(_rotate.Value * delta, Space.Self);
			if (_scaler)
			{
				_time += _scaleSpeed * delta;
				if (_time > 1f) _time--;
				_transform.localScale = Vector3.one * _scales.Evaluate(_time);
			}
		}
	}
}