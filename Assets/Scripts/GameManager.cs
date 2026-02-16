using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    public GameObject playerPrefab;

    [Header("Camera Settings")]
    public GameObject mainCameraPrefab; // Префаб Main Camera
    public GameObject cameraRigPrefab;       // Префаб ----- Cameras ------

    [Header("UI Settings")]
    public GameObject uiCanvasPrefab;
    public GameObject eventSystemPrefab;

    public AmmoWidget AmmoWidget { get; private set; }
    public UIHealthBar PlayerHealthBar { get; private set; }

    private PlayerData savedPlayerData;
    private GameObject currentPlayer;
    private GameObject currentMainCamera;
    private GameObject currentCameraRig;
    private GameObject currentUICanvas;
    private GameObject currentEventSystem;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            CreateEventSystem();
            CreateCameras();
            CreateUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CreateEventSystem()
    {
        // Проверяем, не существует ли уже EventSystem в сцене
        if (currentEventSystem == null)
        {
            EventSystem existingEventSystem = FindObjectOfType<EventSystem>();
            if (existingEventSystem != null)
            {
                // Если нашли существующий, используем его
                currentEventSystem = existingEventSystem.gameObject;
                DontDestroyOnLoad(currentEventSystem);
                Debug.Log("Existing EventSystem found and persisted");
            }
            else if (eventSystemPrefab != null)
            {
                // Создаем новый из префаба
                currentEventSystem = Instantiate(eventSystemPrefab, Vector3.zero, Quaternion.identity);
                currentEventSystem.name = "EventSystem";
                DontDestroyOnLoad(currentEventSystem);
                Debug.Log("EventSystem created from prefab");
            }
            else
            {
                // Создаем базовый EventSystem если нет префаба
                currentEventSystem = new GameObject("EventSystem");
                currentEventSystem.AddComponent<EventSystem>();
                currentEventSystem.AddComponent<StandaloneInputModule>();
                DontDestroyOnLoad(currentEventSystem);
                Debug.LogWarning("EventSystem prefab not assigned, created default EventSystem");
            }
        }
    }

    void CreateUI()
    {
        if (currentUICanvas == null && uiCanvasPrefab != null)
        {
            currentUICanvas = Instantiate(uiCanvasPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(currentUICanvas);

            // Кэшируем ссылки на компоненты UI
            AmmoWidget = currentUICanvas.GetComponentInChildren<AmmoWidget>();

            Debug.Log("UI Canvas created and persisted");
        }
        else if (uiCanvasPrefab == null)
        {
            Debug.LogError("UI Canvas Prefab not assigned in GameManager!");
        }
    }

    public void UpdateAmmoDisplay(int ammoCount, int clipCount)
    {
        if (AmmoWidget != null)
        {
            AmmoWidget.Refresh(ammoCount, clipCount);
        }
    }

    // Создаем HealthBar для игрока (не в DontDestroyOnLoad, а как child игрока)
    void SetupPlayerHealthBar(GameObject player)
    {
        // HealthBar должен быть child-ом игрока, чтобы следовать за ним
        UIHealthBar healthBar = player.GetComponentInChildren<UIHealthBar>();
        if (healthBar == null)
        {
            // Если в префабе игрока нет HealthBar, создаем его
            GameObject healthBarPrefab = Resources.Load<GameObject>("UI/HealthBar");
            if (healthBarPrefab != null)
            {
                GameObject healthBarObj = Instantiate(healthBarPrefab, player.transform);
                healthBar = healthBarObj.GetComponent<UIHealthBar>();
                healthBar.target = player.transform; // Настраиваем target
            }
        }
    }
    void CreateCameras()
    {
        // Создаем Main Camera
        if (currentMainCamera == null && mainCameraPrefab != null)
        {
            currentMainCamera = Instantiate(mainCameraPrefab, Vector3.zero, Quaternion.identity);
            currentMainCamera.tag = "MainCamera";
            DontDestroyOnLoad(currentMainCamera);
            Debug.Log("Main Camera created");
        }

        // Создаем Camera Rig
        if (currentCameraRig == null && cameraRigPrefab != null)
        {
            currentCameraRig = Instantiate(cameraRigPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(currentCameraRig);
            Debug.Log("Camera Rig created");
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // УНИЧТОЖАЕМ старого игрока если он есть
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }

        //СНАЧАЛА пересоздаем камеры
      //  RecreateCameras();

        // СОЗДАЕМ нового игрока
        SpawnPlayerAtSpawnPoint();
    }
    void RecreateCameras()
    {
        // Уничтожаем старый CameraRig
        if (currentCameraRig != null)
        {
            Destroy(currentCameraRig);
            currentCameraRig = null;
        }

        // Создаем новый CameraRig (пока без позиции игрока)
        if (cameraRigPrefab != null)
        {
            currentCameraRig = Instantiate(cameraRigPrefab, Vector3.zero, Quaternion.identity);
            currentCameraRig.name = cameraRigPrefab.name;
            DontDestroyOnLoad(currentCameraRig);
            Debug.Log("Camera Rig recreated");
        }

        // Main Camera не пересоздаем, она уже есть
    }


    void SpawnPlayerAtSpawnPoint()
    {
        // Ищем точку спавна в текущей сцене
        PlayerSpawnPoint spawnPoint = FindObjectOfType<PlayerSpawnPoint>();



        if (spawnPoint == null)
        {
            Debug.LogError("No PlayerSpawnPoint found in scene!");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab not assigned in GameManager!");
            return;
        }

        // Создаем нового игрока
        currentPlayer = Instantiate(playerPrefab,
                                   spawnPoint.transform.position,
                                   spawnPoint.transform.rotation);
        currentPlayer.tag = "Player";
        DontDestroyOnLoad(currentPlayer);
        SetupPlayerHealthBar(currentPlayer);

        Debug.Log($"New player created at {spawnPoint.transform.position}");


        // 3. Настраиваем камеру на слежение за игроком
        SetupCameras();



        // Восстанавливаем сохраненные данные
        if (savedPlayerData != null)
        {
            savedPlayerData.ApplyToPlayer(currentPlayer);
        }
    }

    void SetupCameras()
    {

        if (currentPlayer == null) return;
        if (currentCameraRig == null)
        {
            Debug.LogError("CameraRig is null in SetupCameras!");
            return;
        }

        // Перемещаем весь CameraRig к игроку
        currentCameraRig.transform.position = currentPlayer.transform.position;

        // --- НАСТРАИВАЕМ CameraLookAt ---
        Transform cameraLookAt = null;

        // Ищем CameraLookAt в CameraRig
        cameraLookAt = currentCameraRig.transform.Find("CameraLookAt");
        if (cameraLookAt == null)
        {
            // Ищем глубже
            foreach (Transform child in currentCameraRig.GetComponentsInChildren<Transform>())
            {
                if (child.name == "CameraLookAt")
                {
                    cameraLookAt = child;
                    break;
                }
            }
        }

        if (cameraLookAt == null)
        {
            Debug.LogError("CameraLookAt not found in CameraRig!");
            return;
        }

        // НЕ ДЕЛАЕМ SetParent! Просто позиционируем относительно игрока
        // Сохраняем оригинальный оффсет из префаба
        Vector3 originalOffset = cameraLookAt.localPosition;

        // Устанавливаем позицию CameraLookAt относительно игрока
        cameraLookAt.position = currentPlayer.transform.position + originalOffset;

        // Оставляем CameraLookAt в иерархии CameraRig
        // cameraLookAt.SetParent(currentPlayer.transform); // НЕ НУЖНО!

        Debug.Log($"CameraLookAt positioned at {cameraLookAt.position} with offset {originalOffset}");

        // --- НАСТРАИВАЕМ ВИРТУАЛЬНЫЕ КАМЕРЫ ---
        Cinemachine.CinemachineVirtualCamera[] vcams = currentCameraRig.GetComponentsInChildren<Cinemachine.CinemachineVirtualCamera>();

        foreach (var vcam in vcams)
        {
            vcam.Follow = currentPlayer.transform;
            vcam.LookAt = cameraLookAt;
            vcam.PreviousStateIsValid = false;
            Debug.Log($"Virtual camera {vcam.name} configured");
        }

        // --- НАСТРАИВАЕМ CHARACTER AIMING ---
        CharacterAiming aiming = currentPlayer.GetComponent<CharacterAiming>();
        if (aiming != null)
        {
            aiming.cameraLookAt = cameraLookAt;
        }
    }

    public void TransitionToScene(string sceneName)
    {
        // Сохраняем состояние игрока перед переходом
        if (currentPlayer != null)
        {
            savedPlayerData = new PlayerData(currentPlayer);
            Debug.Log("Saved player data before transition");
        }

        LoadingScreen.Instance.LoadScene(sceneName, () => {
            // Этот код выполнится после загрузки
            Debug.Log("Scene loaded!");
        });


        // Загружаем новую сцену
       // SceneManager.LoadScene(sceneName);
    }

    public void ResetPlayerData()
    {
        savedPlayerData = null;
    }

    public void StartNewGame(string firstSceneName = "CityLevel")
    {
        ResetPlayerData();
        SceneManager.LoadScene(firstSceneName);
    }


}