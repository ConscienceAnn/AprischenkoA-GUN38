using UnityEngine;

namespace Netologia.Quest.Characters
{
	[RequireComponent(typeof(Animator))]
	public class VisualController : MonoBehaviour
	{
		private static readonly int _moveHash = Animator.StringToHash("Move");
		private static readonly int _isMovingHash = Animator.StringToHash("IsMoving");

		private Vector3 _last;
		private Animator _animator;
		private BaseMovement _movement;
		
		private void Awake()
		{
			_animator = GetComponent<Animator>();
			_movement = GetComponentInParent<BaseMovement>();
			_last = transform.position;
		}

		private void LateUpdate()
		{
			var transform = this.transform;
			var current = transform.position;
			var speed = Vector3.Distance(_last, current) / Time.deltaTime;
			_animator.SetFloat(_moveHash, speed);
			
			_last = current;
			//todo может по speed определять IsMove здесь, а не через BaseMovement?
			_animator.SetBool(_isMovingHash, _movement.IsMove);
		}

#if UNITY_EDITOR
		
		[UnityEditor.CustomEditor(typeof(VisualController))]
		private class Editor : UnityEditor.Editor
		{
			private readonly GUIContent _distLabel = new("Distance");
			private readonly GUIContent _buttonLabel = new("Set Material");

			private VisualController _target;
			private Material _material;


			private void Awake()
			{
				_target = target as VisualController;
			}

			public override void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				if (!UnityEditor.EditorApplication.isPlaying)
					ShowMaterialSetter();
				else
					ShowDistance();
			}

			private void ShowMaterialSetter()
			{
				UnityEditor.EditorGUILayout.Space(30f);
				UnityEditor.EditorGUILayout.BeginHorizontal("helpbox");

				_material = UnityEditor.EditorGUILayout.ObjectField(GUIContent.none, _material, typeof(Material), false) as Material;
				if (GUILayout.Button(_buttonLabel) && _material != null)
				{
					var transform = _target.transform;
					for (int i = 0, iMax = transform.childCount; i < iMax; i++)
					{
						var child = transform.GetChild(i);
						if (child.TryGetComponent<SkinnedMeshRenderer>(out var renderer))
						{
							renderer.sharedMaterial = _material;
							UnityEditor.EditorUtility.SetDirty(renderer);
						}
					}					
				}
				UnityEditor.EditorGUILayout.EndHorizontal();
			}

			private void ShowDistance()
			{
				if (_target._movement == null) return;

				UnityEditor.EditorGUILayout.Space(10f);
				UnityEditor.EditorGUILayout.BeginVertical("box");

				var distance = _target._movement.IsMove
					? Vector3.Distance(_target._movement.EndPosition, _target.transform.position)
					: 0f;

				GUI.enabled = false;
				UnityEditor.EditorGUILayout.FloatField(_distLabel, distance);
				GUI.enabled = true;
				UnityEditor.EditorGUILayout.EndVertical();
			}
		}

#endif
	}
}