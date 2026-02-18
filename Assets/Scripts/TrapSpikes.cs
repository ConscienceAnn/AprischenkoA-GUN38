using UnityEngine;
using System.Collections;

public class TrapSpikes : MonoBehaviour
{
    [Header("Настройки урона")]
    [SerializeField] private float damageAmount = 20f;

    [Header("Настройки времени")]
    [SerializeField] private float activeTime = 2f;
    [SerializeField] private float cooldownTime = 3f;
    [SerializeField] private float minRandomOffset = 0f;
    [SerializeField] private float maxRandomOffset = 2f;

    [Header("Настройки анимации")]
    [SerializeField] private float raiseAnimationTime = 0.5f;

    [Header("Звуки ловушки")]
    [SerializeField] private AudioClip spikesRaiseSound;    // Звук подъема шипов
    [SerializeField] private AudioClip playerPainSound;     // Звук крика игрока (будет исходить из ловушки)
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 0.8f;

    private Animator animator;
    private Collider damageCollider;
    private AudioSource audioSource;
    private bool isActive = false;
    private float randomOffset;

    private void Start()
    {
        animator = GetComponent<Animator>();
        damageCollider = GetComponent<Collider>();

        // Настраиваем AudioSource для 3D звука
        SetupAudioSource();

        if (animator == null)
            Debug.LogError("Animator not found on TrapSpikes!");

        if (damageCollider == null)
            Debug.LogError("Collider not found on TrapSpikes!");
        else
            damageCollider.enabled = false;

        randomOffset = Random.Range(minRandomOffset, maxRandomOffset);
        InvokeRepeating(nameof(StartTrapCycle), randomOffset, cooldownTime + activeTime + raiseAnimationTime);
    }

    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Настройки для 3D звука (слышно только рядом)
        audioSource.spatialBlend = 1f;           // Полностью 3D звук
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 0.2f;             // Максимальная громкость в радиусе 2 метров
        audioSource.maxDistance = 0.7f;             // За пределами 15 метров не слышно
        audioSource.volume = soundVolume;
        audioSource.playOnAwake = false;
    }

    private void StartTrapCycle()
    {
        if (!isActive)
        {
            StartCoroutine(TrapCycle());
        }
    }

    private IEnumerator TrapCycle()
    {
        // Воспроизводим звук подъема шипов
        if (spikesRaiseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spikesRaiseSound, soundVolume);
        }

        animator.SetTrigger("Raise");
        yield return new WaitForSeconds(raiseAnimationTime);

        isActive = true;
        damageCollider.enabled = true;

        yield return new WaitForSeconds(activeTime);

        animator.SetTrigger("Lower");
        isActive = false;
        damageCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamageFromTrap(damageAmount);

                // Воспроизводим звук крика от ловушки (3D звук с позиции ловушки)
                if (playerPainSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(playerPainSound, soundVolume);
                    Debug.Log("Pain sound played from trap position");
                }
            }
        }
    }

}