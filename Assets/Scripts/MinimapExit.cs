using UnityEngine;

public class MinimapExit : MonoBehaviour
{
    private StaticMinimap minimap;

    void Start()
    {
        // Находим миникарту
        minimap = FindObjectOfType<StaticMinimap>();

        // Регистрируем выход на миникарте
        if (minimap != null)
        {
            minimap.RegisterExit(gameObject);
        }
    }

    void OnDestroy()
    {
        // Удаляем с миникарты при уничтожении
        if (minimap != null)
        {
            minimap.UnregisterExit(gameObject);
        }
    }
}