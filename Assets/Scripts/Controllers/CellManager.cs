using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CellManager : MonoBehaviour
{
    // Событие для подписки извне (например, GameInstaller)
    public UnityEvent<Cell> OnCellClicked = new();

    private List<Cell> _cells = new();
    private List<Unit> _units = new();

    [SerializeField] private GameObject unitPrefab;

    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material grayMaterial;
    private void Start()
    {

        Debug.Log("1 - CellManager Start");

        _cells.AddRange(FindObjectsByType<Cell>(FindObjectsSortMode.None));

        Debug.Log($"2 - Found {_cells.Count} cells");


        _units.AddRange(FindObjectsByType<Unit>(FindObjectsSortMode.None));

        Debug.Log($"3 - Found {_units.Count} units");


        // Шаг 4: Подписка на события 
         Debug.Log("4 - Subscribing to events");
          foreach (var cell in _cells)
         {
             cell.OnPointerClickEvent += OnCellClicked.Invoke;
        }

        //Передаём материалы как аргументы
        Debug.Log("5 - Before SetupCheckers");
        SetupCheckers(blueMaterial, grayMaterial);
        Debug.Log("6 - After SetupCheckers");


        Debug.Log("7 - Before linking existing units");
        foreach (var unit in _units)
        {
            Cell cell = FindCellAtPosition(unit.transform.position);
            if (cell != null)
            {
                unit.Cell = cell;
                cell.Unit = unit;
            }
        }

        Debug.Log("8 - After linking existing units");

        // Шаг 7: BuildNeighbours 
        // Debug.Log("9 - Before BuildNeighbours");
        // BuildNeighbours();
        // Debug.Log("10 - After BuildNeighbours");

        Debug.Log("11 - CellManager Start completed");
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

    private void SpawnUnit(Cell cell, Team team, Material mat)
    {

        Debug.Log($"Spawning unit for team {team} at position {cell.transform.position}");

        // Поднимаем позицию юнита над клеткой
        //Vector3 unitPosition = cell.transform.position + Vector3.up * 1f;


        // Создаём копию префаба юнита
        GameObject unitObj = Instantiate(unitPrefab, cell.transform.position, Quaternion.identity);
        unitObj.transform.position = cell.transform.position + Vector3.up * 1f;

        // Находим компонент Unit
        Unit unit = unitObj.GetComponent<Unit>();
        unit.Team = team; // запоминаем команду

        // Меняем цвет
        unitObj.GetComponent<MeshRenderer>().material = mat;

        // Связываем юнит и клетку
        unit.Cell = cell;
        cell.Unit = unit;
        _units.Add(unit);

        Debug.Log($"Unit spawned successfully");
    }


    public void SetupCheckers(Material blueMaterial, Material grayMaterial)
    {
        Debug.Log("SetupCheckers started");
        int grayUnits = 0;
        int blueUnits = 0;

        foreach (var cell in _cells)
        {
            Vector3 worldPos = cell.transform.position;
            int x = Mathf.RoundToInt((worldPos.x + 14f) / 2f);
            int y = Mathf.RoundToInt(worldPos.z / 2f);

            // Получаем материал клетки чтобы определить цвет
            MeshRenderer cellRenderer = cell.GetComponent<MeshRenderer>();
            if (cellRenderer == null) continue;

            Material cellMaterial = cellRenderer.material;

            // Проверяем является ли клетка черной
            bool isBlackCell = cellMaterial.name.Contains("BlackMaterial");

            if (isBlackCell)
            {
                // Игрок 1 (серые) - нижние 3 ряда: y = 0, 1, 2
                if (y <= 2)
                {
                    SpawnUnit(cell, Team.Player1, grayMaterial);
                    grayUnits++;
                    Debug.Log($"Spawned GRAY at ({x}, {y})");
                }
                // Игрок 2 (синие) - верхние 3 ряда: y = 5, 6, 7
                else if (y >= 5)
                {
                    SpawnUnit(cell, Team.Player2, blueMaterial);
                    blueUnits++;
                    Debug.Log($"Spawned BLUE at ({x}, {y})");
                }
            }
        }

        Debug.Log($"SetupCheckers completed: {grayUnits} gray, {blueUnits} blue units");

    }


}

