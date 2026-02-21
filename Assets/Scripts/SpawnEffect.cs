using UnityEngine;

public class SpawnEffect : MonoBehaviour
{
    public GameObject effectPrefab;
    public bool destroyAfter = true;
    public float destroyDelay = 2f;

    public void Spawn()
    {
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, transform.rotation);

            if (destroyAfter)
            {
                Destroy(effect, destroyDelay);
            }
        }
    }

    public void SpawnAtPosition(Vector3 position)
    {
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);

            if (destroyAfter)
            {
                Destroy(effect, destroyDelay);
            }
        }
    }

    public void SpawnWithRotation(Vector3 position, Quaternion rotation)
    {
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, position, rotation);

            if (destroyAfter)
            {
                Destroy(effect, destroyDelay);
            }
        }
    }
}