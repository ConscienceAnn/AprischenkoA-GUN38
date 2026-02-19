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
        if (renderer != null)
        {
            zoneMaterial = renderer.material;
            zoneMaterial.color = zoneColor;
        }

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
        if (zoneMaterial != null)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            Color newColor = zoneColor;
            newColor.a = currentAlpha;
            zoneMaterial.color = newColor;
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Проверяем по тегу Agent (как вы предложили)
        if (!other.CompareTag("Agent")) return;

        AiAgent agent = other.GetComponent<AiAgent>();

        // Проверяем, существует ли агент и включен ли он
        if (agent != null && agent.enabled && agent.isActiveAndEnabled)
        {
            // Отключаем стрельбу ТОЛЬКО если есть компонент weapons
            if (agent.weapons != null)
            {
                agent.weapons.SetFiring(false);
            }
            // Для MeleeEnemy weapons = null, поэтому просто пропускаем

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

    void PushEnemy(AiAgent agent, Collider enemyCollider)
    {
        // Проверяем, существует ли агент и коллайдер
        if (agent == null || enemyCollider == null) return;

        Vector3 awayFromZone = enemyCollider.transform.position - transform.position;
        awayFromZone.y = 0;

        if (awayFromZone.magnitude < 0.1f)
        {
            awayFromZone = Random.insideUnitSphere;
            awayFromZone.y = 0;
        }

        awayFromZone.Normalize();

        // Разворачиваем врага
        if (enemyCollider.transform != null)
        {
            enemyCollider.transform.rotation = Quaternion.LookRotation(awayFromZone);
        }

        // Используем Warp для телепортации (работает даже с kinematic)
        if (agent.navMeshAgent != null && agent.navMeshAgent.isOnNavMesh && agent.navMeshAgent.enabled)
        {
            Vector3 newPosition = enemyCollider.transform.position + awayFromZone * 3f;

            // Проверяем, что новая позиция на NavMesh
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(newPosition, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.navMeshAgent.Warp(hit.position);
            }
        }

        if (blockSound != null && audioSource != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(blockSound);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Проверяем по тегу Agent
        if (!other.CompareTag("Agent")) return;

        AiAgent agent = other.GetComponent<AiAgent>();
        if (agent != null)
        {
            if (enemyPushTimers.ContainsKey(agent))
            {
                enemyPushTimers.Remove(agent);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Вы вошли в безопасную зону!");
            if (enterSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(enterSound);
            }
        }

        // Можно добавить лог для агентов
        if (other.CompareTag("Agent"))
        {
            Debug.Log("Враг вошел в безопасную зону");
        }
    }

    // Очистка словаря при уничтожении объекта
    void OnDestroy()
    {
        enemyPushTimers.Clear();
    }
}