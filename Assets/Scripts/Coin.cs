using DG.Tweening;
using UnityEngine;

public class Coin : MonoBehaviour
{
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
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        // Анимация сбора
        transform.DOScale(Vector3.zero, 0.3f)
            .OnComplete(() => Destroy(gameObject));
    }
}