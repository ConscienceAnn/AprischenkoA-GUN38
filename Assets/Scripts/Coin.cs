using DG.Tweening;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private bool isCollected = false;

    void Start()
    {
        StartAnimation();
    }

    public void StartAnimation()
    {
        // Вращение
        transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);

        // Парение
        transform.DOMoveY(transform.position.y + 0.3f, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            Debug.Log("Монетка собрана!");
            isCollected = true;
            Collect(other.gameObject);
        }
    }

    void Collect(GameObject player)
    {
        // Останавливаем все анимации
        transform.DOKill();

        // Анимация сбора
        Sequence collectSequence = DOTween.Sequence();

        // 1. Прыжок вверх
        collectSequence.Append(
            transform.DOJump(
                transform.position + Vector3.up * 2f,
                0.5f, 1, 0.5f
            )
        );

        // 2. Увеличение и вращение
        collectSequence.Join(
            transform.DOScale(transform.localScale * 1.5f, 0.3f)
        );
        collectSequence.Join(
            transform.DORotate(new Vector3(0, 720, 0), 0.5f, RotateMode.LocalAxisAdd)
        );

        // 3. Исчезновение
        collectSequence.Append(
            transform.DOScale(Vector3.zero, 0.2f)
        );

        // 4. Уничтожение объекта
        collectSequence.OnComplete(() => {
            Destroy(gameObject);
        });

        // Вызываем эффект у игрока
        PlayerEffects playerEffects = player.GetComponent<PlayerEffects>();
        if (playerEffects != null)
        {
            playerEffects.CollectCoin();
           
        }
        else
        {
            Debug.LogWarning("PlayerEffects не найден на игроке!");
        }
    }

    // Для отладки
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}