using DG.Tweening;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [Header("Ссылки на части врага")]
    public Transform head;
    public Transform leftArm;
    public Transform rightArm;
    public Renderer headRenderer;

    [Header("Эффект при касании")]
    public Color attackColor = new Color(1, 0.5f, 0.5f); 

    [Header("Анимация")]
    public float headScaleAmount = 1.5f;
    public float headPulseDuration = 0.3f;
    public float armMoveAmount = 0.1f;
    public float armMoveDuration = 0.2f;

    private Vector3 headOriginalScale;
    private Color headOriginalColor;
    private Vector3 leftArmOriginalPos;
    private Vector3 rightArmOriginalPos;
    private MaterialPropertyBlock propertyBlock;

    void Start()
    {
        // Сохраняем исходные значения
        SaveOriginalValues();

        if (headRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
            headRenderer.GetPropertyBlock(propertyBlock);
            headOriginalColor = propertyBlock.HasProperty("_Color") ?
                propertyBlock.GetColor("_Color") : headRenderer.material.color;
        }

        // Запускаем анимации
        AnimateArms();
        AnimateHead();

        Debug.Log($"Враг '{gameObject.name}' инициализирован (триггер)");
    }

    void SaveOriginalValues()
    {
        if (head != null) headOriginalScale = head.localScale;
        if (headRenderer != null) headOriginalColor = headRenderer.material.color;
        if (leftArm != null) leftArmOriginalPos = leftArm.localPosition;
        if (rightArm != null) rightArmOriginalPos = rightArm.localPosition;
    }

    void AnimateHead()
    {
        if (head == null || headRenderer == null) return;

        DOTween.Kill(head);

        Sequence headSeq = DOTween.Sequence();
        headSeq.Append(head.DOScale(headOriginalScale * headScaleAmount, headPulseDuration));
        headSeq.Append(head.DOScale(headOriginalScale, headPulseDuration));
        headSeq.AppendInterval(0.5f);
        headSeq.SetLoops(-1, LoopType.Restart);
    }

    void AnimateArms()
    {
        if (leftArm != null)
        {
            leftArm.DOLocalMoveY(leftArmOriginalPos.y + armMoveAmount, armMoveDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(0.1f);
        }

        if (rightArm != null)
        {
            rightArm.DOLocalMoveY(rightArmOriginalPos.y + armMoveAmount, armMoveDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(0.35f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Враг '{gameObject.name}' коснулся игрока!");

            // 1. Эффект у врага (мгновенное покраснение головы)
            PlayAttackEffect();

            // 2. Эффект у игрока
            PlayerEffects playerEffects = other.GetComponent<PlayerEffects>();
            if (playerEffects != null)
            {
                playerEffects.TakeEnemyDamage();
            }
            else
            {
                Debug.LogWarning("PlayerEffects не найден на игроке!");
            }
        }
    }

    void PlayAttackEffect()
    {
        if (headRenderer == null) return;

        // Быстрое покраснение и возврат
        if (propertyBlock != null)
        {
            // Красный
            propertyBlock.SetColor("_Color", attackColor);
            headRenderer.SetPropertyBlock(propertyBlock);

            // Возвращаем через 0.3 секунды
            Invoke(nameof(ResetHeadColor), 0.3f);
        }
        else
        {
            // Fallback: через DOTween
            headRenderer.material.DOColor(attackColor, 0.1f)
                .OnComplete(() => headRenderer.material.DOColor(headOriginalColor, 0.2f));
        }
    }

    void ResetHeadColor()
    {
        if (headRenderer != null && propertyBlock != null)
        {
            propertyBlock.SetColor("_Color", headOriginalColor);
            headRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    void OnDestroy()
    {
        // Очистка при уничтожении
        DOTween.Kill(head);
        DOTween.Kill(leftArm);
        DOTween.Kill(rightArm);

        if (headRenderer != null)
        {
            DOTween.Kill(headRenderer.material);

            // Восстанавливаем цвет
            if (propertyBlock != null)
            {
                propertyBlock.SetColor("_Color", headOriginalColor);
                headRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}