using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public Team Team; //new
    public bool IsKing; //new


    // Ссылка на клетку, на которой сейчас стоит юнит
    public Cell Cell { get; set; }

    // Событие, вызываемое после завершения перемещения
    public event System.Action OnMoveEndCallback;

    // Прокидываем событие наведения в клетку
    public void OnPointerEnter(PointerEventData eventData)
    {
        Cell?.OnPointerEnter(eventData);
    }

    // Прокидываем клик в клетку
    public void OnPointerClick(PointerEventData eventData)
    {
        Cell?.OnPointerClick(eventData);
    }

    // Прокидываем уход курсора в клетку
    public void OnPointerExit(PointerEventData eventData)
    {
        Cell?.OnPointerExit(eventData);
    }

    // Перемещает юнита на указанную клетку
    public void Move(Cell targetCell)
    {
        if (targetCell == null)
        {
            Debug.LogWarning("Unit.Move: targetCell is null!");
            return;
        }

        // Отвязываем юнита от старой клетки (если была)
        if (Cell != null)
        {
            Cell.Unit = null;
        }

        // Привязываем к новой клетке
        Cell = targetCell;
        targetCell.Unit = this;

        // Запускаем плавное перемещение
        StartCoroutine(MoveToPosition(targetCell.transform.position));
    }

    // Вспомогательный корутин для плавного перемещения
    private System.Collections.IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float duration = 0.3f; // время перемещения в секундах
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Гарантируем точное попадание в конечную позицию
        transform.position = targetPosition;

        // Вызываем событие завершения движения
        OnMoveEndCallback?.Invoke();
    }
}
