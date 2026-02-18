using UnityEngine;

public class TrapSpikes : MonoBehaviour
{
    [Header("Настройки урона")]
    [SerializeField] private float damageAmount = 20f;

    [Header("Настройки времени")]
    [SerializeField] private float activeTime = 2f; // сколько времени шипы подняты
    [SerializeField] private float cooldownTime = 3f; // время между подъемами
    [SerializeField] private float minRandomOffset = 0f; // для разнобоя между ловушками
    [SerializeField] private float maxRandomOffset = 2f;

    private Animator animator;
    private Collider damageCollider;
    private bool isActive = false;
    private float randomOffset;

    private void Start()
    {
        animator = GetComponent<Animator>();
        damageCollider = GetComponent<Collider>();

        // Случайное смещение для каждой ловушки
        randomOffset = Random.Range(minRandomOffset, maxRandomOffset);

        // Запускаем цикл ловушки
        InvokeRepeating(nameof(StartTrapCycle), randomOffset, cooldownTime + activeTime);
    }

    private void StartTrapCycle()
    {
        if (!isActive)
        {
            StartCoroutine(TrapCycle());
        }
    }

    private System.Collections.IEnumerator TrapCycle()
    {
        // Запускаем анимацию подъема
        animator.SetTrigger("Raise");

        // Ждем пока шипы поднимутся (можно настроить под вашу анимацию)
        yield return new WaitForSeconds(0.5f);

        // Шипы подняты - включаем урон
        isActive = true;
        damageCollider.enabled = true;

        // Ждем пока шипы активны
        yield return new WaitForSeconds(activeTime);

        // Опускаем шипы
        animator.SetTrigger("Lower");
        isActive = false;
        damageCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем что шипы активны и это игрок
        if (isActive && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamageFromTrap(damageAmount);
                Debug.Log($"Ловушка нанесла {damageAmount} урона игроку");
            }
        }
    }

    // Для визуализации в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.up, new Vector3(2f, 1f, 2f));
    }
}