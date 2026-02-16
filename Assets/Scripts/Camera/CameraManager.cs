using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Cinemachine.CinemachineVirtualCameraBase killCam;


    private void Start()
    {
        // ≈сли killCam не назначен, пробуем найти его автоматически
        if (killCam == null)
        {
            killCam = FindObjectOfType<CinemachineVirtualCameraBase>();
            Debug.Log($"CameraManager auto-found killCam: {(killCam != null ? killCam.name : "NULL")}");
        }
    }


    public void EnableKillCam() {
        // ѕробуем найти камеру, если еще не нашли
        if (killCam == null)
        {
            killCam = FindObjectOfType<CinemachineVirtualCameraBase>();
            Debug.Log($"EnableKillCam: trying to find killCam - {(killCam != null ? killCam.name : "NULL")}");
        }

        if (killCam != null)
        {
            killCam.Priority = 20;
            Debug.Log("Kill cam enabled");
        }
        else
        {
            Debug.LogError("killCam is NULL! Cannot enable kill cam");
        }

    }
}
