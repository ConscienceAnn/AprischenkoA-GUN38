using UnityEngine;
using System.Collections;

public class AttachMeleeWeapon : MonoBehaviour
{
    public string socketName = "RightHandSocket";
    public Vector3 positionOffset = new Vector3(100f, 0, 0.1f);  // Смещение позиции
    public Vector3 rotationOffset = new Vector3(0, 90, 0);    // Смещение поворота

    private GameObject currentWeapon;
    private Transform handSocket;
    private MeshSockets meshSockets;

    void Start()
    {
        meshSockets = GetComponent<MeshSockets>();
        FindHandSocket();
    }

    public void PickupWeapon(GameObject weaponPrefab)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }
        StartCoroutine(AttachWeapon(weaponPrefab));
    }

    IEnumerator AttachWeapon(GameObject weaponPrefab)
    {
        yield return null;

        if (handSocket == null)
        {
            FindHandSocket();
        }

        if (handSocket != null)
        {
            currentWeapon = Instantiate(weaponPrefab);
            currentWeapon.name = "MeleeWeapon";

            // Прикрепляем к слоту
            currentWeapon.transform.SetParent(handSocket, false);

            // Применяем смещения
            currentWeapon.transform.localPosition = positionOffset;
            currentWeapon.transform.localRotation = Quaternion.Euler(rotationOffset);

            Debug.Log($"{gameObject.name}: Attached melee weapon to {handSocket.name} with offset P:{positionOffset} R:{rotationOffset}");
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Could not find hand socket!");
        }
    }

    void FindHandSocket()
    {
        // Ищем в MeshSockets
        Transform meshSocketsTransform = transform.Find("MeshSockets");
        if (meshSocketsTransform != null)
        {
            handSocket = meshSocketsTransform.Find(socketName);
            if (handSocket != null)
            {
                Debug.Log($"Found socket in MeshSockets: {socketName}");
                return;
            }
        }

        // Ищем в Armature
        Transform armature = transform.Find("Armature");
        if (armature != null)
        {
            handSocket = FindDeepChild(armature, "socketRightHand");
            if (handSocket != null)
            {
                Debug.Log($"Found socket in Armature: socketRightHand");
                return;
            }
        }
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}