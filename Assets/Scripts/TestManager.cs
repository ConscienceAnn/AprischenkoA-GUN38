using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;

    private void Start()
    {
        Debug.Log("=== TEST START ===");

        if (unitPrefab == null)
        {
            Debug.LogError("Unit Prefab is NULL!");
            return;
        }

        Debug.Log("Unit Prefab is assigned: " + unitPrefab.name);
        Debug.Log("=== TEST COMPLETE ===");
    }
}
