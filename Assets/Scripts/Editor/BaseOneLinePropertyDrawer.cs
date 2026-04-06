using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Netologia.Quest.Editor
{
	/// <summary>
	/// Базовый класс для отрисовки сущностей в одну строчку
	/// </summary>
	public abstract class BaseOneLinePropertyDrawer : PropertyDrawer
	{
		private readonly float _fieldNamePercent = .3f;
		private readonly SerializedProperty[] _props;
		private readonly GUIContent[] _contents;
		private Rect[] _rects;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="internalPropertyCount">Кол-во полей в сущности</param>
		/// <param name="fieldNamePercent">Процентное соотношение ширины названия поля к значениям</param>
		protected BaseOneLinePropertyDrawer(int internalPropertyCount, float fieldNamePercent = .3f) 
			=> (_props, _fieldNamePercent, _contents) 
				= (new SerializedProperty[internalPropertyCount], fieldNamePercent, new GUIContent[internalPropertyCount]);

		/*public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			return base.CreatePropertyGUI(property);
		}*/

		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
		{
			//Prepare arrays
			Init(property.Copy());
			//Calculate rectangles
			float x = rect.x, width = rect.width * _fieldNamePercent;
			_rects[0] = new Rect(x, rect.y, width, rect.height);
			x += width;
			width = (1f - _fieldNamePercent) * rect.width / (_rects.Length - 1);
			for(int i = 1; i < _rects.Length; i++)
			{
				_rects[i] = new Rect(x, rect.y, width, rect.height);
				x += width;
			}
			//Draw Field name
			EditorGUI.LabelField(_rects[0], label);
			//Draw internal fields 
			for(int i = 0, j = 1; i < _props.Length; i++, j += 2)
			{
				EditorGUI.LabelField(_rects[j], _contents[i]);
				EditorGUI.PropertyField(_rects[j + 1], _props[i], GUIContent.none);
			}
		}

		private void Init(SerializedProperty property)
		{
			for(int i = 0; i < _props.Length; i++)
			{
				property.NextVisible(true);
				_props[i] = property.Copy();
				_contents[i] = new GUIContent(_props[i].name);
			}

			var count = _props.Length * 2 + 1;
			if(_rects is null || count != _rects.Length)
				_rects = new Rect[count];
		}
	}
}