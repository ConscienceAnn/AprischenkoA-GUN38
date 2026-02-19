using UnityEngine;

public class MeleeWeaponPickup : MonoBehaviour
{
    [Header("Weapon Settings")]
    public MeleeWeapon meleeWeaponFab; // Префаб оружия ближнего боя

    [Header("Visual Effects")]
    public float rotationSpeed = 90f;
    public float bobSpeed = 1f;
    public float bobHeight = 0.3f;

    private Vector3 startPosition;
    private float bobOffset;

    void Start()
    {
        startPosition = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2);
    }

    void Update()
    {
        // Вращение для красоты
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Покачивание вверх-вниз
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    // Публичный метод для получения префаба оружия
    public GameObject GetWeaponPrefab()
    {
        if (meleeWeaponFab != null)
        {
            return meleeWeaponFab.gameObject;
        }
        return null;
    }

    // Метод для получения компонента MeleeWeapon
    public MeleeWeapon GetMeleeWeapon()
    {
        return meleeWeaponFab;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем для AI врага
        AttachMeleeWeapon attachMelee = other.gameObject.GetComponent<AttachMeleeWeapon>();
        if (attachMelee != null)
        {
            // Создаем экземпляр оружия
            if (meleeWeaponFab != null)
            {
                MeleeWeapon newWeapon = Instantiate(meleeWeaponFab);
                newWeapon.name = "MeleeWeapon";

                // Прикрепляем к врагу
                attachMelee.PickupWeapon(newWeapon.gameObject);

                // Уничтожаем пикап
                Destroy(gameObject);

                Debug.Log($"{other.gameObject.name} picked up melee weapon");
            }
            return;
        }

        // Проверяем для игрока (если нужно)
        ActiveWeapon activeWeapon = other.gameObject.GetComponent<ActiveWeapon>();
        if (activeWeapon)
        {
            Debug.Log("Player touched melee weapon pickup");
            // Здесь можно добавить логику для игрока
            return;
        }
    }

    // Визуализация радиуса срабатывания в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);

        if (meleeWeaponFab != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2);
        }
    }
}