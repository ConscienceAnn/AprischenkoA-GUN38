using DG.Tweening;
using UnityEngine;

public class CharacterLegsAnimation : MonoBehaviour
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

        // Àâòîçàïóñê ìîæíî óáðàòü, åñëè áóäåò óïðàâëÿòüñÿ èç CharacterMovement
        // StartWalkingAnimation();
    }

    // Ïóáëè÷íûé ìåòîä äëÿ çàïóñêà (ñîâìåñòèìîñòü ñ CharacterMovement)
    public void StartWalkingAnimation()
    {
        DOTween.Kill(legLeft);
        DOTween.Kill(legRight);

        walkCycle = DOTween.Sequence();

        // --- ÏÐÀÂÀß ÍÎÃÀ ØÀÃÀÅÒ ---
        walkCycle.Append(
            legRight.DOLocalMove(new Vector3(
                rightHome.x,
                rightHome.y + liftHeight,
                rightHome.z + stepDistance),
                stepTime)
            .SetEase(Ease.OutSine)
        );

        walkCycle.Join(
            legLeft.DOLocalMoveZ(leftHome.z - (stepDistance * 0.2f), stepTime)
                .SetEase(Ease.OutSine)
        );

        walkCycle.Append(
            legRight.DOLocalMoveY(rightHome.y, stepTime * 0.3f)
                .SetEase(Ease.InSine)
        );

        walkCycle.AppendInterval(stepTime * 0.1f);

        // --- ËÅÂÀß ÍÎÃÀ ØÀÃÀÅÒ ---
        walkCycle.Append(
            legLeft.DOLocalMove(new Vector3(
                leftHome.x,
                leftHome.y + liftHeight,
                leftHome.z + stepDistance),
                stepTime)
            .SetEase(Ease.OutSine)
        );

        walkCycle.Join(
            legRight.DOLocalMoveZ(rightHome.z - (stepDistance * 0.2f), stepTime)
                .SetEase(Ease.OutSine)
        );

        walkCycle.Append(
            legLeft.DOLocalMoveY(leftHome.y, stepTime * 0.3f)
                .SetEase(Ease.InSine)
        );

        // --- ÂÎÇÂÐÀÒ Â ÍÅÉÒÐÀËÜÍÎÅ ÏÎËÎÆÅÍÈÅ ---
        walkCycle.Append(
            legLeft.DOLocalMoveZ(leftHome.z, stepTime * 0.2f)
                .SetEase(Ease.InOutSine)
        );

        walkCycle.Join(
            legRight.DOLocalMoveZ(rightHome.z, stepTime * 0.2f)
                .SetEase(Ease.InOutSine)
        );

        walkCycle.SetLoops(-1, LoopType.Restart);
    }

    // Äâà ìåòîäà äëÿ ñîâìåñòèìîñòè
    public void StopWalking() => StopWalk();

    public void StopWalk()
    {
        if (walkCycle != null && walkCycle.IsActive())
            walkCycle.Kill();

        legLeft.localPosition = leftHome;
        legRight.localPosition = rightHome;
    }
}