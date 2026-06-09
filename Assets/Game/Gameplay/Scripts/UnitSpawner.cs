using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int spawnCount = 20;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10, 10);
    [SerializeField] private float spawnHeight = 0f;

    private List<GameObject> spawnedUnits = new List<GameObject>();

    private void Start()
    {
        SpawnAllUnits();
    }

    private void SpawnAllUnits()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject unit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
            spawnedUnits.Add(unit);
        }

        Debug.Log($"Spawned {spawnedUnits.Count} units");
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float z = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);
        return new Vector3(x, spawnHeight, z);
    }

    public List<GameObject> GetAllUnits()
    {
        return spawnedUnits;
    }

    public void ClearAllUnits()
    {
        foreach (var unit in spawnedUnits)
        {
            if (unit != null)
                Destroy(unit);
        }
        spawnedUnits.Clear();
    }
}
