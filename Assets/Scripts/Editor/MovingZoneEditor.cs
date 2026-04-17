using UnityEngine;
using UnityEditor;
using Gameplay;

namespace Editor
{
    [CustomEditor(typeof(MovingZone))]
    public class MovingZoneEditor : UnityEditor.Editor
    {
        private MovingZone _zone;
        private SerializedProperty _spriteRendererProp;

        private void OnEnable()
        {
            _zone = (MovingZone)target;
            _spriteRendererProp = serializedObject.FindProperty("_spriteRenderer");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Moving Zone Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Поле для SpriteRenderer
            EditorGUILayout.PropertyField(_spriteRendererProp, new GUIContent("Sprite Renderer"));

            EditorGUILayout.Space(10);

            // Информация о зоне
            if (_zone != null && _zone.GetComponent<SpriteRenderer>() != null)
            {
                var bounds = _zone.GetComponent<SpriteRenderer>().bounds;
                EditorGUILayout.LabelField("Zone Info", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Size: {bounds.size.x:F2} x {bounds.size.y:F2}");
                EditorGUILayout.LabelField($"Center: {bounds.center}");
            }

            EditorGUILayout.Space(10);

            // Подсказка
            EditorGUILayout.HelpBox(
                "Точки определяются автоматически в реальном времени.\n" +
                "Используйте ручки в Scene View для изменения размера зоны.",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            MovingZone zone = (MovingZone)target;
            SpriteRenderer sr = zone.GetComponent<SpriteRenderer>();

            if (sr == null) return;

            Vector3 center = zone.transform.position;
            float handleSize = HandleUtility.GetHandleSize(center) * 0.1f;

            // Правая ручка для изменения ширины
            Vector3 rightPos = center + Vector3.right * sr.bounds.extents.x;
            EditorGUI.BeginChangeCheck();
            Vector3 newRight = Handles.FreeMoveHandle(
                rightPos,
                handleSize,
                Vector3.zero,
                Handles.RectangleHandleCap
            );

            if (EditorGUI.EndChangeCheck())
            {
                float newWidth = Mathf.Abs(newRight.x - center.x) * 2;
                if (newWidth > 0.1f)
                {
                    Undo.RecordObject(sr, "Resize MovingZone");
                    Vector2 newSize = sr.size;
                    newSize.x = newWidth;
                    sr.size = newSize;
                    EditorUtility.SetDirty(zone);
                }
            }

            // Верхняя ручка для изменения высоты
            Vector3 upPos = center + Vector3.up * sr.bounds.extents.y;
            EditorGUI.BeginChangeCheck();
            Vector3 newUp = Handles.FreeMoveHandle(
                upPos,
                handleSize,
                Vector3.zero,
                Handles.RectangleHandleCap
            );

            if (EditorGUI.EndChangeCheck())
            {
                float newHeight = Mathf.Abs(newUp.y - center.y) * 2;
                if (newHeight > 0.1f)
                {
                    Undo.RecordObject(sr, "Resize MovingZone");
                    Vector2 newSize = sr.size;
                    newSize.y = newHeight;
                    sr.size = newSize;
                    EditorUtility.SetDirty(zone);
                }
            }

            // Рисуем границы зоны
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Handles.DrawWireCube(sr.bounds.center, sr.bounds.size);
        }
    }
}