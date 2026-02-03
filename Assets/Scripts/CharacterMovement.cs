using DG.Tweening;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("Ссылки")]
    public CharacterLegsAnimation legsAnimation;

    [Header("Настройки движения")]
    public float moveSpeed = 0.5f;
    public Ease moveEase = Ease.Linear;
    public float rotationSpeed = 5f; // Скорость поворота

    // Текущее движение
    private Tween movementTween;

    /// <summary>
    /// Начинает движение персонажа по дорожке (ОДИН метод!)
    /// </summary>
    public void MoveAlongPath(Transform startPoint, Transform endPoint)
    {
        // 1. Ставим персонажа в начало дорожки
        transform.position = startPoint.position;

        // Сразу поворачиваем в нужном направлении
        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // 2. Запускаем анимацию ног
        if (legsAnimation != null)
            legsAnimation.StartWalkingAnimation();

        // 3. Рассчитываем длительность движения
        float distance = Vector3.Distance(startPoint.position, endPoint.position);
        float duration = distance / moveSpeed;

        // 4. Движемся к конечной точке через DOTween
        movementTween = transform.DOMove(endPoint.position, duration)
            .SetEase(moveEase)
            .OnUpdate(() => {
                // Плавный поворот во время движения
                SmoothRotation(endPoint.position);
            })
            .OnComplete(() => {
                // Останавливаем анимацию ног при прибытии
                if (legsAnimation != null)
                    legsAnimation.StopWalk();

                // Вызываем событие завершения движения
                OnPathCompleted?.Invoke();
            });
    }

    /// <summary>
    /// Плавный поворот во время движения
    /// </summary>
    void SmoothRotation(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero && direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    /// <summary>
    /// Останавливает движение персонажа
    /// </summary>
    public void StopMovement()
    {
        if (movementTween != null && movementTween.IsActive())
        {
            movementTween.Kill();
        }

        if (legsAnimation != null)
            legsAnimation.StopWalk();
    }

    // Событие завершения пути
    public System.Action OnPathCompleted;

}