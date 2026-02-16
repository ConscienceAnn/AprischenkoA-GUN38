using UnityEngine;

public class DestroyableSetup : MonoBehaviour
{
    [ContextMenu("Add Destroyable Component To All Objects On CanDestroy Layer")]
    void AddDestroyableToLayer()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("CanDestroy"))
            {
                if (obj.GetComponent<DestroyableObject>() == null)
                {
                    obj.AddComponent<DestroyableObject>();
                    count++;
                    Debug.Log($"Added to: {obj.name}");
                }
            }
        }

        Debug.Log($"Complete! Added to {count} objects");
    }
}