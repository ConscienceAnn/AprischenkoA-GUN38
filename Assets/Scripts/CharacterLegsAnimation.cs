using DG.Tweening;
using UnityEngine;

public class  CharacterLegsAnimation: MonoBehaviour
{
    public Transform legLeft;
    public Transform legRight;

    public float stepDistance = 0.3f;
    public float liftHeight = 0.15f;
    public float stepTime = 0.4f;

    private Vector3 leftHome;
    private Vector3 rightHome;
    private Sequence walkCycle;

    void Start()
    {
        leftHome = legLeft.localPosition;
        rightHome = legRight.localPosition;

        CreateWalkCycle();
    }

    void CreateWalkCycle()
    {
        DOTween.Kill(legLeft);
        DOTween.Kill(legRight);

        walkCycle = DOTween.Sequence();

        // --- ПРАВАЯ НОГА ШАГАЕТ ---
        // 1. Правая поднимается и идёт вперёд
        walkCycle.AppendCallback(() => Debug.Log("Правая нога шагает"));
        walkCycle.Append(
            legRight.DOLocalMove(new Vector3(
                rightHome.x,
                rightHome.y + liftHeight,
                rightHome.z + stepDistance),
                stepTime)
            .SetEase(Ease.OutSine)
        );

        // 2. Левая немного отодвигается назад (имитация опоры)
        walkCycle.Join(
            legLeft.DOLocalMoveZ(leftHome.z - (stepDistance * 0.2f), stepTime)
                .SetEase(Ease.OutSine)
        );

        // 3. Правая опускается
        walkCycle.Append(
            legRight.DOLocalMoveY(rightHome.y, stepTime * 0.3f)
                .SetEase(Ease.InSine)
        );

        // --- ПАУЗА МЕЖДУ ШАГАМИ ---
        walkCycle.AppendInterval(stepTime * 0.1f);

        // --- ЛЕВАЯ НОГА ШАГАЕТ ---
        // 4. Левая поднимается и идёт вперёд
        walkCycle.AppendCallback(() => Debug.Log("Левая нога шагает"));
        walkCycle.Append(
            legLeft.DOLocalMove(new Vector3(
                leftHome.x,
                leftHome.y + liftHeight,
                leftHome.z + stepDistance),
                stepTime)
            .SetEase(Ease.OutSine)
        );

        // 5. Правая немного отодвигается назад
        walkCycle.Join(
            legRight.DOLocalMoveZ(rightHome.z - (stepDistance * 0.2f), stepTime)
                .SetEase(Ease.OutSine)
        );

        // 6. Левая опускается
        walkCycle.Append(
            legLeft.DOLocalMoveY(leftHome.y, stepTime * 0.3f)
                .SetEase(Ease.InSine)
        );

        // --- ВОЗВРАТ В НЕЙТРАЛЬНОЕ ПОЛОЖЕНИЕ ---
        walkCycle.Append(
            legLeft.DOLocalMoveZ(leftHome.z, stepTime * 0.2f)
                .SetEase(Ease.InOutSine)
        );

        walkCycle.Join(
            legRight.DOLocalMoveZ(rightHome.z, stepTime * 0.2f)
                .SetEase(Ease.InOutSine)
        );

        // Зацикливаем
        walkCycle.SetLoops(-1, LoopType.Restart);
    }

    public void StopWalk()
    {
        if (walkCycle != null && walkCycle.IsActive())
            walkCycle.Kill();

        legLeft.localPosition = leftHome;
        legRight.localPosition = rightHome;
    }
}