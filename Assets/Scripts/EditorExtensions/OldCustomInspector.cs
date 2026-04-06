#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Netologia.Quest.Editor
{
	public abstract class OldCustomInspector : UnityEditor.Editor
	{
		private const int c_CountInOneLine = 3;
		private readonly static Color[] _colors = new Color[]
		{
			new Color(1f, 1f, 0f, 1f), new Color(1f, .8f, 0f, 1f), new Color(1f, .6f, 0f, 1f), new Color(1f, .4f, 0f, 1f),
			new Color(.8f, 1f, 0f, 1f), new Color(.6f, 1f, 0f, 1f), new Color(.4f, 1f, 0f, 1f), new Color(.2f, 1f, 0f, 1f)
		};

		private List<Data> _datas = null;
		private GUIStyle _style;

		private readonly struct Data
		{
			public readonly string Name;
			public readonly Action Method;

			public Data(string name, Action method)
				=> (Name, Method) = (name, method);
		}

		private void OnEnable()
		{
			var type = target.GetType();
			var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			_datas = new List<Data>(methods.Length);
			foreach (var method in methods)
			{
				var attr = method.GetCustomAttribute<ContextMenu>(true);
				if (attr != null)
					_datas.Add(new Data(attr.menuItem, method.CreateDelegate(typeof(Action), target) as Action));
			}

			_style = CreateInstance<GUISkin>().box;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (_datas is null) return;

			EditorGUILayout.Space(10f);
			EditorGUILayout.BeginHorizontal(_style);

			for (int i = 0, count = c_CountInOneLine, iMax = _datas.Count; i < iMax; i++)
			{
				GUI.color = _colors[i % _colors.Length];
				if (GUILayout.Button(_datas[i].Name))
				{
					_datas[i].Method.Invoke();

					EditorUtility.SetDirty(target);
					var mono = target as MonoBehaviour;

					if (mono != null)
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mono.gameObject.scene);
				}

				count--;
				if (count <= 0)
				{
					count = c_CountInOneLine;
					EditorGUILayout.EndHorizontal();
					GUI.color = Color.white;
					EditorGUILayout.BeginHorizontal(_style);
					GUI.color = _colors[i % _colors.Length];
				}
			}

			GUI.color = Color.white;
			EditorGUILayout.EndHorizontal();
		}
	}
}

#endif