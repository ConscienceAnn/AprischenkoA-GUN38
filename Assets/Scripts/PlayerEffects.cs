using DG.Tweening;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    [Header("Ссылки на меши")]
    public Renderer bodyMesh;
    public Renderer headMesh;

    [Header("Цвета эффектов")]
    public Color damageColor = Color.red;
    public Color damageColor1 = Color.green;
    public Color coinColor = Color.yellow;

    [Header("Настройки анимации")]
    public float flashSpeed = 0.1f;
    public int trapFlashCount = 1;    // Ловушки мигания
    public int enemyFlashCount = 1;   // Враг мигания  
    public int coinFlashCount = 1;    // Монетка мигания

    private Color bodyOriginalColor;
    private Color headOriginalColor;

    void Start()
    {
        if (bodyMesh != null) bodyOriginalColor = bodyMesh.material.color;
        if (headMesh != null) headOriginalColor = headMesh.material.color;
    }

    // Эффект при получении урона от ловушки
    public void TakeTrapDamage()
    {
        Debug.Log("Урон от ловушки");
        FlashEffect(damageColor, trapFlashCount);
    }

    // Эффект при столкновении с врагом
    public void TakeEnemyDamage()
    {
        Debug.Log("Урон от врага");
        FlashEffect(damageColor1, enemyFlashCount);
    }

    // Эффект при сборе монетки
    public void CollectCoin()
    {
        Debug.Log("Собрана монетка!");
        FlashEffect(coinColor, coinFlashCount);

        // Дополнительная анимация для монетки (легкое увеличение)
        Sequence coinSequence = DOTween.Sequence();
        coinSequence.Append(
            transform.DOScale(transform.localScale * 1.1f, 0.15f)
                .SetEase(Ease.OutBack)
        );
        coinSequence.Append(
            transform.DOScale(Vector3.one, 0.15f)
                .SetEase(Ease.InBack)
        );
    }

    // Общий метод мигания с DOTween
    void FlashEffect(Color flashColor, int flashCount)
    {
        // Останавливаем предыдущие анимации
        if (bodyMesh != null) bodyMesh.material.DOKill();
        if (headMesh != null) headMesh.material.DOKill();

        // Мигание для body mesh
        if (bodyMesh != null)
        {
            bodyMesh.material.DOColor(flashColor, flashSpeed)
                .SetLoops(flashCount * 2, LoopType.Yoyo) // *2 потому что Yoyo = туда-обратно
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    if (bodyMesh != null)
                        bodyMesh.material.color = bodyOriginalColor;
                });
        }

        // Мигание для head mesh
        if (headMesh != null)
        {
            headMesh.material.DOColor(flashColor, flashSpeed)
                .SetLoops(flashCount * 2, LoopType.Yoyo)
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    if (headMesh != null)
                        headMesh.material.color = headOriginalColor;
                });
        }
    }

    // Сброс всех эффектов
    public void ResetEffects()
    {
        if (bodyMesh != null)
        {
            bodyMesh.material.DOKill();
            bodyMesh.material.color = bodyOriginalColor;
        }

        if (headMesh != null)
        {
            headMesh.material.DOKill();
            headMesh.material.color = headOriginalColor;
        }

        transform.DOKill();
        transform.localScale = Vector3.one;
    }
}