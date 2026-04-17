using UnityEngine;
using UnityEditor;
using Gameplay;

namespace Editor
{
    [CustomEditor(typeof(MovingZone))]
    public class MovingZoneEditor : UnityEditor.Editor
    {
        private MovingZone _zone;
        private SerializedProperty _coveredPointsProp;
        private SerializedProperty _autoDetectPointsProp;

        private void OnEnable()
        {
            _zone = (MovingZone)target;
            _coveredPointsProp = serializedObject.FindProperty("_coveredPoints");
            _autoDetectPointsProp = serializedObject.FindProperty("_autoDetectPoints");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Moving Zone Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Автоопределение точек
            EditorGUILayout.PropertyField(_autoDetectPointsProp, new GUIContent("Auto Detect Points"));

            EditorGUILayout.Space(10);

            // Кнопка ручного обновления
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("REFRESH COVERED POINTS", GUILayout.Height(25)))
            {
                _zone.AutoDetectPoints();
                EditorUtility.SetDirty(_zone);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // Показываем список точек (только для чтения)
            EditorGUILayout.LabelField($"Covered Points: {_zone.CoveredPoints.Count}", EditorStyles.boldLabel);

            GUI.enabled = false;
            EditorGUILayout.PropertyField(_coveredPointsProp, new GUIContent("Points List"), true);
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            // Визуальное редактирование размера зоны
            MovingZone zone = (MovingZone)target;

            // Ручки для изменения размера
            Vector3 center = zone.transform.position;
            float size = HandleUtility.GetHandleSize(center) * 0.1f;

            // Правая ручка
            Vector3 rightPos = center + Vector3.right * zone.GetComponent<SpriteRenderer>().bounds.extents.x;
            EditorGUI.BeginChangeCheck();
            Vector3 newRight = Handles.FreeMoveHandle(rightPos, size, Vector3.zero, Handles.RectangleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                float newWidth = Mathf.Abs(newRight.x - center.x) * 2;
                if (newWidth > 0.1f)
                {
                    var sr = zone.GetComponent<SpriteRenderer>();
                    Vector2 newSize = sr.size;
                    newSize.x = newWidth;
                    sr.size = newSize;
                    zone.AutoDetectPoints();
                }
            }

            // Верхняя ручка
            Vector3 upPos = center + Vector3.up * zone.GetComponent<SpriteRenderer>().bounds.extents.y;
            EditorGUI.BeginChangeCheck();
            Vector3 newUp = Handles.FreeMoveHandle(upPos, size, Vector3.zero, Handles.RectangleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                float newHeight = Mathf.Abs(newUp.y - center.y) * 2;
                if (newHeight > 0.1f)
                {
                    var sr = zone.GetComponent<SpriteRenderer>();
                    Vector2 newSize = sr.size;
                    newSize.y = newHeight;
                    sr.size = newSize;
                    zone.AutoDetectPoints();
                }
            }
        }
    }
}