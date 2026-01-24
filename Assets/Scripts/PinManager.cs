using UnityEngine;
using System.Collections.Generic;
using UnityEditor.PackageManager;

public class PinManager : MonoBehaviour
{
    [SerializeField] private GameObject _allPinsPrefab;
    [SerializeField] private Transform _pinsSpawnPoint;
    [SerializeField] private float _pinStandingThreshold = 0.7f;

    private GameObject _currentPins;
    private List<PinInfo> _pinInfoList = new List<PinInfo>();

    private class PinInfo
    {
        public Transform Transform;
        public Rigidbody Rigidbody; 
        public bool IsStanding;
    }


    public void SpawnPins()
    {
        if (_currentPins != null)
        {
            Destroy(_currentPins);
        }

        if (_allPinsPrefab != null && _pinsSpawnPoint != null)
        {
            _currentPins = Instantiate(_allPinsPrefab, _pinsSpawnPoint.position, _pinsSpawnPoint.rotation);
            CachePinInfo();
        }
    }


    // Кешируем информацию о кеглях один раз
    private void CachePinInfo()
    {
        _pinInfoList.Clear();

        if (_currentPins == null) return;

        foreach (Transform pinTransform in _currentPins.transform)
        {
            if (pinTransform != null)
            {
                Rigidbody rb = pinTransform.GetComponent<Rigidbody>();
                if (rb != null) // Проверяем что есть Rigidbody
                {
                    _pinInfoList.Add(new PinInfo
                    {
                        Transform = pinTransform,
                        Rigidbody = rb, // Сохраняем Rigidbody!
                        IsStanding = false
                    });
                }
            }
        }
    }



    public int CountStandingPins()
    {
        int standingPins = 0;

        if (_currentPins == null)
        {
            Debug.LogWarning("PinManager.CountStandingPins: _currentPins is null");
            return 0;
        }

        Debug.Log($"PinManager.CountStandingPins: проверяем {_currentPins.transform.childCount} кеглей");

        // Используем кешированные данные вместо получения Transform каждый раз
        foreach (PinInfo pinInfo in _pinInfoList)
        {
            if (pinInfo.Transform == null) continue;

            float upDot = Vector3.Dot(pinInfo.Transform.up, Vector3.up);
            pinInfo.IsStanding = upDot > _pinStandingThreshold; // Сохраняем состояние

            // ИСПРАВЛЕННЫЕ СТРОКИ:
            Debug.Log($"Кегля {pinInfo.Transform.name}: upDot={upDot:F2}, стоит={pinInfo.IsStanding}, порог={_pinStandingThreshold}");

            if (pinInfo.IsStanding)
            {
                standingPins++;
            }
        }

        Debug.Log($"PinManager.CountStandingPins: итого стоит {standingPins} кеглей");
        return standingPins;
    }

    public bool IsAnyStandingPinMoving(float velocityThreshold = 0.05f)
    {
        if (_currentPins == null)
        {
            Debug.LogWarning("PinManager.IsAnyStandingPinMoving: _currentPins is null");
            return false;
        }

        // Используем кешированные Rigidbody
        foreach (PinInfo pinInfo in _pinInfoList)
        {
            if (pinInfo.Transform == null || pinInfo.Rigidbody == null) continue;

            // Нужно обновить IsStanding здесь тоже
            float upDot = Vector3.Dot(pinInfo.Transform.up, Vector3.up);
            bool isStanding = upDot > _pinStandingThreshold;

            if (isStanding && pinInfo.Rigidbody.velocity.magnitude > velocityThreshold)
            {
                Debug.Log($"Кегля {pinInfo.Transform.name} движется: скорость={pinInfo.Rigidbody.velocity.magnitude:F2}");
                return true;
            }
        }

        Debug.Log("Все стоящие кегли не двигаются");
        return false;
    }

}