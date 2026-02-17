using UnityEngine;
using UnityEngine.AI;

public class EnemyFootsteps : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip footstepSound;
    public float stepInterval = 0.7f;
    public float minSpeed = 0.5f;

    [Header("3D Sound Settings")]
    public float minDistance = 3f;    // Минимальное расстояние, где звук громкий
    public float maxDistance = 20f;   // Максимальное расстояние, где звук слышен

    private NavMeshAgent agent;
    private float stepTimer;
    private Vector3 lastPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Настраиваем AudioSource
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // ВАЖНО: Настройки 3D звука
        audioSource.spatialBlend = 1f;           // Полностью 3D звук
        audioSource.rolloffMode = AudioRolloffMode.Linear; // Линейное затухание
        audioSource.minDistance = minDistance;    // Громкость на ближней дистанции
        audioSource.maxDistance = maxDistance;    // Дальность слышимости
        audioSource.dopplerLevel = 0f;            // Отключаем эффект Доплера (не нужен для шагов)

        lastPosition = transform.position;
    }

    void Update()
    {
        // Проверяем, жив ли враг
        AiAgent aiAgent = GetComponent<AiAgent>();
        if (aiAgent != null && aiAgent.health.IsDead())
            return;

        // Скорость движения
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        // Если скорость маленькая - не играем шаги
        if (speed < minSpeed || !agent.isOnNavMesh || !agent.hasPath)
        {
            stepTimer = 0;
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0)
        {
            PlayFootstep();
            stepTimer = stepInterval;
        }
    }

    void PlayFootstep()
    {
        if (footstepSound == null) return;

        // Небольшая вариация тона
        audioSource.pitch = 1f + Random.Range(-0.1f, 0.1f);

        // Проигрываем звук
        audioSource.PlayOneShot(footstepSound);

        // Визуальная отладка (можно убрать)
        Debug.Log($"Enemy footstep at {transform.position}, distance to player: {GetDistanceToPlayer()}");
    }

    float GetDistanceToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            return Vector3.Distance(transform.position, player.transform.position);
        return 0f;
    }

    // Для визуализации радиуса слышимости в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}