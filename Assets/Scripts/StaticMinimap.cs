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

    public float minimapSizeInPixels = 500f;

    private Camera minimapCamera;
    private RenderTexture renderTexture;
    private WorldBounds worldBounds;
    private Transform playerTransform;
    private RectTransform playerIconRect;
    private Dictionary<GameObject, RectTransform> trackedIcons = new Dictionary<GameObject, RectTransform>();
    private Bounds mapBounds;

    public enum PickupType
    {
        Health,
        Ammo,
        Weapon
    }

    void Start()
    {
        CreateMinimapCamera();
        FindWorldBounds();
        SetupCameraPosition();
        SetupRenderTexture();
        // FindPlayer();
        // CreatePlayerIcon();
        FindAllObjects();
        FindExitDoor();
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
    }

    void SetupRenderTexture()
    {
        renderTexture = new RenderTexture(512, 512, 16);
        renderTexture.name = "MinimapRenderTexture";
        minimapCamera.targetTexture = renderTexture;

        if (minimapImage != null)
            minimapImage.texture = renderTexture;
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
                Debug.Log($"Player found: {player.name}");
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

        Image image = iconObj.GetComponent<Image>();
        image.sprite = playerIcon;
        image.color = playerColor;
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
        if (playerTransform == null) FindPlayer();

        // Если нашли игрока, но иконки еще нет - создаем

        if (playerTransform != null && playerIconRect == null)
        {
            Debug.Log("Игрок найден, создаем иконку");
            CreatePlayerIcon();
        }

        if (playerTransform != null && playerIconRect != null)
            playerIconRect.anchoredPosition = WorldToMinimapPos(playerTransform.position);

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

        float uiX = (viewportPos.x - 0.5f) * minimapSizeInPixels;
        float uiY = (viewportPos.y - 0.5f) * minimapSizeInPixels;

        return new Vector2(uiX, uiY);
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
        }
        else
        {
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
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