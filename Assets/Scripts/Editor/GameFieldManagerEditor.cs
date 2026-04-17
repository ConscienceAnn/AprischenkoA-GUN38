using UnityEngine;
using UnityEditor;
using Gameplay;

namespace Editor
{
    [CustomEditor(typeof(GameFieldManager))]
    public class GameFieldManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _totalColumnsProp;
        private SerializedProperty _totalRowsProp;
        private SerializedProperty _gridSizeProp;
        private SerializedProperty _movePointPrefabProp;

        private void OnEnable()
        {
            _totalColumnsProp = serializedObject.FindProperty("_totalColumns");
            _totalRowsProp = serializedObject.FindProperty("_totalRows");
            _gridSizeProp = serializedObject.FindProperty("_gridSize");
            _movePointPrefabProp = serializedObject.FindProperty("_movePointPrefab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // === Секция: Grid Settings (кастомная) ===
            EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_totalColumnsProp, new GUIContent("Columns"));
            EditorGUILayout.PropertyField(_totalRowsProp, new GUIContent("Rows"));
            EditorGUILayout.PropertyField(_gridSizeProp, new GUIContent("Grid Size"));
            EditorGUILayout.PropertyField(_movePointPrefabProp, new GUIContent("Move Point Prefab"));

            EditorGUILayout.Space(5);

            // === Кнопка генерации ===
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("GENERATE GRID", GUILayout.Height(30)))
            {
                int cols = _totalColumnsProp.intValue;
                int rows = _totalRowsProp.intValue;

                if (EditorUtility.DisplayDialog(
                    "Generate Grid",
                    $"Create {cols}x{rows} grid?\nExisting points will be deleted.",
                    "Yes", "Cancel"))
                {
                    GameFieldManager manager = (GameFieldManager)target;
                    manager.GenerateGrid();
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            // === ВСЕ ОСТАЛЬНЫЕ ПОЛЯ (кроме тех, что уже нарисовали) ===
            // Исключаем: то, что уже нарисовали + m_Script
            string[] excludedFields =
            {
                "_totalColumns",
                "_totalRows",
                "_gridSize",
                "_movePointPrefab",
                "m_Script"
            };

            DrawPropertiesExcluding(serializedObject, excludedFields);

            // Применяем изменения
            serializedObject.ApplyModifiedProperties();
        }
    }
}