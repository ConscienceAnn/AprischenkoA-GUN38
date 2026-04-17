using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Gameplay;

namespace Editor
{
    public static class RotateMenu
    {
        [MenuItem("Rotate/Create Level", false, 1)]
        private static void CreateLevel()
        {
            var scene = SceneManager.GetActiveScene();

            // Проверяем, не созданы ли уже менеджеры
            var existingManager = Object.FindObjectOfType<GameFieldManager>();
            if (existingManager != null)
            {
                EditorUtility.DisplayDialog("Error",
                    "Scene already contains GameFieldManager!", "OK");
                return;
            }

            // Создаем GameFieldManager
            var fieldManagerGO = new GameObject("GameFieldManager");
            var fieldManager = fieldManagerGO.AddComponent<GameFieldManager>();

            // Создаем InputManager
            var inputManagerGO = new GameObject("InputManager");
            inputManagerGO.AddComponent<InputManager>();

            // Создаем EventSystem (если нет)
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }

            // Создаем Main Camera (если нет)
            if (Camera.main == null)
            {
                var cameraGO = new GameObject("Main Camera");
                var camera = cameraGO.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5;
                camera.transform.position = new Vector3(1, 1, -10);
                cameraGO.tag = "MainCamera";
            }

            // Регистрируем Undo
            Undo.RegisterCreatedObjectUndo(fieldManagerGO, "Create GameFieldManager");
            Undo.RegisterCreatedObjectUndo(inputManagerGO, "Create InputManager");

            // Генерируем сетку по умолчанию 3x3
            fieldManager.GenerateGrid();

            Selection.activeObject = fieldManagerGO;

            Debug.Log("Level structure created! Select GameFieldManager and click 'Generate Grid' if needed.");
        }

        [MenuItem("Rotate/Clear Level", false, 2)]
        private static void ClearLevel()
        {
            if (!EditorUtility.DisplayDialog("Clear Level",
                "This will delete ALL objects from the scene except Camera. Continue?",
                "Yes", "Cancel"))
                return;

            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();

            foreach (var go in rootObjects)
            {
                // Не удаляем камеру
                if (go.GetComponent<Camera>() != null)
                    continue;

                Undo.DestroyObjectImmediate(go);
            }

            Debug.Log("Scene cleared!");
        }
    }
}