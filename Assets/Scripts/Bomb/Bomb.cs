using UnityEngine;
using System.Collections.Generic;

public class Bomb : MonoBehaviour
{
    [Header("Настройки")]
    public float damage = 10f;
    public float explosionRadius = 3f;
    public GameObject explosionEffect;

    [Header("Звуки")]
    public AudioClip painSound; // Звук боли от ловушки (игрок)
    public AudioClip explosionSound; // Звук взрыва бомбы
    [Range(0f, 1f)]
    public float painSoundVolume = 1f;
    [Range(0f, 1f)]
    public float explosionSoundVolume = 1f;

    private bool hasExploded = false;
    private List<Health> damagedTargets = new List<Health>();
    private AudioSource explosionAudioSource; // Для взрыва
    private AudioSource painAudioSource; // Для боли

    void Start()
    {
        // Создаем два AudioSource для разных звуков
        CreateAudioSources();
    }

    void CreateAudioSources()
    {
        // Для взрыва
        explosionAudioSource = gameObject.AddComponent<AudioSource>();
        explosionAudioSource.spatialBlend = 1f;
        explosionAudioSource.playOnAwake = false;

        // Для боли
        painAudioSource = gameObject.AddComponent<AudioSource>();
        painAudioSource.spatialBlend = 1f;
        painAudioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasExploded && other.CompareTag("Player"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        damagedTargets.Clear();

        Debug.Log($"Бомба взорвалась! Радиус: {explosionRadius}, Урон: {damage}");

        // Наносим урон
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            Health healthComponent = col.GetComponentInParent<Health>();
            if (healthComponent != null && !damagedTargets.Contains(healthComponent))
            {
                damagedTargets.Add(healthComponent);
                Vector3 direction = (col.transform.position - transform.position).normalized;
                healthComponent.TakeDamage(damage, direction);
            }
        }

        // Визуальный эффект
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // ЗВУК ВЗРЫВА
        if (explosionSound != null)
        {
            explosionAudioSource.PlayOneShot(explosionSound, explosionSoundVolume);
            Debug.Log($"Звук взрыва: {explosionSound.name}");
        }

        // ЗВУК БОЛИ (если есть пострадавшие)
        if (painSound != null && damagedTargets.Count > 0)
        {
            painAudioSource.PlayOneShot(painSound, painSoundVolume);
            Debug.Log($"Звук боли: {painSound.name}");
        }

        // Прячем бомбу
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Collider colBomb = GetComponent<Collider>();
        if (colBomb != null) colBomb.enabled = false;

        // Уничтожаем после самого длинного звука
        float longestSound = Mathf.Max(
            explosionSound != null ? explosionSound.length : 0,
            painSound != null ? painSound.length : 0
        );

        Destroy(gameObject, longestSound + 0.1f); // +0.1f небольшой запас
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}