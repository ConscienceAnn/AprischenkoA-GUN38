using UnityEngine;

public class CellManagerTest : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;

    private void Start()
    {
        Debug.Log("=== CELL MANAGER TEST START ===");

        // Самый простой тест - просто создать один юнит
        if (unitPrefab != null)
        {
            Vector3 spawnPos = new Vector3(0, 1, 0);
            Instantiate(unitPrefab, spawnPos, Quaternion.identity);
            Debug.Log("Unit instantiated successfully");
        }
        else
        {
            Debug.LogError("Unit Prefab is NULL");
        }

        Debug.Log("=== CELL MANAGER TEST COMPLETE ===");
    }
}