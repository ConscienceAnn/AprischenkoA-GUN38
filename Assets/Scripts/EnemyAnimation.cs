using DG.Tweening;
using UnityEngine;

public class SimpleEnemyAnim : MonoBehaviour
{
    public Transform head;
    public Transform leftArm;
    public Transform rightArm;
    public Renderer headRenderer;

    private Vector3 headOriginalScale;
    private Color headOriginalColor;
    private Vector3 leftArmOriginalPos;
    private Vector3 rightArmOriginalPos;

    void Start()
    {
        // Сохраняем исходные значения
        SaveOriginalValues();

        // Запускаем анимации
        AnimateArms();

        AnimateHead();
    }

    void SaveOriginalValues()
    {
        headOriginalScale = head.localScale;
        headOriginalColor = headRenderer.material.color;
        leftArmOriginalPos = leftArm.localPosition;
        rightArmOriginalPos = rightArm.localPosition;
    }
    void AnimateHead()
    {
        // Очищаем предыдущие анимации головы
        DOTween.Kill(head);
        DOTween.Kill(headRenderer.material);

        // СОЗДАЁМ ОДИН ЦИКЛ И ЗАЦИКЛИВАЕМ ЕГО
        Sequence headSeq = DOTween.Sequence();

        // 1. Увеличиваем и краснеем (ОДИН РАЗ за цикл)
        headSeq.Append(head.DOScale(headOriginalScale * 1.5f, 0.3f));
        headSeq.Join(headRenderer.material.DOColor(Color.red, 0.3f));

        // 2. Возвращаем к исходным значениям
        headSeq.Append(head.DOScale(headOriginalScale, 0.3f));
        headSeq.Join(headRenderer.material.DOColor(headOriginalColor, 0.3f));

        // 3. Пауза перед повторением цикла
        headSeq.AppendInterval(0.5f); // Можно настроить длительность паузы

        // 4. ЗАЦИКЛИВАЕМ этот один цикл БЕСКОНЕЧНО
        headSeq.SetLoops(-1, LoopType.Restart);

        // Комментарий для ТЗ:
        // SetLoops(-1) - бесконечное повторение одного цикла
        // LoopType.Restart - каждый цикл начинается с начала
    }

    void AnimateArms()
    {
        // Очищаем предыдущие анимации
        DOTween.Kill(leftArm);
        DOTween.Kill(rightArm);

        // ЛЕВАЯ РУКА: вверх-вниз относительно исходной позиции
        leftArm.DOLocalMoveY(leftArmOriginalPos.y + 0.1f, 0.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo) // Yoyo: верх  низ верх...
            .SetDelay(0.1f);

        // ПРАВАЯ РУКА: тоже вверх-вниз относительно исходной позиции, но в противофазе
        rightArm.DOLocalMoveY(rightArmOriginalPos.y + 0.1f, 0.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(0.35f); // Задержка для противофазы
    }
}