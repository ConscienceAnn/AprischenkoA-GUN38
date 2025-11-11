using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CellManager : MonoBehaviour
{
    public UnityEvent<Cell> OnCellClicked = new();

    private List<Cell> _cells = new();
    private List<Unit> _units = new();

    [SerializeField] private GameObject unitPrefab;

    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material grayMaterial;
    private void Start()
    {
        _cells.AddRange(FindObjectsByType<Cell>(FindObjectsSortMode.None));

        DebugCoordinates();

        _units.AddRange(FindObjectsByType<Unit>(FindObjectsSortMode.None));

          foreach (var cell in _cells)
         {
             cell.OnPointerClickEvent += OnCellClicked.Invoke;
        }

        SetupCheckers(blueMaterial, grayMaterial);

        foreach (var unit in _units)
        {
            Cell cell = FindCellAtPosition(unit.transform.position);
            if (cell != null)
            {
                unit.Cell = cell;
                cell.Unit = unit;
            }
        }

         BuildNeighbours();
    }


    private Cell FindCellAtPosition(Vector3 position)
    {
        foreach (var cell in _cells)
        {
            if (Vector3.Distance(cell.transform.position, position) < 0.1f)
                return cell;
        }
        return null;
    }


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


                if (Mathf.Abs(delta.magnitude - 1f) < 0.1f)
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

        GameObject unitObj = Instantiate(unitPrefab, cell.transform.position, Quaternion.identity);
        unitObj.transform.position = cell.transform.position + Vector3.up * 1f;


        Unit unit = unitObj.GetComponent<Unit>();
        unit.Team = team; 

        unitObj.GetComponent<MeshRenderer>().material = mat;

        unit.Cell = cell;
        cell.Unit = unit;
        _units.Add(unit);
    }


    public void SetupCheckers(Material blueMaterial, Material grayMaterial)
    {
        int grayUnits = 0;
        int blueUnits = 0;

        foreach (var cell in _cells)
        {
            Vector3 worldPos = cell.transform.position;
            Vector2Int coords = WorldToBoardCoords(worldPos);
            int x = coords.x;
            int y = coords.y;

            MeshRenderer cellRenderer = cell.GetComponent<MeshRenderer>();
            if (cellRenderer == null) continue;

            bool isBlackCell = cellRenderer.material.name.Contains("BlackMaterial");

            if (isBlackCell)
            {
                Debug.Log($"BLACK Cell at world({worldPos.x}, {worldPos.z}) -> indices({x}, {y})");

                // Player1 (серые) Ч левые 3 столбца: x = 0, 1, 2
                if (x <= 2)
                {
                    SpawnUnit(cell, Team.Player1, grayMaterial);
                    grayUnits++;
                }
                // Player2 (синие) Ч правые 3 столбца: x = 5, 6, 7
                else if (x >= 5)
                {
                    SpawnUnit(cell, Team.Player2, blueMaterial);
                    blueUnits++;
                }
            }
        }
    }


    private void DebugCoordinates()
    {
        int blackCellsLeft = 0;
        int blackCellsRight = 0;

        foreach (var cell in _cells)
        {
            Vector3 worldPos = cell.transform.position;
            int x = Mathf.RoundToInt((worldPos.x + 14f) / 2f);
            int y = Mathf.RoundToInt(worldPos.z / 2f);

            MeshRenderer renderer = cell.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            bool isBlack = renderer.material.name.Contains("BlackMaterial");

            if (isBlack)
            {
                if (x <= 2) blackCellsLeft++;
                if (x >= 5) blackCellsRight++;
            }
        }
    }


    public Vector2Int WorldToBoardCoords(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - 1f) / 2f);
        int y = Mathf.RoundToInt(worldPos.z / 2f);
        return new Vector2Int(x, y);
    }

}

