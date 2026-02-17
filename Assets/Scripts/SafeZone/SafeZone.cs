using UnityEngine;
using System.Collections.Generic;

public class SafeZone : MonoBehaviour
{
    [Header("Settings")]
    public float pushForce = 15f;
    public float pushCooldown = 0.5f;

    [Header("Visual")]
    public Color zoneColor = new Color(0, 1, 0, 0.3f);
    public float pulseSpeed = 2f;
    public float minAlpha = 0.1f;
    public float maxAlpha = 0.5f;

    [Header("Sound")]
    public AudioClip enterSound;
    public AudioClip blockSound;
    public float soundVolume = 0.7f;

    private Material zoneMaterial;
    private AudioSource audioSource;
    private Dictionary<AiAgent, float> enemyPushTimers = new Dictionary<AiAgent, float>();

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        zoneMaterial = renderer.material;
        zoneMaterial.color = zoneColor;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 1f;
        audioSource.volume = soundVolume;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        Color newColor = zoneColor;
        newColor.a = currentAlpha;
        zoneMaterial.color = newColor;
    }

    void OnTriggerStay(Collider other)
    {
        AiAgent agent = other.GetComponent<AiAgent>();
        if (agent != null && agent.enabled)
        {
            // Отключаем стрельбу
            agent.weapons.SetFiring(false);

            float lastPushTime = 0f;
            if (enemyPushTimers.ContainsKey(agent))
            {
                lastPushTime = enemyPushTimers[agent];
            }

            if (Time.time - lastPushTime > pushCooldown)
            {
                PushEnemy(agent, other);
                enemyPushTimers[agent] = Time.time;
            }
        }
    }

    // !!! ЗАМЕНИТЕ СТАРЫЙ МЕТОД НА ЭТОТ !!!
    void PushEnemy(AiAgent agent, Collider enemyCollider)
    {
        Vector3 awayFromZone = enemyCollider.transform.position - transform.position;
        awayFromZone.y = 0;

        if (awayFromZone.magnitude < 0.1f)
        {
            awayFromZone = Random.insideUnitSphere;
            awayFromZone.y = 0;
        }

        awayFromZone.Normalize();

        // Разворачиваем врага
        enemyCollider.transform.rotation = Quaternion.LookRotation(awayFromZone);

        // Используем Warp для телепортации (работает даже с kinematic)
        if (agent.navMeshAgent != null && agent.navMeshAgent.isOnNavMesh)
        {
            Vector3 newPosition = enemyCollider.transform.position + awayFromZone * 3f;
            agent.navMeshAgent.Warp(newPosition);
        }

        if (blockSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(blockSound);
        }
    }

    void OnTriggerExit(Collider other)
    {
        AiAgent agent = other.GetComponent<AiAgent>();
        if (agent != null)
        {
            if (enemyPushTimers.ContainsKey(agent))
            {
                enemyPushTimers.Remove(agent);
            }
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("Вы покинули безопасную зону");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Вы вошли в безопасную зону!");
            if (enterSound != null)
            {
                audioSource.PlayOneShot(enterSound);
            }
        }
    }
}