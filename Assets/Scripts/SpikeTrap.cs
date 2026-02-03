using DG.Tweening;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Анимация")]
    public float riseHeight = 0.15f;
    public float riseTime = 0.5f;
    public float stayTime = 0.5f;
    public float lowerTime = 0.5f;
    public float waitTime = 1f;

    [Header("Ссылки")]
    public Renderer trapMesh;

    [Header("Эффект")]
    public Color bloodColor = Color.red;

    private Vector3 originalPosition;
    private Vector3 raisedPosition;
    private MaterialPropertyBlock propertyBlock;
    private Color originalColor;
    private bool hasBlood = false; // Флаг "крови" на ловушке

    void Start()
    {
        originalPosition = transform.localPosition;
        raisedPosition = originalPosition + Vector3.up * riseHeight;

        if (trapMesh == null)
        {
            trapMesh = GetComponent<Renderer>();
            if (trapMesh == null)
            {
                trapMesh = GetComponentInChildren<Renderer>();
            }
        }

        if (trapMesh != null)
        {
            propertyBlock = new MaterialPropertyBlock();
            trapMesh.GetPropertyBlock(propertyBlock);

            if (propertyBlock.isEmpty)
            {
                originalColor = trapMesh.material.color;
            }
            else if (propertyBlock.HasProperty("_Color"))
            {
                originalColor = propertyBlock.GetColor("_Color");
            }

            Debug.Log($"Ловушка '{gameObject.name}' инициализирована");
        }

        StartAnimation();
    }

    void StartAnimation()
    {
        Sequence spikeSequence = DOTween.Sequence();

        // ПОДЪЕМ
        spikeSequence.Append(
            transform.DOLocalMove(raisedPosition, riseTime)
                .SetEase(Ease.OutBack)
        );

        // ПАУЗА (опасное состояние)
        spikeSequence.AppendInterval(stayTime);

        // ОПУСКАНИЕ
        spikeSequence.Append(
            transform.DOLocalMove(originalPosition, lowerTime)
                .SetEase(Ease.InBack)
        );

        // ОЖИДАНИЕ
        spikeSequence.AppendInterval(waitTime);

        spikeSequence.SetLoops(-1, LoopType.Restart);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Игрок коснулся ловушки '{gameObject.name}'!");

            // Эффект на игроке ВСЕГДА при касании!
            PlayerEffects playerEffects = other.GetComponent<PlayerEffects>();
            if (playerEffects != null)
            {
                playerEffects.TakeTrapDamage();
            }

            // Эффект на ловушке (если еще нет "крови")
            if (!hasBlood && propertyBlock != null && trapMesh != null)
            {
                propertyBlock.SetColor("_Color", bloodColor);
                trapMesh.SetPropertyBlock(propertyBlock);
                hasBlood = true;

                // Сбрасываем через 3 секунды
                Invoke(nameof(ResetTrapColor), 3f);
            }
        }
    }

    void ResetTrapColor()
    {
        if (propertyBlock != null && trapMesh != null)
        {
            propertyBlock.SetColor("_Color", originalColor);
            trapMesh.SetPropertyBlock(propertyBlock);
            hasBlood = false;
        }
    }
}