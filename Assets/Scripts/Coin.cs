using DG.Tweening;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private bool isCollected = false;

    void Start()
    {
        // Автоматическая анимация при создании
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
        // Проверяем тег и что монетка ещё не собрана
        if (!isCollected && other.CompareTag("Player"))
        {
            Debug.Log("Монетка собрана!");
            isCollected = true;
            Collect();
        }
    }

    void Collect()
    {
        // Останавливаем все анимации
        transform.DOKill();

        // Анимация сбора
        Sequence collectSequence = DOTween.Sequence();

        // 1. Прыжок вверх
        collectSequence.Append(
            transform.DOJump(
                transform.position + Vector3.up * 2f, // Прыжок вверх
                0.5f,    // Высота прыжка
                1,     // Количество прыжков
                0.5f   // Длительность
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


        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Renderer playerRenderer = player.GetComponent<Renderer>();
            if (playerRenderer != null)
            {
                // Мигание жёлтым цветом
                Sequence playerEffect = DOTween.Sequence();
                Color originalColor = playerRenderer.material.color;

                playerEffect.Append(
                    playerRenderer.material.DOColor(Color.yellow, 0.1f)
                );

                playerEffect.Append(
                    playerRenderer.material.DOColor(originalColor, 0.1f)
                );

                playerEffect.SetLoops(3, LoopType.Yoyo);
            }
        }
    }

    // Для отладки - показываем зону триггера
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