using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public Team Team; 
    public bool IsKing; 

    public Cell Cell { get; set; }

    public event System.Action OnMoveEndCallback;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cell?.OnPointerEnter(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Cell?.OnPointerClick(eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Cell?.OnPointerExit(eventData);
    }

    public void Move(Cell targetCell)
    {
        if (targetCell == null)
        {
            Debug.LogWarning("Unit.Move: targetCell is null!");
            return;
        }

        if (Cell != null)
        {
            Cell.Unit = null;
        }

        Cell = targetCell;
        targetCell.Unit = this;

        StartCoroutine(MoveToPosition(targetCell.transform.position));
    }

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

        transform.position = targetPosition;

        OnMoveEndCallback?.Invoke();
    }
}
