using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip footstepSound; // Один звук вместо массива
    public float stepInterval = 0.5f;

    [Header("Speed Thresholds")]
    public float walkThreshold = 0.1f;
    public float runThreshold = 5f;

    private CharacterController controller;
    private float stepTimer;
    private Vector3 lastPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        lastPosition = transform.position;
    }

    void Update()
    {
        // Скорость движения
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        // Проверяем, на земле ли игрок и двигается ли
        if (!controller.isGrounded || speed < walkThreshold)
        {
            stepTimer = 0;
            return;
        }

        // Интервал шагов зависит от скорости
        float currentInterval = stepInterval;
        if (speed > runThreshold)
            currentInterval = stepInterval * 0.6f; // Чаще при беге

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0)
        {
            PlayFootstep();
            stepTimer = currentInterval;
        }
    }

    void PlayFootstep()
    {
        if (footstepSound == null) return;

        audioSource.pitch = 1f + Random.Range(-0.1f, 0.1f);
        audioSource.PlayOneShot(footstepSound);
    }
}