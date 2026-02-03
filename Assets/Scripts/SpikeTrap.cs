using DG.Tweening;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Основные настройки")]
    public float riseHeight = 0.150f;      // Высота подъёма
    public float riseTime = 0.5f;      // Время подъёма
    public float stayTime = 1f;        // Время в поднятом состоянии
    public float lowerTime = 0.5f;     // Время опускания
    public float waitTime = 2f;        // Ожидание перед повторением

    [Header("Easing функции")]
    public Ease riseEase = Ease.OutBack;    // Эффект "пружины" при подъёме
    public Ease lowerEase = Ease.InBack;    // Эффект "вдавливания" при опускании

    private Vector3 originalPosition;
    private Vector3 raisedPosition;

    void Start()
    {
        // Сохраняем исходную позицию
        originalPosition = transform.localPosition;

        // Рассчитываем позицию поднятых шипов
        raisedPosition = originalPosition + Vector3.up * riseHeight;

        // Запускаем анимацию
        StartAnimation();
    }

    void StartAnimation()
    {
        // Создаём последовательность анимаций
        Sequence spikeSequence = DOTween.Sequence();

        // 1. ПОДЪЁМ с эффектом "пружины"
        spikeSequence.Append(
            transform.DOLocalMove(raisedPosition, riseTime)
                .SetEase(riseEase)
                .OnStart(() => {
                    Debug.Log("Шипы поднимаются!");
                    // Включить коллайдер урона здесь
                })
        );

        // 2. ПАУЗА в поднятом состоянии
        spikeSequence.AppendInterval(stayTime);

        // 3. ОПУСКАНИЕ с эффектом
        spikeSequence.Append(
            transform.DOLocalMove(originalPosition, lowerTime)
                .SetEase(lowerEase)
                .OnComplete(() => {
                    Debug.Log("Шипы опустились!");
                    // Выключить коллайдер урона здесь
                })
        );

        // 4. ОЖИДАНИЕ перед следующим циклом
        spikeSequence.AppendInterval(waitTime);

        // 5. ЗАЦИКЛИВАЕМ
        spikeSequence.SetLoops(-1, LoopType.Restart);

       
        // SetLoops(-1) означает бесконечное повторение
        // LoopType.Restart - каждый цикл начинается сначала
    }

    // Для нанесения урона игроку
    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        // Проверяем, активны ли шипы в данный момент
    //        float currentHeight = transform.localPosition.y;
    //        float activeThreshold = originalPosition.y + (riseHeight * 0.7f);

    //        if (currentHeight > activeThreshold)
    //        {
    //            Debug.Log("Игрок получил урон от шипов!");
    //            // Нанести урон игроку
    //            // other.GetComponent<PlayerHealth>().TakeDamage(10);
    //        }
    //    }
    //}

}