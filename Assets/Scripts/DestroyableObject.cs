using UnityEngine;
using DG.Tweening;

public class DestroyableObject : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 30f;

    [Header("Effects")]
    public GameObject destroyEffect;
    public AudioClip destroySound;

    [Header("Destruction Animation")]
    public float destroyDuration = 0.5f;
    public float flyApartForce = 2f;

    private AudioSource audioSource;
    private Collider objectCollider;
    private Rigidbody rb;
    private bool isDestroyed = false;

    void Start()
    {
        if (destroySound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        objectCollider = GetComponent<Collider>();

        if (flyApartForce > 0 && GetComponent<Rigidbody>() == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (isDestroyed) return;

        health -= damage;

        if (health <= 0)
        {
            Destroy();
        }
    }

    void Destroy()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (objectCollider != null)
            objectCollider.enabled = false;

        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        if (destroySound != null && audioSource != null)
            audioSource.PlayOneShot(destroySound);

        // Рандомная анимация
        int animType = Random.Range(0, 2);

        if (animType == 0)
        {
            // Просто исчезает
            transform.DOScale(0, destroyDuration).SetEase(Ease.InBack)
                .OnComplete(() => Destroy(gameObject));
        }
        else
        {
            // Падает и исчезает
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(Vector3.up * flyApartForce + Random.insideUnitSphere * flyApartForce, ForceMode.Impulse);
            }
            transform.DOScale(0, destroyDuration).SetDelay(0.2f)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}