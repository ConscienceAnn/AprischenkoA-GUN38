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
    private MeleeCombat meleeCombat; // Ссылка на компонент боя

    void Start()
    {
        meshSockets = GetComponent<MeshSockets>();
        meleeCombat = GetComponent<MeleeCombat>(); // Получаем ссылку на MeleeCombat
        FindHandSocket();
    }

    public void PickupWeapon(GameObject weaponPrefab)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }
        StartCoroutine(AttachWeapon(weaponPrefab));
    }

    IEnumerator AttachWeapon(GameObject weaponPrefab)
    {
        // Ждем кадр для инициализации
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

            // Ждем еще немного, чтобы оружие точно заспавнилось
            yield return new WaitForEndOfFrame();

            // Получаем компонент MeleeWeapon на созданном оружии
            MeleeWeapon weaponComponent = currentWeapon.GetComponent<MeleeWeapon>();

            // Проверяем, есть ли AttackPoint в оружии
            if (weaponComponent != null)
            {
                if (weaponComponent.attackPoint == null)
                {
                    Debug.LogWarning($"{gameObject.name}: Weapon has MeleeWeapon component but no AttackPoint assigned in prefab!");
                }
                else
                {
                    Debug.Log($"{gameObject.name}: Weapon has AttackPoint at {weaponComponent.attackPoint.name}");
                }
            }

            // Сообщаем MeleeCombat, что оружие готово
            if (meleeCombat != null)
            {
                meleeCombat.OnWeaponAttached(weaponComponent);
                Debug.Log($"{gameObject.name}: Notified MeleeCombat about weapon attachment");
            }
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

    // Метод для проверки, есть ли оружие
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