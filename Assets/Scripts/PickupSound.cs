using UnityEngine;

public class PickupSound : MonoBehaviour
{
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    public void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            // Создаем временный объект для звука
            GameObject soundGO = new GameObject("PickupSound");
            soundGO.transform.position = transform.position;

            AudioSource audioSource = soundGO.AddComponent<AudioSource>();
            audioSource.clip = pickupSound;
            audioSource.volume = soundVolume;
            audioSource.spatialBlend = 1f;
            audioSource.Play();

            // Уничтожаем после звука
            Destroy(soundGO, pickupSound.length);
        }
    }
}