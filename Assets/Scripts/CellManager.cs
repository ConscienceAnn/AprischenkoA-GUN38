using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CellManager : MonoBehaviour
{
    // Событие для подписки извне (например, GameInstaller)
    public UnityEvent<Cell> OnCellClicked = new();

    private List<Cell> _cells = new();
    private List<Unit> _units = new();

    private void Start()
    {
        // 1. Найти все клетки и юниты
        _cells.AddRange(FindObjectsByType<Cell>(FindObjectsSortMode.None));
        _units.AddRange(FindObjectsByType<Unit>(FindObjectsSortMode.None));

        // 2. Подписка на клики по клеткам
        foreach (var cell in _cells)
        {
            cell.OnPointerClickEvent += OnCellClicked.Invoke;
        }

        // 3. Связать юнитов с клетками
        foreach (var unit in _units)
        {
            Cell cell = FindCellAtPosition(unit.transform.position);
            if (cell != null)
            {
                unit.Cell = cell;
                cell.Unit = unit;
            }
        }

        // 4. Построить соседей для каждой клетки
        BuildNeighbours();
    }

    // Находит клетку по позиции (с небольшой погрешностью)
    private Cell FindCellAtPosition(Vector3 position)
    {
        foreach (var cell in _cells)
        {
            if (Vector3.Distance(cell.transform.position, position) < 0.1f)
                return cell;
        }
        return null;
    }

    // Строит соседей для всех клеток
    private void BuildNeighbours()
    {
        foreach (var cell in _cells)
        {
            Vector3 cellPos = cell.transform.position;

            foreach (var other in _cells)
            {
                if (other == cell) continue;

                Vector3 otherPos = other.transform.position;
                Vector3 delta = otherPos - cellPos;

                // Пока шаг 1, но это не точно
                if (Mathf.Abs(delta.magnitude - 1f) < 0.1f) // сосед на расстоянии ~1
                {
                    NeighbourType type = NeighbourType.None;

                    if (Mathf.Abs(delta.x - 1f) < 0.1f) type = NeighbourType.Right;
                    else if (Mathf.Abs(delta.x + 1f) < 0.1f) type = NeighbourType.Left;
                    else if (Mathf.Abs(delta.z - 1f) < 0.1f) type = NeighbourType.Top;    
                    else if (Mathf.Abs(delta.z + 1f) < 0.1f) type = NeighbourType.Bottom;

                    if (type != NeighbourType.None)
                    {
                        cell.Neighbours[type] = other;
                    }
                }
            }
        }
    }
}

