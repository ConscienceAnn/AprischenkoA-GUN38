using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StaticMinimap : MonoBehaviour
{
    [Header("UI")]
    public RawImage minimapImage;
    public RectTransform iconContainer;
    public GameObject iconPrefab;

    [Header("Настройки камеры")]
    public float cameraHeight = 25f;
    public float cameraSize = 40f;
    public float cameraPadding = 5f;

    [Header("Иконки")]
    public Sprite playerIcon;
    public Sprite enemyIcon;
    public Sprite healthIcon;
    public Sprite ammoIcon;
    public Sprite weaponIcon;

    [Header("Цвета")]
    public Color playerColor = Color.green;
    public Color enemyColor = Color.red;
    public Color healthColor = Color.green;
    public Color ammoColor = Color.blue;
    public Color weaponColor = new Color(1f, 0.5f, 0f);

    [Header("Размеры")]
    public float playerSize = 25f;
    public float enemySize = 20f;
    public float pickupSize = 20f;

    [Header("Динамическое обновление")]
    public float updateInterval = 1f; // Проверка каждую секунду
    private float updateTimer;

    [Header("Выход")]
    public Sprite exitIconSprite;
    public Color exitIconColor = Color.yellow;
    public float exitIconSize = 20f;

    [Header("Ориентация карты")]
    public Transform exitDoor; // Ссылка на дверь выхода
    private float rotationAngle = 0f; // Угол поворота карты

    public float minimapSizeInPixels = 500f; // Размер миникарты в пикселях

    private Camera minimapCamera;
    private RenderTexture renderTexture;
    private WorldBounds worldBounds;
    private Transform playerTransform;
    private RectTransform playerIconRect;
    private Dictionary<GameObject, RectTransform> trackedIcons = new Dictionary<GameObject, RectTransform>();
    private Bounds mapBounds;

    void Start()
    {
        CreateMinimapCamera();
        FindWorldBounds();
        SetupCameraPosition();
        SetupRenderTexture();
        FindPlayer();
        CreatePlayerIcon();
        FindAllObjects();
        FindExitDoor();
        DebugContainerSetup();

        Debug.Log($"IconContainer size: {iconContainer.rect.width} x {iconContainer.rect.height}");
        Debug.Log($"IconContainer anchors: {iconContainer.anchorMin} - {iconContainer.anchorMax}");
    }

    void CreateMinimapCamera()
    {
        GameObject camObj = new GameObject("MinimapCamera");
        camObj.transform.SetParent(transform);
        minimapCamera = camObj.AddComponent<Camera>();

        // Убираем тег MainCamera
        minimapCamera.tag = "Untagged";

        // Удаляем AudioListener
        AudioListener listener = minimapCamera.GetComponent<AudioListener>();
        if (listener != null) Destroy(listener);

        // Настройки камеры
        minimapCamera.orthographic = true;
        minimapCamera.nearClipPlane = 0.1f;
        minimapCamera.farClipPlane = 100f;
        minimapCamera.depth = 1;
        minimapCamera.clearFlags = CameraClearFlags.Color;
        minimapCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    }

    void FindWorldBounds()
    {
        worldBounds = FindObjectOfType<WorldBounds>();
        if (worldBounds == null)
        {
            Debug.LogError("WorldBounds not found on scene!");
        }
    }

    void SetupCameraPosition()
    {
        if (worldBounds == null) return;

        Vector3 min = worldBounds.min.position;
        Vector3 max = worldBounds.max.position;

        Vector3 center = new Vector3(
            (min.x + max.x) / 2,
            cameraHeight,
            (min.z + max.z) / 2
        );

        float width = Mathf.Abs(max.x - min.x) + cameraPadding * 2;
        float height = Mathf.Abs(max.z - min.z) + cameraPadding * 2;
        float size = Mathf.Max(width, height) / 2;

        minimapCamera.transform.position = center;

        // Поворот будет применен в FindExitDoor()
        // Убираем фиксированный поворот отсюда

        minimapCamera.orthographicSize = size;

        mapBounds = new Bounds(center, new Vector3(width, 0, height));
    }

    void SetupRenderTexture()
    {
        // Создаем RenderTexture
        renderTexture = new RenderTexture(512, 512, 16);
        renderTexture.name = "MinimapRenderTexture";
        minimapCamera.targetTexture = renderTexture;

        // Назначаем на UI
        if (minimapImage != null)
        {
            minimapImage.texture = renderTexture;
        }
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void CreatePlayerIcon()
    {
        if (iconContainer == null || iconPrefab == null) return;

        GameObject iconObj = Instantiate(iconPrefab, iconContainer);
        iconObj.name = "PlayerIcon";

        playerIconRect = iconObj.GetComponent<RectTransform>();
        playerIconRect.sizeDelta = new Vector2(playerSize, playerSize);

        // ВЕРНУТЬ ОБРАТНО! Pivot в центре
        playerIconRect.pivot = new Vector2(0.5f, 0.5f);
        playerIconRect.anchorMin = new Vector2(0.5f, 0.5f);
        playerIconRect.anchorMax = new Vector2(0.5f, 0.5f);

        Image image = iconObj.GetComponent<Image>();
        image.sprite = playerIcon;
        image.color = playerColor;
    }

    void FindAllObjects()
    {
        // Враги
        AiAgent[] enemies = FindObjectsOfType<AiAgent>();
        foreach (var enemy in enemies)
        {
            AddIcon(enemy.gameObject, enemyIcon, enemyColor, enemySize);
        }

        // Аптечки
        HealthPickup[] healths = FindObjectsOfType<HealthPickup>();
        foreach (var health in healths)
        {
            AddIcon(health.gameObject, healthIcon, healthColor, pickupSize);
        }

        // Патроны
        AmmoPickup[] ammos = FindObjectsOfType<AmmoPickup>();
        foreach (var ammo in ammos)
        {
            AddIcon(ammo.gameObject, ammoIcon, ammoColor, pickupSize);
        }

        // Оружие
        WeaponPickup[] weapons = FindObjectsOfType<WeaponPickup>();
        foreach (var weapon in weapons)
        {
            AddIcon(weapon.gameObject, weaponIcon, weaponColor, pickupSize);
        }
    }

    void AddIcon(GameObject target, Sprite sprite, Color color, float size)
    {
        if (target == null) return;
        if (trackedIcons.ContainsKey(target)) return;
        if (iconContainer == null || iconPrefab == null) return;

        GameObject iconObj = Instantiate(iconPrefab, iconContainer);
        iconObj.name = $"Icon_{target.name}";

        RectTransform rect = iconObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);

        // ВАЖНО: Якоря в центре контейнера!
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        Image image = iconObj.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;

        trackedIcons.Add(target, rect);
    }

    void LateUpdate()
    {
        // УБИРАЕМ ТЕСТ! Теперь используем реальную позицию
        if (playerTransform != null && playerIconRect != null)
        {
            playerIconRect.anchoredPosition = WorldToMinimapPos(playerTransform.position);
            //playerIconRect.rotation = Quaternion.Euler(0, 0, -playerTransform.eulerAngles.y);
        }

        // Обновляем позиции объектов
        UpdateIcons();
        DebugPlayerPosition();
    }

    void UpdateIcons()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (var kvp in trackedIcons)
        {
            GameObject obj = kvp.Key;
            RectTransform icon = kvp.Value;

            if (obj == null)
            {
                toRemove.Add(obj);
                Destroy(icon.gameObject);
                continue;
            }

            icon.anchoredPosition = WorldToMinimapPos(obj.transform.position);
        }

        foreach (var obj in toRemove)
        {
            trackedIcons.Remove(obj);
        }
    }

    Vector2 WorldToMinimapPos(Vector3 worldPos)
    {

        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldPos);

        if (viewportPos.z < 0) return Vector2.zero;
 
        float uiX1 = (viewportPos.x - 0.5f) * minimapSizeInPixels;
        float uiY1 = (viewportPos.y - 0.5f) * minimapSizeInPixels;
        Debug.Log($"UI без инверсии: ({uiX1:f1}, {uiY1:f1})");

        return new Vector2(uiX1, uiY1); 
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    public void RegisterExit(GameObject exitObject)
    {
        if (exitObject == null) return;
        if (trackedIcons.ContainsKey(exitObject)) return;

        AddIcon(exitObject, exitIconSprite, exitIconColor, exitIconSize);
    }

    public void UnregisterExit(GameObject exitObject)
    {
        //if (trackedIcons.TryGetValue(exitObject, out RectTransform icon))
        //{
        //    Destroy(icon.gameObject);
        //    trackedIcons.Remove(exitObject);
        //}

        if (exitObject == null) return;

        if (trackedIcons.TryGetValue(exitObject, out RectTransform icon))
        {
            if (icon != null && icon.gameObject != null)
            {
                Destroy(icon.gameObject);
            }
            trackedIcons.Remove(exitObject);
        }

    }

    void FindExitDoor()
    {
        // Ищем объект с тегом "Exit" или компонентом MinimapExit
        GameObject exit = GameObject.FindGameObjectWithTag("Exit");
        if (exit == null)
        {
            // Если не нашли по тегу, ищем через компонент
            MinimapExit minimapExit = FindObjectOfType<MinimapExit>();
            if (minimapExit != null)
                exit = minimapExit.gameObject;
        }

        if (exit != null)
        {
            exitDoor = exit.transform;

            // Вычисляем угол поворота карты
            Vector3 doorDirection = -exitDoor.forward;
            // Нам нужно, чтобы дверь смотрела вверх (положительная ось Y UI)
            float angle = Vector3.SignedAngle(doorDirection, Vector3.forward, Vector3.up);
            rotationAngle = -angle; // Инвертируем для UI

            // Поворачиваем камеру мини-карты
            minimapCamera.transform.rotation = Quaternion.Euler(90f, rotationAngle, 0f);

            Debug.Log($"Карта повернута на {rotationAngle} градусов. Выход: {exit.name}");
        }
        else
        {
            Debug.LogWarning("Выход не найден, карта без поворота");
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            rotationAngle = 0f;
        }
    }

    void DebugPlayerPosition()
    {
        Vector3 worldPos = playerTransform.position;
        Vector3 min = worldBounds.min.position;
        Vector3 max = worldBounds.max.position;

        float normX = (worldPos.x - min.x) / (max.x - min.x);
        float normZ = (worldPos.z - min.z) / (max.z - min.z);

        Debug.Log($"=== ПОЗИЦИЯ ИГРОКА ===");
        Debug.Log($"Мир: {worldPos}");
        Debug.Log($"Границы: Min {min}, Max {max}");
        Debug.Log($"Нормализовано: X={normX}, Z={normZ}");
        Debug.Log($"UI позиция: {playerIconRect.anchoredPosition}");

        // ВАЖНО: Где находится игрок В РЕАЛЬНОСТИ?
        Debug.Log($"Игрок В РЕАЛЬНОСТИ: X={worldPos.x}, Z={worldPos.z}");
        Debug.Log($"Левый край: X={min.x}, Правый край: X={max.x}");
        Debug.Log($"Игрок {(worldPos.x < (min.x + max.x) / 2 ? "СЛЕВА" : "СПРАВА")} от центра");
    }

    void DebugContainerSetup()
    {
        // Принудительно устанавливаем правильные настройки контейнера
        iconContainer.anchorMin = new Vector2(0.5f, 0.5f);
        iconContainer.anchorMax = new Vector2(0.5f, 0.5f);
        iconContainer.pivot = new Vector2(0.5f, 0.5f);
        iconContainer.anchoredPosition = Vector2.zero;

        Debug.Log($"Container rect: {iconContainer.rect}");
        Debug.Log($"Container pivot: {iconContainer.pivot}");
        Debug.Log($"Container anchored position: {iconContainer.anchoredPosition}");
    }
}