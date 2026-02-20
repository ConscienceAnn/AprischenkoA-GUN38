using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

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
    public Sprite meleeWeaponIcon;

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

    [Header("Выход")]
    public Sprite exitIconSprite;
    public Color exitIconColor = Color.yellow;
    public float exitIconSize = 20f;

    [Header("Ориентация карты")]
    public Transform exitDoor;
    private float rotationAngle = 0f;

    [Header("Настройки размера")]
    public float minimapBaseSize = 500f;
    public bool adaptToScreenSize = true;
    public float screenSizeFactor = 0.2f;

    private Camera minimapCamera;
    private RenderTexture renderTexture;
    private WorldBounds worldBounds;
    private Transform playerTransform;
    private RectTransform playerIconRect;
    private Dictionary<GameObject, RectTransform> trackedIcons = new Dictionary<GameObject, RectTransform>();
    private Bounds mapBounds;

    private RectTransform minimapImageRect;
    private RectTransform minimapMaskRect;
    private RectTransform minimapPanelRect;
    private float currentMinimapSize;

    public enum PickupType
    {
        Health,
        Ammo,
        Weapon
    }

    void Start()
    {
        FindUIComponents();

        CreateMinimapCamera();
        FindWorldBounds();
        SetupCameraPosition();

        // Сначала адаптируем размер
        AdaptSizeToScreen();

        // Потом создаем RenderTexture с правильным размером
        SetupRenderTexture();

        FindAllObjects();
        FindExitDoor();

        // Выводим дебаг информацию после всех настроек
        DebugMinimapInfo();
    }

    void FindUIComponents()
    {
        if (minimapImage != null)
        {
            minimapImageRect = minimapImage.rectTransform;

            if (minimapImageRect.parent != null)
            {
                minimapMaskRect = minimapImageRect.parent.GetComponent<RectTransform>();

                if (minimapMaskRect != null && minimapMaskRect.parent != null)
                {
                    minimapPanelRect = minimapMaskRect.parent.GetComponent<RectTransform>();
                }
            }
        }

        Debug.Log("=== НАЧАЛЬНЫЕ РАЗМЕРЫ UI ===");
        Debug.Log($"Panel: {(minimapPanelRect != null ? minimapPanelRect.sizeDelta.ToString() : "null")}");
        Debug.Log($"Mask: {(minimapMaskRect != null ? minimapMaskRect.sizeDelta.ToString() : "null")}");
        Debug.Log($"Image: {(minimapImageRect != null ? minimapImageRect.sizeDelta.ToString() : "null")}");
        Debug.Log($"IconContainer: {(iconContainer != null ? iconContainer.sizeDelta.ToString() : "null")}");
        Debug.Log($"Screen: {Screen.width}x{Screen.height}");
    }

    void AdaptSizeToScreen()
    {
        if (!adaptToScreenSize) return;

        float screenMinSize = Mathf.Min(Screen.width, Screen.height);
        float newSize = screenMinSize * screenSizeFactor;

        Debug.Log($"\n=== АДАПТАЦИЯ РАЗМЕРА ===");
        Debug.Log($"Новый размер: {newSize}px (было: {currentMinimapSize}px)");

        // Для Panel оставляем как есть (она позиционирует карту на экране)
        if (minimapPanelRect != null)
        {
            minimapPanelRect.sizeDelta = new Vector2(newSize, newSize);
            Debug.Log($"Panel новый размер: {minimapPanelRect.sizeDelta}, позиция: {minimapPanelRect.anchoredPosition}");
        }

        // Для Mask устанавливаем якоря в центр
        if (minimapMaskRect != null)
        {
            minimapMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
            minimapMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
            minimapMaskRect.pivot = new Vector2(0.5f, 0.5f);
            minimapMaskRect.sizeDelta = new Vector2(newSize, newSize);
            minimapMaskRect.anchoredPosition = Vector2.zero;
            Debug.Log($"Mask новый размер: {minimapMaskRect.sizeDelta}, позиция: {minimapMaskRect.anchoredPosition}");
        }

        // Для Image тоже ставим якоря в центр (ВАЖНО!)
        if (minimapImageRect != null)
        {
            minimapImageRect.anchorMin = new Vector2(0.5f, 0.5f);
            minimapImageRect.anchorMax = new Vector2(0.5f, 0.5f);
            minimapImageRect.pivot = new Vector2(0.5f, 0.5f);
            minimapImageRect.sizeDelta = new Vector2(newSize, newSize);
            minimapImageRect.anchoredPosition = Vector2.zero;
            Debug.Log($"Image новый размер: {minimapImageRect.sizeDelta}, позиция: {minimapImageRect.anchoredPosition}");
        }

        // IconContainer уже с центральными якорями
        if (iconContainer != null)
        {
            iconContainer.sizeDelta = new Vector2(newSize, newSize);
            iconContainer.anchoredPosition = Vector2.zero;
            Debug.Log($"IconContainer новый размер: {iconContainer.sizeDelta}, позиция: {iconContainer.anchoredPosition}");
        }

        currentMinimapSize = newSize;
    }

    void SetupRenderTexture()
    {
        int textureSize = Mathf.RoundToInt(currentMinimapSize > 0 ? currentMinimapSize : minimapBaseSize);
        Debug.Log($"\n=== НАСТРОЙКА RENDER TEXTURE ===");
        Debug.Log($"Размер текстуры: {textureSize}x{textureSize}");

        renderTexture = new RenderTexture(textureSize, textureSize, 16);
        renderTexture.name = "MinimapRenderTexture";
        minimapCamera.targetTexture = renderTexture;

        if (minimapImage != null)
            minimapImage.texture = renderTexture;
    }

    /// <summary>
    /// Детальный дебаг всей информации о миникарте
    /// </summary>
    void DebugMinimapInfo()
    {
        Debug.Log("\n=== ДЕТАЛЬНАЯ ИНФОРМАЦИЯ О МИНИКАРТЕ ===");

        // Информация о камере
        if (minimapCamera != null)
        {
            Debug.Log($"Камера: позиция={minimapCamera.transform.position}, orthographicSize={minimapCamera.orthographicSize}");
        }

        // Информация о границах мира
        if (worldBounds != null && worldBounds.min != null && worldBounds.max != null)
        {
            Debug.Log($"Границы мира: min={worldBounds.min.position}, max={worldBounds.max.position}");
            float worldWidth = Mathf.Abs(worldBounds.max.position.x - worldBounds.min.position.x);
            float worldHeight = Mathf.Abs(worldBounds.max.position.z - worldBounds.min.position.z);
            Debug.Log($"Размер мира: {worldWidth} x {worldHeight}");
        }

        // Информация о UI элементах
        Debug.Log("\n--- UI ЭЛЕМЕНТЫ ---");

        if (minimapPanelRect != null)
        {
            Debug.Log($"MinimapPanel: размер={minimapPanelRect.sizeDelta}, позиция={minimapPanelRect.anchoredPosition}, " +
                     $"якоря=({minimapPanelRect.anchorMin}, {minimapPanelRect.anchorMax}), pivot={minimapPanelRect.pivot}");
        }

        if (minimapMaskRect != null)
        {
            Debug.Log($"MinimapMask: размер={minimapMaskRect.sizeDelta}, позиция={minimapMaskRect.anchoredPosition}, " +
                     $"якоря=({minimapMaskRect.anchorMin}, {minimapMaskRect.anchorMax}), pivot={minimapMaskRect.pivot}");

            // Проверяем наличие компонента Mask
            Mask mask = minimapMaskRect.GetComponent<Mask>();
            Debug.Log($"Mask компонент: {(mask != null ? "есть" : "ОТСУТСТВУЕТ!")}");
            if (mask != null)
            {
                Debug.Log($"  - Show Mask Graphic: {mask.showMaskGraphic}");
            }
        }

        if (minimapImageRect != null)
        {
            Debug.Log($"MinimapImage: размер={minimapImageRect.sizeDelta}, позиция={minimapImageRect.anchoredPosition}, " +
                     $"якоря=({minimapImageRect.anchorMin}, {minimapImageRect.anchorMax}), pivot={minimapImageRect.pivot}");

            // Проверяем настройки RawImage
            Debug.Log($"  - texture: {(minimapImage.texture != null ? minimapImage.texture.name : "null")}");
            Debug.Log($"  - uvRect: {minimapImage.uvRect}");
        }

        if (iconContainer != null)
        {
            Debug.Log($"IconContainer: размер={iconContainer.sizeDelta}, позиция={iconContainer.anchoredPosition}, " +
                     $"якоря=({iconContainer.anchorMin}, {iconContainer.anchorMax}), pivot={iconContainer.pivot}");
        }

        // Информация о RenderTexture
        if (renderTexture != null)
        {
            Debug.Log($"\nRenderTexture: {renderTexture.width}x{renderTexture.height}, format={renderTexture.format}");
        }

        // Проверка соотношения размеров
        Debug.Log("\n--- ПРОВЕРКА СООТВЕТСТВИЯ РАЗМЕРОВ ---");

        if (minimapImageRect != null && iconContainer != null)
        {
            bool sizesMatch = Mathf.Abs(minimapImageRect.sizeDelta.x - iconContainer.sizeDelta.x) < 0.1f;
            Debug.Log($"Размеры Image и IconContainer совпадают: {sizesMatch}");
            if (!sizesMatch)
            {
                Debug.LogWarning($"  НЕСООТВЕТСТВИЕ! Image: {minimapImageRect.sizeDelta.x}, IconContainer: {iconContainer.sizeDelta.x}");
            }
        }

        if (minimapImageRect != null && minimapMaskRect != null)
        {
            bool maskMatch = Mathf.Abs(minimapImageRect.sizeDelta.x - minimapMaskRect.sizeDelta.x) < 0.1f;
            Debug.Log($"Размеры Image и Mask совпадают: {maskMatch}");
        }

        Debug.Log("=== КОНЕЦ ИНФОРМАЦИИ ===\n");
    }

    /// <summary>
    /// Метод для дебага позиции конкретного объекта
    /// </summary>
    void DebugWorldToMinimapConversion(Vector3 worldPos, string objectName)
    {
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldPos);
        Vector2 minimapPos = WorldToMinimapPos(worldPos);

        Debug.Log($"=== КОНВЕРТАЦИЯ {objectName} ===");
        Debug.Log($"Мировая позиция: {worldPos}");
        Debug.Log($"Viewport позиция: {viewportPos} (z={viewportPos.z})");
        Debug.Log($"Позиция на миникарте: {minimapPos}");

        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            Debug.LogWarning($"  {objectName} за пределами видимости камеры!");
        }
    }

    void CreateMinimapCamera()
    {
        GameObject camObj = new GameObject("MinimapCamera");
        camObj.transform.SetParent(transform);
        minimapCamera = camObj.AddComponent<Camera>();
        minimapCamera.tag = "Untagged";

        AudioListener listener = minimapCamera.GetComponent<AudioListener>();
        if (listener != null) Destroy(listener);

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
            Debug.LogError("WorldBounds not found on scene!");
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
        minimapCamera.orthographicSize = size;
        mapBounds = new Bounds(center, new Vector3(width, 0, height));

        Debug.Log($"\n=== НАСТРОЙКА КАМЕРЫ ===");
        Debug.Log($"Центр мира: {center}");
        Debug.Log($"Размер камеры: {size}");
    }

    void FindPlayer()
    {
        playerTransform = null;

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<CharacterController>() != null &&
                player.GetComponent<AiAgent>() == null)
            {
                playerTransform = player.transform;
                Debug.Log($"Player found: {player.name} at {playerTransform.position}");

                // Дебаг для позиции игрока
                DebugWorldToMinimapConversion(playerTransform.position, "PLAYER");
                return;
            }
        }

        if (playerTransform == null)
            Debug.LogWarning("Player with CharacterController not found!");
    }

    void CreatePlayerIcon()
    {
        if (iconContainer == null || iconPrefab == null) return;

        GameObject iconObj = Instantiate(iconPrefab, iconContainer);
        iconObj.name = "PlayerIcon";

        playerIconRect = iconObj.GetComponent<RectTransform>();
        playerIconRect.sizeDelta = new Vector2(playerSize, playerSize);
        playerIconRect.pivot = new Vector2(0.5f, 0.5f);
        playerIconRect.anchorMin = new Vector2(0.5f, 0.5f);
        playerIconRect.anchorMax = new Vector2(0.5f, 0.5f);
        playerIconRect.anchoredPosition = Vector2.zero;

        Image image = iconObj.GetComponent<Image>();
        image.sprite = playerIcon;
        image.color = playerColor;

        Debug.Log($"Player icon created at {playerIconRect.anchoredPosition}");
    }

    void FindAllObjects()
    {
        AiAgent[] enemies = FindObjectsOfType<AiAgent>();
        foreach (var enemy in enemies)
            AddIcon(enemy.gameObject, enemyIcon, enemyColor, enemySize);

        HealthPickup[] healths = FindObjectsOfType<HealthPickup>();
        foreach (var health in healths)
            AddIcon(health.gameObject, healthIcon, healthColor, pickupSize);

        AmmoPickup[] ammos = FindObjectsOfType<AmmoPickup>();
        foreach (var ammo in ammos)
            AddIcon(ammo.gameObject, ammoIcon, ammoColor, pickupSize);

        WeaponPickup[] weapons = FindObjectsOfType<WeaponPickup>();
        foreach (var weapon in weapons)
            AddIcon(weapon.gameObject, weaponIcon, weaponColor, pickupSize);

        MeleeWeaponPickup[] meleeWeapons = FindObjectsOfType<MeleeWeaponPickup>();
        foreach (var meleeWeapon in meleeWeapons)
        {
            AddIcon(meleeWeapon.gameObject, meleeWeaponIcon, weaponColor, pickupSize);
        }
    }

    void AddIcon(GameObject target, Sprite sprite, Color color, float size)
    {
        if (target == null || trackedIcons.ContainsKey(target) || iconContainer == null || iconPrefab == null)
            return;

        GameObject iconObj = Instantiate(iconPrefab, iconContainer);
        iconObj.name = $"Icon_{target.name}";

        RectTransform rect = iconObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);
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
        if (playerTransform == null)
        {
            FindPlayer();
        }

        if (playerTransform != null && playerIconRect == null)
        {
            Debug.Log("Игрок найден, создаем иконку");
            CreatePlayerIcon();
        }

        if (playerTransform != null && playerIconRect != null)
        {
            Vector2 newPos = WorldToMinimapPos(playerTransform.position);

            // Дебаг каждого 60-го кадра
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"Player world: {playerTransform.position} -> minimap: {newPos}");
            }

            playerIconRect.anchoredPosition = newPos;
        }

        UpdateIcons();
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
            trackedIcons.Remove(obj);
    }

    Vector2 WorldToMinimapPos(Vector3 worldPos)
    {
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldPos);
        if (viewportPos.z < 0) return Vector2.zero;

        float actualSize = currentMinimapSize > 0 ? currentMinimapSize : minimapBaseSize;

        float uiX = (viewportPos.x - 0.5f) * actualSize;
        float uiY = (viewportPos.y - 0.5f) * actualSize;

        return new Vector2(uiX, uiY);
    }

    public void UpdateSize()
    {
        AdaptSizeToScreen();
        SetupRenderTexture();
        DebugMinimapInfo();
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
        if (exitObject == null || trackedIcons.ContainsKey(exitObject)) return;
        AddIcon(exitObject, exitIconSprite, exitIconColor, exitIconSize);
    }

    public void UnregisterExit(GameObject exitObject)
    {
        if (exitObject == null) return;

        if (trackedIcons.TryGetValue(exitObject, out RectTransform icon))
        {
            if (icon != null && icon.gameObject != null)
                Destroy(icon.gameObject);
            trackedIcons.Remove(exitObject);
        }
    }

    void FindExitDoor()
    {
        GameObject exit = GameObject.FindGameObjectWithTag("Exit");
        if (exit == null)
        {
            MinimapExit minimapExit = FindObjectOfType<MinimapExit>();
            if (minimapExit != null)
                exit = minimapExit.gameObject;
        }

        if (exit != null)
        {
            exitDoor = exit.transform;
            Vector3 doorDirection = -exitDoor.forward;
            float angle = Vector3.SignedAngle(doorDirection, Vector3.forward, Vector3.up);
            rotationAngle = -angle;
            minimapCamera.transform.rotation = Quaternion.Euler(90f, rotationAngle, 0f);
            Debug.Log($"Exit door found, camera rotated by {rotationAngle} degrees");
        }
        else
        {
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Debug.Log("No exit door found, default camera rotation");
        }
    }

    public void RegisterPickup(GameObject pickupObject, PickupType type)
    {
        if (pickupObject == null || trackedIcons.ContainsKey(pickupObject)) return;

        Sprite iconSprite = null;
        Color iconColor = Color.white;
        float iconSize = pickupSize;

        switch (type)
        {
            case PickupType.Health:
                iconSprite = healthIcon;
                iconColor = healthColor;
                break;
            case PickupType.Ammo:
                iconSprite = ammoIcon;
                iconColor = ammoColor;
                break;
            case PickupType.Weapon:
                iconSprite = weaponIcon;
                iconColor = weaponColor;
                break;
        }

        if (iconSprite == null) return;
        AddIcon(pickupObject, iconSprite, iconColor, iconSize);
    }

    public void UnregisterPickup(GameObject pickupObject)
    {
        if (pickupObject == null) return;

        if (trackedIcons.TryGetValue(pickupObject, out RectTransform icon))
        {
            if (icon != null && icon.gameObject != null)
                Destroy(icon.gameObject);
            trackedIcons.Remove(pickupObject);
        }
    }
}