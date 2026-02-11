using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    public GameObject playerPrefab;

    [Header("Camera Settings")]
    public GameObject mainCameraPrefab; // Префаб Main Camera
    public GameObject cameraRigPrefab;       // Префаб ----- Cameras ------

    // Данные игрока для сохранения между сценами
    private PlayerData savedPlayerData;
    private GameObject currentPlayer;
    private GameObject currentMainCamera;
    private GameObject currentCameraRig;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;


            CreateCameras();
        }
        else
        {
            Destroy(gameObject);
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
        RecreateCameras();

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

        // Загружаем новую сцену
        SceneManager.LoadScene(sceneName);
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