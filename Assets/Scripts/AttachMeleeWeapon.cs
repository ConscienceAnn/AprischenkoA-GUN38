using UnityEngine;
using System.Collections;

public class AttachMeleeWeapon : MonoBehaviour
{
    public string socketName = "RightHandSocket"; // Имя слота в иерархии
    public Vector3 positionOffset = Vector3.zero;  // Смещение позиции
    public Vector3 rotationOffset = Vector3.zero;  // Смещение поворота

    private GameObject currentWeapon;
    private Transform handSocket;
    private MeshSockets meshSockets;

    void Start()
    {
        meshSockets = GetComponent<MeshSockets>();
        FindHandSocket();
    }

    // Метод для подбора оружия - вызывается из пикапа
    public void PickupWeapon(GameObject weaponPrefab)
    {
        // Если уже есть оружие, удаляем его
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }

        // Запускаем корутину для прикрепления
        StartCoroutine(AttachWeapon(weaponPrefab));
    }

    IEnumerator AttachWeapon(GameObject weaponPrefab)
    {
        // Ждем кадр чтобы все инициализировалось
        yield return null;

        // Если слот еще не найден, пробуем найти снова
        if (handSocket == null)
        {
            FindHandSocket();
        }

        // Пробуем прикрепить к слоту
        if (handSocket != null)
        {
            // Создаем оружие
            currentWeapon = Instantiate(weaponPrefab);
            currentWeapon.name = "MeleeWeapon";





            // Прикрепляем к слоту
            currentWeapon.transform.SetParent(handSocket, false);
            currentWeapon.transform.localPosition = positionOffset;
            currentWeapon.transform.localRotation = Quaternion.Euler(rotationOffset);
            Debug.Log($"{gameObject.name}: Attached melee weapon to {handSocket.name} with offset P:{positionOffset} R:{rotationOffset}");

        }
        else
        {
            Debug.LogError($"{gameObject.name}: Could not find hand socket! Trying MeshSockets...");

        }
    }

    void FindHandSocket()
    {
        // Ищем в MeshSockets (ваша иерархия)
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

        // Ищем в Armature (socketRightHand)
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

        // Ищем по имени во всей иерархии
        handSocket = FindDeepChild(transform, socketName);
        if (handSocket != null)
        {
            Debug.Log($"Found socket by name: {socketName}");
            return;
        }

        Debug.LogWarning($"{gameObject.name}: No hand socket found with name {socketName}");
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    // Метод для проверки наличия оружия
    public bool HasWeapon()
    {
        return currentWeapon != null;
    }

    // Метод для получения текущего оружия
    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }
}